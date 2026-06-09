using CombatSystem;
using UnityEngine;

namespace PlayerSystem
{
    public class PlayerMeleeAttack : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private Transform attackPoint;

        [Header("Attack")]
        [SerializeField] private int damage = 50;
        [SerializeField] private float attackRadius = 1.2f;
        [SerializeField] private float attackCooldown = 0.4f;
        [SerializeField] private LayerMask enemyLayer;

        private float nextAttackTime;

        private void OnEnable()
        {
            inputReader.AttackPressed += Attack;
        }

        private void OnDisable()
        {
            inputReader.AttackPressed -= Attack;
        }

        private void Attack()
        {
            if (Time.time < nextAttackTime)
                return;

            nextAttackTime = Time.time + attackCooldown;

            Collider[] hits = Physics.OverlapSphere(
                attackPoint.position,
                attackRadius,
                enemyLayer
            );

            foreach (Collider hit in hits)
            {
                if (hit.TryGetComponent(out Health health))
                {
                    health.TakeDamage(damage);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (attackPoint == null)
                return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}