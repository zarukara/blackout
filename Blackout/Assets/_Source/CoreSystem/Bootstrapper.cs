using CameraSystem;
using CombatSystem;
using PlayerSystem;
using UISystem;
using UnityEngine;
using WeaponSystem;

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
        [SerializeField] private PlayerDashController playerDashController;
        [SerializeField] private PlayerWeaponCollector playerWeaponCollector;
        [SerializeField] private PlayerWeaponController playerWeaponController;

        [Header("Camera References")]
        [SerializeField] private CameraRotationController cameraRotationController;

        [Header("UI References")]
        [SerializeField] private PlayerHealthView playerHealthView;
        [SerializeField] private GameOverView gameOverView;
        [SerializeField] private PauseMenuView pauseMenuView;
        [SerializeField] private WeaponWheelView weaponWheelView;

        private void Awake()
        {
            playerMovement.Initialize(inputReader, mainCamera);
            playerRotation.Initialize(inputReader, mainCamera);
            playerGrabController.Initialize(inputReader);
            playerMeleeAttack.Initialize(playerGrabController);
            playerDashController.Initialize(inputReader, playerGrabController, mainCamera);

            playerWeaponController.Initialize(inputReader, playerWeaponCollector);

            cameraRotationController.Initialize(inputReader);

            playerHealthView.Initialize(playerHealth);
            gameOverView.Initialize(playerHealth);
            pauseMenuView.Initialize(inputReader, playerHealth);
            weaponWheelView.Initialize(inputReader, playerWeaponCollector, playerWeaponController);
        }
    }
}