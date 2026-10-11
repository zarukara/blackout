using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace EnemySystem
{
    [DisallowMultipleComponent]
    public sealed class MeleeApproach : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float refreshInterval = 0.3f;
        [SerializeField, Min(2f)] private float waitingDistance = 2.7f;
        [SerializeField, Min(0.5f)] private float personalSpace = 1.25f;
        [SerializeField, Min(0.1f)] private float movingSpeed = 0.8f;
        [SerializeField, Min(0f)] private float stationarySpeed = 0.3f;
        [SerializeField, Min(0f)] private float predictionTime = 0.15f;

        private static readonly List<MeleeApproach> neighbours = new();
        private static readonly float[] candidateAngles = { 0f, 22f, -22f, 44f, -44f, 70f, -70f };
        private EnemyMovement movement;
        private EnemyMeleeAttack attack;
        private NavMeshAgent agent;
        private NavMeshPath path;
        private Transform target;
        private CharacterController targetBody;
        private Vector3 lastTargetPosition;
        private Vector3 velocity;
        private Vector3 plannedTargetPosition;
        private float nextRefresh;
        private float stoppedSince;
        private bool hadPermission;
        private bool hasDestination;

        public Vector3 Destination { get; private set; }
        public bool IsPursuingMovingTarget { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry() => neighbours.Clear();

        private void Awake()
        {
            movement = GetComponent<EnemyMovement>();
            attack = GetComponent<EnemyMeleeAttack>();
            agent = GetComponent<NavMeshAgent>();
            path = new NavMeshPath();
            if (agent != null)
            {
                agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
                agent.avoidancePriority = 35 + (int)((uint)GetInstanceID() % 30);
            }
        }

        private void OnEnable() { if (!neighbours.Contains(this)) neighbours.Add(this); }
        private void OnDisable() { neighbours.Remove(this); ResetApproach(); }

        public void ResetApproach()
        {
            target = null;
            hasDestination = false;
            nextRefresh = 0f;
            velocity = Vector3.zero;
            IsPursuingMovingTarget = false;
        }

        public void Tick(Transform newTarget, bool mayAttack)
        {
            if (newTarget == null || movement == null || !movement.IsReady) return;
            if (newTarget != target)
            {
                ResetApproach();
                target = newTarget;
                targetBody = target.GetComponentInParent<CharacterController>();
                lastTargetPosition = target.position;
            }

            Vector3 delta = Flat(target.position - lastTargetPosition);
            lastTargetPosition = target.position;
            Vector3 measured = delta / Mathf.Max(Time.deltaTime, 0.001f);
            velocity = Vector3.Lerp(velocity, Vector3.ClampMagnitude(measured, 12f), 1f - Mathf.Exp(-12f * Time.deltaTime));
            if (velocity.magnitude > movingSpeed)
            {
                if (!IsPursuingMovingTarget) nextRefresh = 0f;
                IsPursuingMovingTarget = true;
                stoppedSince = Time.time;
            }
            else if (velocity.magnitude > stationarySpeed) stoppedSince = Time.time;
            else if (IsPursuingMovingTarget && Time.time - stoppedSince > 0.35f)
            {
                IsPursuingMovingTarget = false;
                nextRefresh = 0f;
            }

            bool escaped = Flat(target.position - plannedTargetPosition).sqrMagnitude > 0.8f * 0.8f;
            if (Time.time >= nextRefresh || mayAttack != hadPermission || escaped)
            {
                nextRefresh = Time.time + refreshInterval;
                hadPermission = mayAttack;
                plannedTargetPosition = target.position;
                Vector3 separation = GetSeparation();
                if (IsPursuingMovingTarget || Flat(target.position - transform.position).sqrMagnitude > 25f)
                {
                    // Follow a moving player, with a small local offset rather than a formation.
                    Vector3 wanted = TargetGroundPosition() + velocity * predictionTime + separation;
                    if (NavMesh.SamplePosition(wanted, out NavMeshHit hit, 0.8f, agent.areaMask))
                    {
                        Destination = hit.position;
                        hasDestination = true;
                    }
                }
                else SelectApproachPoint(mayAttack ? attack.AttackDistance - 0.15f : waitingDistance);
            }

            if (hasDestination) movement.MoveToPosition(Destination, IsPursuingMovingTarget ? attack.AttackDistance * 0.75f : 0.12f);
            else movement.Stop();
        }

        private void SelectApproachPoint(float radius)
        {
            Vector3 away = Flat(transform.position - target.position).normalized;
            if (away.sqrMagnitude < 0.01f) away = -transform.forward;
            Vector3 best = Destination;
            float bestScore = float.PositiveInfinity;
            bool found = false;
            float bias = ((uint)GetInstanceID() % 17 - 8f) * 0.8f;
            for (int c = 0; c < candidateAngles.Length; c++)
            {
                Vector3 wanted = TargetGroundPosition() + Quaternion.Euler(0f, candidateAngles[c] + bias, 0f) * away * radius;
                if (!NavMesh.SamplePosition(wanted, out NavMeshHit hit, 0.65f, agent.areaMask) ||
                    Mathf.Abs(hit.position.y - transform.position.y) > 1.2f ||
                    !agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete) continue;

                float score = Flat(hit.position - transform.position).sqrMagnitude * 0.2f;
                if (hasDestination) score += Flat(hit.position - Destination).sqrMagnitude * 0.45f;
                for (int n = 0; n < neighbours.Count; n++)
                {
                    MeleeApproach other = neighbours[n];
                    if (other == this || other == null || other.attack == null || !other.attack.IsAvailable) continue;
                    float distance = Flat(hit.position - other.transform.position).magnitude;
                    if (other.hasDestination && other.target == target)
                        distance = Mathf.Min(distance, Flat(hit.position - other.Destination).magnitude);
                    score += Mathf.Pow(Mathf.Max(0f, personalSpace - distance), 2f) * 12f;
                }
                if (score >= bestScore) continue;
                bestScore = score;
                best = hit.position;
                found = true;
            }
            hasDestination = found;
            if (found) Destination = best;
        }

        private Vector3 GetSeparation()
        {
            Vector3 result = Vector3.zero;
            for (int i = 0; i < neighbours.Count; i++)
            {
                MeleeApproach other = neighbours[i];
                if (other == this || other == null || other.attack == null || !other.attack.IsAvailable) continue;
                Vector3 away = Flat(transform.position - other.transform.position);
                float distance = away.magnitude;
                if (distance > 0.01f && distance < personalSpace)
                    result += away / distance * (personalSpace - distance);
            }
            return Vector3.ClampMagnitude(result, 0.6f);
        }

        private static Vector3 Flat(Vector3 value) { value.y = 0f; return value; }

        private Vector3 TargetGroundPosition()
        {
            Vector3 position = target.position;
            if (targetBody != null) position.y = targetBody.bounds.min.y + 0.05f;
            return position;
        }
    }
}
