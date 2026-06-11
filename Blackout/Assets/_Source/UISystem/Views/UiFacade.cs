using PlayerSystem;
using UnityEngine;

namespace UISystem
{
    [DisallowMultipleComponent]
    public class UiFacade : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private UiStateController stateController;

        [Header("Views")]
        [SerializeField] private PlayerHealthView playerHealthView;
        [SerializeField] private GameOverView gameOverView;
        [SerializeField] private PauseMenuView pauseMenuView;
        [SerializeField] private WeaponWheelView weaponWheelView;

        public UiStateController StateController => stateController;
        public PlayerHealthView PlayerHealthView => playerHealthView;
        public GameOverView GameOverView => gameOverView;
        public PauseMenuView PauseMenuView => pauseMenuView;
        public WeaponWheelView WeaponWheelView => weaponWheelView;

        private void Awake()
        {
            CacheReferences();
        }

        private void OnValidate()
        {
            CacheReferences();
        }

        [ContextMenu("Cache References")]
        public void CacheReferences()
        {
            if (stateController == null)
                stateController = GetComponentInChildren<UiStateController>(true);

            if (playerHealthView == null)
                playerHealthView = GetComponentInChildren<PlayerHealthView>(true);

            if (gameOverView == null)
                gameOverView = GetComponentInChildren<GameOverView>(true);

            if (pauseMenuView == null)
                pauseMenuView = GetComponentInChildren<PauseMenuView>(true);

            if (weaponWheelView == null)
                weaponWheelView = GetComponentInChildren<WeaponWheelView>(true);
        }

        public void Initialize(PlayerInputReader inputReader, PlayerFacade player)
        {
            CacheReferences();

            if (!IsValid())
            {
                Debug.LogError("UiFacade has missing references.", this);
                return;
            }

            stateController.Initialize();

            playerHealthView.Initialize(player.Health);
            gameOverView.Initialize(player.Health, stateController);
            pauseMenuView.Initialize(inputReader, stateController);

            weaponWheelView.Initialize(
                inputReader,
                player.WeaponCollector,
                player.WeaponController,
                stateController
            );
        }

        public bool IsValid()
        {
            return stateController != null
                   && playerHealthView != null
                   && gameOverView != null
                   && pauseMenuView != null
                   && weaponWheelView != null;
        }
    }
}