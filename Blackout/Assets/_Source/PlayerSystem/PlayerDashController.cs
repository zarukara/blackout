using System.Collections;
using UnityEngine;

namespace PlayerSystem
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerDashController : MonoBehaviour
    {
        [Header("Dash")]
        [SerializeField] private float dashDistance = 4f;
        [SerializeField] private float dashDuration = 0.12f;
        [SerializeField] private float dashCooldown = 0.6f;

        private PlayerInputReader inputReader;
        private PlayerGrabController grabController;
        private Camera mainCamera;
        private CharacterController characterController;

        private bool isDashing;
        private float nextDashTime;

        public void Initialize(
            PlayerInputReader inputReader,
            PlayerGrabController grabController,
            Camera mainCamera)
        {
            this.inputReader = inputReader;
            this.grabController = grabController;
            this.mainCamera = mainCamera;

            this.inputReader.DashPressed += TryDash;
        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void OnDestroy()
        {
            if (inputReader != null)
            {
                inputReader.DashPressed -= TryDash;
            }
        }

        private void TryDash()
        {
            if (isDashing)
                return;

            if (Time.time < nextDashTime)
                return;

            if (grabController != null && grabController.HasGrabbedEnemy)
                return;

            Vector3 dashDirection = GetDashDirection();

            if (dashDirection.sqrMagnitude <= 0.001f)
                return;

            nextDashTime = Time.time + dashCooldown;
            StartCoroutine(DashRoutine(dashDirection));
        }

        private Vector3 GetDashDirection()
        {
            Vector2 moveInput = inputReader.MoveInput;

            if (moveInput.sqrMagnitude <= 0.001f)
            {
                return transform.forward;
            }

            Vector3 cameraForward = mainCamera.transform.forward;
            Vector3 cameraRight = mainCamera.transform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;

            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 direction = cameraRight * moveInput.x + cameraForward * moveInput.y;
            direction.y = 0f;

            return direction.normalized;
        }

        private IEnumerator DashRoutine(Vector3 direction)
        {
            isDashing = true;

            float elapsedTime = 0f;
            float dashSpeed = dashDistance / dashDuration;

            while (elapsedTime < dashDuration)
            {
                characterController.Move(direction * dashSpeed * Time.deltaTime);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            isDashing = false;
        }
    }
}