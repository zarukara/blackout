using CombatSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
            {
                deathScreen.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                playerHealth.Died -= ShowDeathScreen;
            }
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