using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerSystem
{
    public class PlayerInputReader : MonoBehaviour
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference lookAction;
        [SerializeField] private InputActionReference attackAction;
        [SerializeField] private InputActionReference grabAction;
        [SerializeField] private InputActionReference cameraLeftAction;
        [SerializeField] private InputActionReference cameraRightAction;
        [SerializeField] private InputActionReference dashAction;
        [SerializeField] private InputActionReference pauseAction;

        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }

        public event Action AttackPressed;
        public event Action GrabPressed;
        public event Action CameraLeftPressed;
        public event Action CameraRightPressed;
        public event Action DashPressed;
        public event Action PausePressed;

        private void OnEnable()
        {
            EnableActions();
            SubscribeActions();
        }

        private void OnDisable()
        {
            UnsubscribeActions();
            DisableActions();
        }

        private void Update()
        {
            MoveInput = moveAction.action.ReadValue<Vector2>();
            LookInput = lookAction.action.ReadValue<Vector2>();
        }

        private void EnableActions()
        {
            moveAction.action.Enable();
            lookAction.action.Enable();
            attackAction.action.Enable();
            grabAction.action.Enable();
            cameraLeftAction.action.Enable();
            cameraRightAction.action.Enable();
            dashAction.action.Enable();
            pauseAction.action.Enable();
        }

        private void DisableActions()
        {
            moveAction.action.Disable();
            lookAction.action.Disable();
            attackAction.action.Disable();
            grabAction.action.Disable();
            cameraLeftAction.action.Disable();
            cameraRightAction.action.Disable();
            dashAction.action.Disable();
            pauseAction.action.Disable();
        }

        private void SubscribeActions()
        {
            attackAction.action.performed += OnAttackPerformed;
            grabAction.action.performed += OnGrabPerformed;
            cameraLeftAction.action.performed += OnCameraLeftPerformed;
            cameraRightAction.action.performed += OnCameraRightPerformed;
            dashAction.action.performed += OnDashPerformed;
            pauseAction.action.performed += OnPausePerformed;
        }

        private void UnsubscribeActions()
        {
            attackAction.action.performed -= OnAttackPerformed;
            grabAction.action.performed -= OnGrabPerformed;
            cameraLeftAction.action.performed -= OnCameraLeftPerformed;
            cameraRightAction.action.performed -= OnCameraRightPerformed;
            dashAction.action.performed -= OnDashPerformed;
            pauseAction.action.performed -= OnPausePerformed;
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            AttackPressed?.Invoke();
        }

        private void OnGrabPerformed(InputAction.CallbackContext context)
        {
            GrabPressed?.Invoke();
        }

        private void OnCameraLeftPerformed(InputAction.CallbackContext context)
        {
            CameraLeftPressed?.Invoke();
        }

        private void OnCameraRightPerformed(InputAction.CallbackContext context)
        {
            CameraRightPressed?.Invoke();
        }

        private void OnDashPerformed(InputAction.CallbackContext context)
        {
            DashPressed?.Invoke();
        }

        private void OnPausePerformed(InputAction.CallbackContext context)
        {
            PausePressed?.Invoke();
        }
    }
}