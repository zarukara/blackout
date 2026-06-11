using CombatSystem;
using SceneSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace UISystem
{
    public class GameOverView : MonoBehaviour
    {
        private Health playerHealth;
        private UiStateController uiStateController;

        public void Initialize(Health playerHealth, UiStateController uiStateController)
        {
            this.playerHealth = playerHealth;
            this.uiStateController = uiStateController;

            this.playerHealth.Died += ShowDeathScreen;
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
                playerHealth.Died -= ShowDeathScreen;
        }

        private void Update()
        {
            if (uiStateController == null)
                return;

            if (!uiStateController.IsState(GameUiState.Death))
                return;

            if (Keyboard.current == null)
                return;

            if (Keyboard.current.rKey.wasPressedThisFrame)
                RestartScene();

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
                ExitToMainMenu();
        }

        private void ShowDeathScreen()
        {
            uiStateController.OpenDeath();
            ClearSelectedButton();
        }

        private void RestartScene()
        {
            ClearSelectedButton();
            SceneLoader.ReloadCurrentScene();
        }

        private void ExitToMainMenu()
        {
            ClearSelectedButton();
            SceneLoader.LoadMainMenu();
        }

        private void ClearSelectedButton()
        {
            if (EventSystem.current == null)
                return;

            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}