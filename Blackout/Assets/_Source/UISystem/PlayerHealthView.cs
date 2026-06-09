using CombatSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UISystem
{
    public class PlayerHealthView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Health playerHealth;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private Image healthFillImage;

        [Header("Animation")]
        [SerializeField] private float fillChangeSpeed = 5f;

        private float targetFillAmount = 1f;

        private void OnEnable()
        {
            if (playerHealth != null)
            {
                playerHealth.HealthChanged += UpdateHealthView;
            }
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.HealthChanged -= UpdateHealthView;
            }
        }

        private void Start()
        {
            if (playerHealth != null)
            {
                UpdateHealthView(playerHealth.CurrentHealth, playerHealth.MaxHealth);
                healthFillImage.fillAmount = targetFillAmount;
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