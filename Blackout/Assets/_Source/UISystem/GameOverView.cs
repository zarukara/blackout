using CombatSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace UISystem
{
    public class GameOverView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Health playerHealth;
        [SerializeField] private GameObject deathScreen;

        private bool isGameOver;

        private void OnEnable()
        {
            playerHealth.Died += ShowDeathScreen;
        }

        private void OnDisable()
        {
            playerHealth.Died -= ShowDeathScreen;
        }

        private void Start()
        {
            deathScreen.SetActive(false);
        }

        private void Update()
        {
            if (!isGameOver)
                return;

            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                RestartScene();
            }
        }

        private void ShowDeathScreen()
        {
            isGameOver = true;
            deathScreen.SetActive(true);
            Time.timeScale = 0f;
        }

        private void RestartScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}