using CameraSystem;
using CombatSystem;
using PlayerSystem;
using UISystem;
using UnityEngine;

namespace CoreSystem
{
    public class Bootstrapper : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private Camera mainCamera;

        [Header("Player References")]
        [SerializeField] private Health playerHealth;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerRotation playerRotation;
        [SerializeField] private PlayerGrabController playerGrabController;
        [SerializeField] private PlayerMeleeAttack playerMeleeAttack;

        [Header("Camera References")]
        [SerializeField] private CameraRotationController cameraRotationController;

        [Header("UI References")]
        [SerializeField] private PlayerHealthView playerHealthView;
        [SerializeField] private GameOverView gameOverView;

        private void Awake()
        {
            playerMovement.Initialize(inputReader, mainCamera);
            playerRotation.Initialize(inputReader, mainCamera);
            playerGrabController.Initialize(inputReader);
            playerMeleeAttack.Initialize(inputReader, playerGrabController);

            cameraRotationController.Initialize(inputReader);

            playerHealthView.Initialize(playerHealth);
            gameOverView.Initialize(playerHealth);
        }
    }
}