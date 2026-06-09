using PlayerSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
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
        private bool isPaused;

        public void Initialize(PlayerInputReader inputReader)
        {
            this.inputReader = inputReader;
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
            if (isPaused)
                Resume();
            else
                Pause();
        }

        private void Pause()
        {
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
            Time.timeScale = 1f;
            ClearSelectedButton();

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
            Debug.Log("Main menu is not implemented yet.");
            ClearSelectedButton();
        }

        private void ClearSelectedButton()
        {
            if (EventSystem.current == null)
                return;

            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}