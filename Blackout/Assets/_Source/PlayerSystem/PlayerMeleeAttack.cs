using System.Collections.Generic;
using CombatSystem;
using EnemySystem;
using UnityEngine;

namespace PlayerSystem
{
    public class PlayerMeleeAttack : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Health playerHealth;
        [SerializeField] private Transform attackPoint;

        [Header("Attack")]
        [SerializeField] private int damage = 50;
        [SerializeField] private float attackRadius = 1.2f;
        [SerializeField] private float attackCooldown = 0.4f;
        [SerializeField] private LayerMask enemyLayer;

        [Header("Grabbed Enemy Heal")]
        [SerializeField] private int healOnGrabbedKill = 25;

        private readonly HashSet<Health> damagedTargets = new HashSet<Health>();

        private PlayerInputReader inputReader;
        private PlayerGrabController grabController;
        private float nextAttackTime;

        public void Initialize(PlayerInputReader inputReader, PlayerGrabController grabController)
        {
            this.inputReader = inputReader;
            this.grabController = grabController;

            this.inputReader.AttackPressed += Attack;
        }

        private void OnDestroy()
        {
            if (inputReader != null)
                inputReader.AttackPressed -= Attack;
        }

        private void Attack()
        {
            if (Time.time < nextAttackTime)
                return;

            nextAttackTime = Time.time + attackCooldown;

            if (grabController != null && grabController.HasGrabbedEnemy)
            {
                AttackGrabbedEnemy();
                return;
            }

            AttackEnemiesInRadius();
        }

        private void AttackGrabbedEnemy()
        {
            EnemyGrabHandler grabbedEnemy = grabController.GrabbedEnemy;

            if (grabbedEnemy == null)
                return;

            Health enemyHealth = grabbedEnemy.GetComponent<Health>();

            if (enemyHealth == null)
                return;

            enemyHealth.TakeDamage(damage);

            if (enemyHealth.IsDead && playerHealth != null)
            {
                playerHealth.Heal(healOnGrabbedKill);
            }
        }

        private void AttackEnemiesInRadius()
        {
            damagedTargets.Clear();

            Collider[] hits = Physics.OverlapSphere(
                attackPoint.position,
                attackRadius,
                enemyLayer
            );

            foreach (Collider hit in hits)
            {
                Health health = hit.GetComponentInParent<Health>();

                if (health == null)
                    continue;

                if (damagedTargets.Contains(health))
                    continue;

                damagedTargets.Add(health);
                health.TakeDamage(damage);
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