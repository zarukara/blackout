using UnityEngine;
using WeaponSystem;

namespace PlayerSystem
{
    [DisallowMultipleComponent]
    public class PlayerAnimationController : MonoBehaviour
    {
        private static readonly int MoveXHash = Animator.StringToHash("MoveX");
        private static readonly int MoveYHash = Animator.StringToHash("MoveY");
        private static readonly int ClawAttackHash = Animator.StringToHash("ClawAttack");

        [Header("References")]
        [SerializeField] private Animator animator;

        [Header("Locomotion")]
        [SerializeField] private float dampTime = 0.1f;
        [SerializeField] private float movementThreshold = 0.01f;

        private PlayerInputReader inputReader;
        private PlayerWeaponController weaponController;
        private Camera mainCamera;

        public void Initialize(
            PlayerInputReader inputReader,
            Camera mainCamera,
            PlayerWeaponController weaponController)
        {
            if (this.inputReader != null)
                this.inputReader.AttackPressed -= OnAttackPressed;

            this.inputReader = inputReader;
            this.mainCamera = mainCamera;
            this.weaponController = weaponController;

            CacheReferences();

            if (this.inputReader != null)
                this.inputReader.AttackPressed += OnAttackPressed;
        }

        private void Awake()
        {
            CacheReferences();
        }

        private void OnValidate()
        {
            CacheReferences();
        }

        private void LateUpdate()
        {
            if (inputReader == null || mainCamera == null || animator == null)
                return;

            UpdateLocomotion();
        }

        private void OnDestroy()
        {
            if (inputReader != null)
                inputReader.AttackPressed -= OnAttackPressed;
        }

        [ContextMenu("Cache References")]
        public void CacheReferences()
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>(true);
        }

        private void UpdateLocomotion()
        {
            Vector2 moveInput = inputReader.MoveInput;

            if (moveInput.sqrMagnitude <= movementThreshold * movementThreshold)
            {
                SetMovementParameters(0f, 0f);
                return;
            }

            Vector3 cameraForward = mainCamera.transform.forward;
            Vector3 cameraRight = mainCamera.transform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;

            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 worldMoveDirection =
                cameraRight * moveInput.x +
                cameraForward * moveInput.y;

            worldMoveDirection = Vector3.ClampMagnitude(
                worldMoveDirection,
                1f
            );

            Vector3 localMoveDirection =
                transform.InverseTransformDirection(worldMoveDirection);

            SetMovementParameters(
                localMoveDirection.x,
                localMoveDirection.z
            );
        }

        private void SetMovementParameters(float moveX, float moveY)
        {
            animator.SetFloat(
                MoveXHash,
                moveX,
                dampTime,
                Time.deltaTime
            );

            animator.SetFloat(
                MoveYHash,
                moveY,
                dampTime,
                Time.deltaTime
            );
        }

        private void OnAttackPressed()
        {
            if (animator == null || weaponController == null)
                return;

            if (weaponController.CurrentWeaponType != WeaponType.Claws)
                return;

            animator.SetTrigger(ClawAttackHash);
        }
    }
}