using CombatSystem;
using UnityEngine;

namespace EnemySystem
{
    public class EnemyMeleeAttack : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform target;

        [Header("Attack")]
        [SerializeField] private int damage = 10;
        [SerializeField] private float attackDistance = 1.7f;
        [SerializeField] private float attackCooldown = 1f;

        private Health targetHealth;
        private float nextAttackTime;

        private void Awake()
        {
            if (target != null)
            {
                targetHealth = target.GetComponent<Health>();
            }
        }

        private void Update()
        {
            TryAttack();
        }

        private void TryAttack()
        {
            if (target == null || targetHealth == null)
                return;

            if (Time.time < nextAttackTime)
                return;

            float distance = Vector3.Distance(transform.position, target.position);

            if (distance > attackDistance)
                return;

            nextAttackTime = Time.time + attackCooldown;
            targetHealth.TakeDamage(damage);
        }
    }
}