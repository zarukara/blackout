using EnemySystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerSystem
{
    [DisallowMultipleComponent]
    public class PlayerTargetingController : MonoBehaviour
    {
        [Header("Targeting")]
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private float raycastDistance = 1000f;

        [Header("Fallback Aim")]
        [SerializeField] private float aimPlaneHeight = 0f;

        [Header("Debug")]
        [SerializeField] private bool drawDebug = true;

        private Camera mainCamera;
        private TargetableEnemy currentTarget;

        public TargetableEnemy CurrentTarget => currentTarget;
        public bool HasTarget => currentTarget != null && currentTarget.IsAvailable;

        public void Initialize(Camera mainCamera)
        {
            this.mainCamera = mainCamera;
            UpdateTargetUnderCursor();
        }

        private void Update()
        {
            UpdateTargetUnderCursor();
        }

        public Vector3 GetShootDirectionFrom(Vector3 origin, Vector3 fallbackDirection)
        {
            if (HasTarget)
            {
                Vector3 targetPoint = currentTarget.GetAimPoint();
                return GetDirectionToPoint(origin, targetPoint, fallbackDirection);
            }

            if (TryGetCursorWorldPoint(out Vector3 cursorPoint))
                return GetDirectionToPoint(origin, cursorPoint, fallbackDirection);

            return GetHorizontalDirection(fallbackDirection);
        }

        private void UpdateTargetUnderCursor()
        {
            if (mainCamera == null || Mouse.current == null)
            {
                SetCurrentTarget(null);
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!Physics.Raycast(ray, out RaycastHit hit, raycastDistance, targetLayer))
            {
                SetCurrentTarget(null);
                return;
            }

            TargetableEnemy targetableEnemy = hit.collider.GetComponentInParent<TargetableEnemy>();

            if (targetableEnemy == null || !targetableEnemy.IsAvailable)
            {
                SetCurrentTarget(null);
                return;
            }

            SetCurrentTarget(targetableEnemy);
        }

        private void SetCurrentTarget(TargetableEnemy newTarget)
        {
            if (currentTarget == newTarget)
                return;

            if (currentTarget != null)
                currentTarget.SetTargeted(false);

            currentTarget = newTarget;

            if (currentTarget != null)
                currentTarget.SetTargeted(true);
        }

        private bool TryGetCursorWorldPoint(out Vector3 point)
        {
            point = Vector3.zero;

            if (mainCamera == null || Mouse.current == null)
                return false;

            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            Plane aimPlane = new Plane(
                Vector3.up,
                new Vector3(0f, aimPlaneHeight, 0f)
            );

            if (!aimPlane.Raycast(ray, out float enter))
                return false;

            point = ray.GetPoint(enter);
            return true;
        }

        private Vector3 GetDirectionToPoint(Vector3 origin, Vector3 targetPoint, Vector3 fallbackDirection)
        {
            Vector3 direction = targetPoint - origin;
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.001f)
                return GetHorizontalDirection(fallbackDirection);

            if (drawDebug)
                Debug.DrawLine(origin, origin + direction.normalized * 10f, Color.red, 0.05f);

            return direction.normalized;
        }

        private Vector3 GetHorizontalDirection(Vector3 direction)
        {
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.001f)
                return transform.forward;

            return direction.normalized;
        }
    }
}