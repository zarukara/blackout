using UnityEngine;

namespace CombatSystem
{
    public class Health : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private int maxHealth = 100;

        private int currentHealth;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(int damage)
        {
            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Destroy(gameObject);
        }
    }
}