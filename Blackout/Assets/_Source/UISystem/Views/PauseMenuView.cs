using PlayerSystem;
using SceneSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UISystem
{
    public class PauseMenuView : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button controlsButton;
        [SerializeField] private Button exitButton;

        private PlayerInputReader inputReader;
        private UiStateController uiStateController;

        public void Initialize(PlayerInputReader inputReader, UiStateController uiStateController)
        {
            this.inputReader = inputReader;
            this.uiStateController = uiStateController;

            this.inputReader.PausePressed += TogglePause;

            resumeButton.onClick.AddListener(Resume);
            restartButton.onClick.AddListener(RestartScene);
            optionsButton.onClick.AddListener(OpenOptions);
            controlsButton.onClick.AddListener(OpenControls);
            exitButton.onClick.AddListener(ExitToMainMenu);

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
            uiStateController.TogglePause();
            ClearSelectedButton();
        }

        private void Resume()
        {
            uiStateController.ClosePause();
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