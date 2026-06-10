using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneSystem
{
    public static class SceneLoader
    {
        public static void LoadMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        public static void LoadGameScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Level_01");
        }

        public static void ReloadCurrentScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public static void QuitGame()
        {
            Time.timeScale = 1f;
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}