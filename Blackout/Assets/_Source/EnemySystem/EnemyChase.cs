using UnityEngine;

namespace EnemySystem
{
    [RequireComponent(typeof(CharacterController))]
    public class EnemyChase : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform target;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float stoppingDistance = 1.5f;
        [SerializeField] private float rotationSpeed = 12f;
        [SerializeField] private float gravity = -20f;

        private CharacterController characterController;
        private Vector3 verticalVelocity;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            ChaseTarget();
        }

        private void ChaseTarget()
        {
            if (target == null)
                return;

            Vector3 direction = target.position - transform.position;
            direction.y = 0f;

            float distance = direction.magnitude;

            if (distance > stoppingDistance)
            {
                Vector3 moveDirection = direction.normalized;
                characterController.Move(moveDirection * (moveSpeed * Time.deltaTime));
            }

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            ApplyGravity();
        }

        private void ApplyGravity()
        {
            if (characterController.isGrounded && verticalVelocity.y < 0f)
            {
                verticalVelocity.y = -2f;
            }

            verticalVelocity.y += gravity * Time.deltaTime;
            characterController.Move(verticalVelocity * Time.deltaTime);
        }
    }
}