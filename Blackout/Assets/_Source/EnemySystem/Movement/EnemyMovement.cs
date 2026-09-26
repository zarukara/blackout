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

        public float StoppingDistance => stoppingDistance;

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
            navMeshAgent.ResetPath();
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