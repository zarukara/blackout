using CombatSystem;
using PlayerSystem;
using SceneSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UISystem
{
    public class PauseMenuView : MonoBehaviour
    {
        [Header("Screens")]
        [SerializeField] private GameObject pauseScreen;

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button controlsButton;
        [SerializeField] private Button exitButton;

        private PlayerInputReader inputReader;
        private Health playerHealth;
        private bool isPaused;

        public void Initialize(PlayerInputReader inputReader, Health playerHealth)
        {
            this.inputReader = inputReader;
            this.playerHealth = playerHealth;

            this.inputReader.PausePressed += TogglePause;

            resumeButton.onClick.AddListener(Resume);
            restartButton.onClick.AddListener(RestartScene);
            optionsButton.onClick.AddListener(OpenOptions);
            controlsButton.onClick.AddListener(OpenControls);
            exitButton.onClick.AddListener(ExitToMainMenu);

            if (pauseScreen != null)
                pauseScreen.SetActive(false);

            ClearSelectedButton();
        }

        private void OnDestroy()
        {
            if (inputReader != null)
                inputReader.PausePressed -= TogglePause;

            resumeButton.onClick.RemoveListener(Resume);
            restartButton.onClick.RemoveListener(RestartScene);
            optionsButton.onClick.RemoveListener(OpenOptions);
            controlsButton.onClick.RemoveListener(OpenControls);
            exitButton.onClick.RemoveListener(ExitToMainMenu);
        }

        private void TogglePause()
        {
            if (playerHealth != null && playerHealth.IsDead)
                return;

            if (isPaused)
                Resume();
            else
                Pause();
        }

        private void Pause()
        {
            if (playerHealth != null && playerHealth.IsDead)
                return;

            isPaused = true;
            pauseScreen.SetActive(true);
            Time.timeScale = 0f;

            ClearSelectedButton();
        }

        public void Resume()
        {
            isPaused = false;
            pauseScreen.SetActive(false);
            Time.timeScale = 1f;

            ClearSelectedButton();
        }

        private void RestartScene()
        {
            ClearSelectedButton();
            SceneLoader.ReloadCurrentScene();
        }

        private void OpenOptions()
        {
            Debug.Log("Options menu is not implemented yet.");
            ClearSelectedButton();
        }

        private void OpenControls()
        {
            Debug.Log("Controls menu is not implemented yet.");
            ClearSelectedButton();
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