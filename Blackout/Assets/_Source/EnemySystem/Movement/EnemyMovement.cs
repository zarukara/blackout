using UnityEngine;

namespace EnemySystem
{
    [RequireComponent(typeof(CharacterController))]
    public class EnemyMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float stoppingDistance = 1.5f;
        [SerializeField] private float rotationSpeed = 12f;
        [SerializeField] private float gravity = -20f;

        private CharacterController characterController;
        private Vector3 verticalVelocity;

        public float StoppingDistance => stoppingDistance;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        public void MoveToTarget(Transform target)
        {
            if (target == null || characterController == null || !characterController.enabled)
                return;

            Vector3 direction = target.position - transform.position;
            direction.y = 0f;

            float distance = direction.magnitude;

            if (distance > stoppingDistance)
            {
                Vector3 moveDirection = direction.normalized;
                characterController.Move(moveDirection * (moveSpeed * Time.deltaTime));
            }

            RotateToDirection(direction);
            ApplyGravity();
        }

        public void RotateToTarget(Transform target)
        {
            if (target == null)
                return;

            Vector3 direction = target.position - transform.position;
            direction.y = 0f;

            RotateToDirection(direction);
        }

        public void ApplyGravity()
        {
            if (characterController == null || !characterController.enabled)
                return;

            if (characterController.isGrounded && verticalVelocity.y < 0f)
                verticalVelocity.y = -2f;

            verticalVelocity.y += gravity * Time.deltaTime;
            characterController.Move(verticalVelocity * Time.deltaTime);
        }

        public bool IsTargetInsideStoppingDistance(Transform target)
        {
            if (target == null)
                return false;

            Vector3 direction = target.position - transform.position;
            direction.y = 0f;

            return direction.magnitude <= stoppingDistance;
        }

        public void DisableController()
        {
            if (characterController != null)
                characterController.enabled = false;
        }

        public void EnableController()
        {
            if (characterController != null)
                characterController.enabled = true;
        }

        private void RotateToDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude <= 0.001f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }
}