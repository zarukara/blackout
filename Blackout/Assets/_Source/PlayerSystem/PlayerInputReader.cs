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

        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }

        public event Action AttackPressed;
        public event Action GrabPressed;

        private void OnEnable()
        {
            moveAction.action.Enable();
            lookAction.action.Enable();
            attackAction.action.Enable();
            grabAction.action.Enable();

            attackAction.action.performed += OnAttackPerformed;
            grabAction.action.performed += OnGrabPerformed;
        }

        private void OnDisable()
        {
            attackAction.action.performed -= OnAttackPerformed;
            grabAction.action.performed -= OnGrabPerformed;

            moveAction.action.Disable();
            lookAction.action.Disable();
            attackAction.action.Disable();
            grabAction.action.Disable();
        }

        private void Update()
        {
            MoveInput = moveAction.action.ReadValue<Vector2>();
            LookInput = lookAction.action.ReadValue<Vector2>();
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            AttackPressed?.Invoke();
        }

        private void OnGrabPerformed(InputAction.CallbackContext context)
        {
            GrabPressed?.Invoke();
        }
    }
}