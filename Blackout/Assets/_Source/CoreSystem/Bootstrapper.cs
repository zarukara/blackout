using CameraSystem;
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

        [Header("Player")]
        [SerializeField] private PlayerFacade player;

        [Header("UI")]
        [SerializeField] private UiFacade ui;

        [Header("Camera")]
        [SerializeField] private CameraRotationController cameraRotationController;

        private void Awake()
        {
            ResolveReferences();

            InitializePlayer();
            InitializeCamera();
            InitializeUI();
        }

        private void ResolveReferences()
        {
            ResolvePlayer();
            ResolveUI();
        }

        private void ResolvePlayer()
        {
            if (player == null)
                player = FindFirstObjectByType<PlayerFacade>();

            if (player != null)
                player.CacheReferences();
        }

        private void ResolveUI()
        {
            if (ui == null)
                ui = FindFirstObjectByType<UiFacade>();

            if (ui != null)
                ui.CacheReferences();
        }

        private void InitializePlayer()
        {
            if (inputReader == null)
            {
                Debug.LogError("PlayerInputReader is missing in Bootstrapper.", this);
                return;
            }

            if (mainCamera == null)
            {
                Debug.LogError("Main Camera is missing in Bootstrapper.", this);
                return;
            }

            if (player == null)
            {
                Debug.LogError("PlayerFacade was not found in scene.", this);
                return;
            }

            if (!player.IsValid())
            {
                Debug.LogError("PlayerFacade has missing references.", player);
                return;
            }

            player.TargetingController.Initialize(mainCamera);

            player.Movement.Initialize(inputReader, mainCamera);
            player.Rotation.Initialize(inputReader, mainCamera);
            player.GrabController.Initialize(inputReader);
            player.DashController.Initialize(inputReader, player.GrabController, mainCamera);
            player.WeaponController.Initialize(inputReader, player.WeaponCollector);
        }

        private void InitializeCamera()
        {
            if (cameraRotationController == null)
                return;

            cameraRotationController.Initialize(inputReader);
        }

        private void InitializeUI()
        {
            if (inputReader == null)
                return;

            if (player == null || !player.IsValid())
                return;

            if (ui == null)
            {
                Debug.LogError("UiFacade was not found in scene.", this);
                return;
            }

            if (!ui.IsValid())
            {
                Debug.LogError("UiFacade has missing references.", ui);
                return;
            }

            ui.Initialize(inputReader, player);
        }
    }
}