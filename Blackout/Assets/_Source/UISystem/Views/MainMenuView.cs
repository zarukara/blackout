using SceneSystem;
using UnityEngine;
using UnityEngine.UI;

namespace UISystem
{
    public class MainMenuView : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button exitButton;

        private void Awake()
        {
            newGameButton.onClick.AddListener(StartNewGame);
            continueButton.onClick.AddListener(ContinueGame);
            optionsButton.onClick.AddListener(OpenOptions);
            exitButton.onClick.AddListener(ExitGame);
        }

        private void OnDestroy()
        {
            newGameButton.onClick.RemoveListener(StartNewGame);
            continueButton.onClick.RemoveListener(ContinueGame);
            optionsButton.onClick.RemoveListener(OpenOptions);
            exitButton.onClick.RemoveListener(ExitGame);
        }

        private void StartNewGame()
        {
            SceneLoader.LoadGameScene();
        }

        private void ContinueGame()
        {
            Debug.Log("Continue is not implemented yet.");
        }

        private void OpenOptions()
        {
            Debug.Log("Options menu is not implemented yet.");
        }

        private void ExitGame()
        {
            SceneLoader.QuitGame();
        }
    }
}