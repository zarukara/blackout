using UnityEngine;
using UnityEngine.AI;

namespace EnemySystem
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float stoppingDistance = 1.5f;
        [SerializeField] private float rotationSpeed = 12f;

        [Header("NavMesh")]
        [SerializeField] private float navMeshSearchRadius = 2f;

        private NavMeshAgent navMeshAgent;
        private readonly RaycastHit[] lungeHits = new RaycastHit[32];
        private Vector3 lastDestination;
        private float nextPathTime;
        private bool hasDestination;

        public float StoppingDistance => stoppingDistance;
        public bool IsReady => navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh;

        public void MoveToPosition(Vector3 position, float arrivalDistance)
        {
            if (!IsReady) return;
            navMeshAgent.stoppingDistance = arrivalDistance;
            navMeshAgent.isStopped = false;
            if (!hasDestination || (Time.time >= nextPathTime &&
                ((position - lastDestination).sqrMagnitude > 0.15f * 0.15f ||
                 (!navMeshAgent.pathPending && !navMeshAgent.hasPath))))
            {
                navMeshAgent.SetDestination(position);
                lastDestination = position;
                hasDestination = true;
                nextPathTime = Time.time + 0.2f;
            }
            RotateToDirection(navMeshAgent.desiredVelocity);
        }

        public void MoveAttack(Vector3 direction, float distance, Transform target, LayerMask obstacles)
        {
            if (!IsReady || distance <= 0f) return;
            Vector3 origin = navMeshAgent.nextPosition;
            // Preserve space even though player contact is ignored physically.
            if (target != null)
            {
                Vector3 toTarget = target.position - origin;
                toTarget.y = 0f;
                if (Vector3.Dot(direction, toTarget) > 0f)
                    distance = Mathf.Min(distance, Mathf.Max(0f, toTarget.magnitude - navMeshAgent.radius - 0.4f));
            }
            if (distance <= 0f) return;
            Vector3 end = origin + direction * distance;
            if (NavMesh.Raycast(origin, end, out NavMeshHit edge, navMeshAgent.areaMask))
                distance = Mathf.Min(distance, Mathf.Max(0f, Vector3.Distance(origin, edge.position) - 0.03f));

            float radius = navMeshAgent.radius;
            int count = Physics.CapsuleCastNonAlloc(origin + Vector3.up * (radius + 0.05f),
                origin + Vector3.up * Mathf.Max(radius + 0.05f, navMeshAgent.height - radius),
                radius, direction, lungeHits, distance + 0.03f, obstacles, QueryTriggerInteraction.Ignore);
            if (count == lungeHits.Length) return;
            for (int i = 0; i < count; i++)
            {
                Transform hit = lungeHits[i].transform;
                if (hit.IsChildOf(transform) || (target != null && hit.IsChildOf(target))) continue;
                distance = Mathf.Min(distance, Mathf.Max(0f, lungeHits[i].distance - 0.03f));
            }
            if (distance > 0f) navMeshAgent.Move(direction * distance);
        }

        private void Awake()
        {
            CacheReferences();
            ApplySettings();
        }

        private void OnValidate()
        {
            CacheReferences();
            ApplySettings();
        }

        public void MoveToTarget(Transform target)
        {
            if (target == null ||
                navMeshAgent == null ||
                !navMeshAgent.enabled ||
                !navMeshAgent.isOnNavMesh)
            {
                return;
            }

            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(target.position);

            Vector3 moveDirection = navMeshAgent.desiredVelocity;
            moveDirection.y = 0f;

            RotateToDirection(moveDirection);
        }

        public void RotateToTarget(Transform target)
        {
            if (target == null)
                return;

            Vector3 direction =
                target.position - transform.position;

            direction.y = 0f;

            RotateToDirection(direction);
        }

        public bool IsTargetInsideStoppingDistance(
            Transform target)
        {
            if (target == null)
                return false;

            Vector3 direction =
                target.position - transform.position;

            direction.y = 0f;

            return direction.sqrMagnitude <=
                   stoppingDistance * stoppingDistance;
        }

        public void Stop()
        {
            if (navMeshAgent == null ||
                !navMeshAgent.enabled ||
                !navMeshAgent.isOnNavMesh)
            {
                return;
            }

            navMeshAgent.isStopped = true;
            if (navMeshAgent.hasPath || navMeshAgent.pathPending) navMeshAgent.ResetPath();
            navMeshAgent.velocity = Vector3.zero;
            hasDestination = false;
        }

        public void DisableController()
        {
            if (navMeshAgent == null)
                return;

            if (navMeshAgent.enabled)
            {
                if (navMeshAgent.isOnNavMesh)
                {
                    navMeshAgent.isStopped = true;
                    navMeshAgent.ResetPath();
                }

                navMeshAgent.enabled = false;
            }
        }

        public void EnableController()
        {
            if (navMeshAgent == null)
                return;

            // State transitions during normal combat must never snap/warp the agent.
            if (IsReady)
                return;

            hasDestination = false;

            if (NavMesh.SamplePosition(
                    transform.position,
                    out NavMeshHit hit,
                    navMeshSearchRadius,
                    NavMesh.AllAreas))
            {
                transform.position = hit.position;

                if (!navMeshAgent.enabled)
                    navMeshAgent.enabled = true;

                if (navMeshAgent.isOnNavMesh)
                {
                    navMeshAgent.Warp(hit.position);
                    navMeshAgent.isStopped = false;
                }
            }
            else
            {
                Debug.LogWarning(
                    $"Could not place enemy on NavMesh: {name}",
                    this
                );
            }
        }

        private void CacheReferences()
        {
            if (navMeshAgent == null)
                navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void ApplySettings()
        {
            if (navMeshAgent == null)
                return;

            navMeshAgent.speed = moveSpeed;
            navMeshAgent.stoppingDistance =
                stoppingDistance;

            navMeshAgent.updateRotation = false;
            navMeshAgent.updateUpAxis = true;
        }

        private void RotateToDirection(
            Vector3 direction)
        {
            if (direction.sqrMagnitude <= 0.001f)
                return;

            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }
}
