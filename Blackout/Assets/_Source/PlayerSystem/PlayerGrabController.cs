using EnemySystem;
using UnityEngine;

namespace PlayerSystem
{
    public class PlayerGrabController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform grabPoint;
        [SerializeField] private Transform holdPoint;

        [Header("Grab")]
        [SerializeField] private float grabRadius = 1.2f;
        [SerializeField] private LayerMask enemyLayer;

        [Header("Throw")]
        [SerializeField] private float throwForce = 12f;

        private PlayerInputReader inputReader;
        private EnemyGrabHandler grabbedEnemy;

        public EnemyGrabHandler GrabbedEnemy => grabbedEnemy;
        public bool HasGrabbedEnemy => grabbedEnemy != null;

        public void Initialize(PlayerInputReader inputReader)
        {
            this.inputReader = inputReader;
            this.inputReader.GrabPressed += HandleGrabInput;
        }

        private void OnDestroy()
        {
            if (inputReader != null)
                inputReader.GrabPressed -= HandleGrabInput;

            UnsubscribeFromGrabbedEnemy();
        }

        private void HandleGrabInput()
        {
            if (grabbedEnemy != null)
            {
                ThrowGrabbedEnemy();
                return;
            }

            TryGrab();
        }

        private void TryGrab()
        {
            Collider[] hits = Physics.OverlapSphere(
                grabPoint.position,
                grabRadius,
                enemyLayer
            );

            EnemyGrabHandler nearestEnemy = null;
            float nearestDistance = float.MaxValue;

            foreach (Collider hit in hits)
            {
                EnemyGrabHandler enemy = hit.GetComponentInParent<EnemyGrabHandler>();

                if (enemy == null || enemy.IsGrabbed || enemy.IsThrown)
                    continue;

                float distance = Vector3.Distance(transform.position, enemy.transform.position);

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemy;
                }
            }

            if (nearestEnemy == null)
                return;

            grabbedEnemy = nearestEnemy;
            grabbedEnemy.Health.Died += ClearGrabbedEnemy;
            grabbedEnemy.Grab(holdPoint);
        }

        private void ThrowGrabbedEnemy()
        {
            EnemyGrabHandler enemyToThrow = grabbedEnemy;

            UnsubscribeFromGrabbedEnemy();
            grabbedEnemy = null;

            enemyToThrow.Throw(transform.forward, throwForce);
        }

        private void ClearGrabbedEnemy()
        {
            UnsubscribeFromGrabbedEnemy();
            grabbedEnemy = null;
        }

        private void UnsubscribeFromGrabbedEnemy()
        {
            if (grabbedEnemy == null)
                return;

            grabbedEnemy.Health.Died -= ClearGrabbedEnemy;
        }

        private void OnDrawGizmosSelected()
        {
            if (grabPoint == null)
                return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(grabPoint.position, grabRadius);
        }
    }
}