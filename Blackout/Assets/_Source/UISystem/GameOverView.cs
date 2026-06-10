using CombatSystem;
using SceneSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace UISystem
{
    public class GameOverView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject deathScreen;

        private Health playerHealth;
        private bool isGameOver;

        public void Initialize(Health playerHealth)
        {
            this.playerHealth = playerHealth;
            this.playerHealth.Died += ShowDeathScreen;

            if (deathScreen != null)
                deathScreen.SetActive(false);

            ClearSelectedButton();
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
                playerHealth.Died -= ShowDeathScreen;
        }

        private void Update()
        {
            if (!isGameOver)
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
            isGameOver = true;

            if (deathScreen != null)
                deathScreen.SetActive(true);

            Time.timeScale = 0f;
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