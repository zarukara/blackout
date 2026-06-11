using CombatSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UISystem
{
    public class PlayerHealthView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private Image healthFillImage;

        [Header("Animation")]
        [SerializeField] private float fillChangeSpeed = 5f;

        private Health playerHealth;
        private float targetFillAmount = 1f;

        public void Initialize(Health playerHealth)
        {
            this.playerHealth = playerHealth;

            this.playerHealth.HealthChanged += UpdateHealthView;

            UpdateHealthView(
                this.playerHealth.CurrentHealth,
                this.playerHealth.MaxHealth
            );

            if (healthFillImage != null)
            {
                healthFillImage.fillAmount = targetFillAmount;
            }
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                playerHealth.HealthChanged -= UpdateHealthView;
            }
        }

        private void Update()
        {
            if (healthFillImage == null)
                return;

            healthFillImage.fillAmount = Mathf.MoveTowards(
                healthFillImage.fillAmount,
                targetFillAmount,
                fillChangeSpeed * Time.deltaTime
            );
        }

        private void UpdateHealthView(int currentHealth, int maxHealth)
        {
            if (healthText != null)
            {
                healthText.text = $"{currentHealth} / {maxHealth}";
            }

            targetFillAmount = (float)currentHealth / maxHealth;
        }
    }
}