using System;
using UnityEngine;

namespace CombatSystem
{
    public class Health : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private bool destroyOnDeath = true;

        public int CurrentHealth { get; private set; }
        public int MaxHealth => maxHealth;
        public bool IsDead { get; private set; }

        public event Action<int, int> HealthChanged;
        public event Action<DamageInfo> Damaged;
        public event Action Died;

        private void Awake()
        {
            CurrentHealth = maxHealth;
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public void TakeDamage(int damage)
        {
            ApplyDamage(damage, Vector3.zero, Vector3.zero, false);
        }

        public void TakeDamage(int damage, Vector3 hitPoint, Vector3 hitDirection)
        {
            ApplyDamage(damage, hitPoint, hitDirection, true);
        }

        private void ApplyDamage(int damage, Vector3 hitPoint, Vector3 hitDirection, bool hasContact)
        {
            if (IsDead || damage <= 0)
                return;

            int previousHealth = CurrentHealth;
            CurrentHealth -= damage;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);
            int appliedDamage = previousHealth - CurrentHealth;

            HealthChanged?.Invoke(CurrentHealth, maxHealth);

            Damaged?.Invoke(new DamageInfo(
                appliedDamage, CurrentHealth <= 0, hitPoint, hitDirection, hasContact));

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(int amount)
        {
            if (IsDead || amount <= 0)
                return;

            CurrentHealth += amount;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);

            HealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        private void Die()
        {
            if (IsDead)
                return;

            IsDead = true;
            Died?.Invoke();

            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }
    }
}
