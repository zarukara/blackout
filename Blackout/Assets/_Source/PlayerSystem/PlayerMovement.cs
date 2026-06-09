using UnityEngine;

namespace PlayerSystem
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float gravity = -20f;

        private PlayerInputReader inputReader;
        private CharacterController characterController;
        private Vector3 verticalVelocity;

        public void Initialize(PlayerInputReader inputReader)
        {
            this.inputReader = inputReader;
        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (inputReader == null)
                return;

            Move();
        }

        private void Move()
        {
            Vector2 input = inputReader.MoveInput;

            Vector3 moveDirection = new Vector3(input.x, 0f, input.y);
            moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

            if (characterController.isGrounded && verticalVelocity.y < 0f)
            {
                verticalVelocity.y = -2f;
            }

            verticalVelocity.y += gravity * Time.deltaTime;

            Vector3 finalMove = moveDirection * moveSpeed + verticalVelocity;
            characterController.Move(finalMove * Time.deltaTime);
        }
    }
}