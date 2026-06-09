using CombatSystem;
using UnityEngine;

namespace EnemySystem
{
    public class EnemyMeleeAttack : MonoBehaviour
    {
        [Header("Attack")]
        [SerializeField] private int damage = 10;
        [SerializeField] private float attackDistance = 1.7f;
        [SerializeField] private float attackCooldown = 1f;

        private float nextAttackTime;

        public float AttackDistance => attackDistance;

        public bool CanAttack(Transform target)
        {
            if (target == null)
                return false;

            Vector3 direction = target.position - transform.position;
            direction.y = 0f;

            return direction.magnitude <= attackDistance;
        }

        public void TryAttack(Transform target)
        {
            if (target == null)
                return;

            if (Time.time < nextAttackTime)
                return;

            if (!CanAttack(target))
                return;

            Health targetHealth = target.GetComponent<Health>();

            if (targetHealth == null)
                return;

            nextAttackTime = Time.time + attackCooldown;
            targetHealth.TakeDamage(damage);
        }
    }
}