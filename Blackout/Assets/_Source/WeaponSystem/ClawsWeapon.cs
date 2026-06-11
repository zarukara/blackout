using System.Collections.Generic;
using CombatSystem;
using EnemySystem;
using PlayerSystem;
using UnityEngine;

namespace WeaponSystem
{
    public class ClawsWeapon : APlayerWeapon
    {
        [Header("References")]
        [SerializeField] private Transform attackPoint;
        [SerializeField] private PlayerGrabController playerGrabController;
        [SerializeField] private Health playerHealth;

        [Header("Attack")]
        [SerializeField] private int damage = 50;
        [SerializeField] private float attackRadius = 1.5f;
        [SerializeField] private float attackCooldown = 0.35f;
        [SerializeField] private LayerMask enemyLayer;

        [Header("Grabbed Enemy")]
        [SerializeField] private int grabbedEnemyHealAmount = 15;

        private readonly HashSet<Health> damagedTargets = new();

        private float nextAttackTime;

        private void Awake()
        {
            CacheReferences();
        }

        private void OnValidate()
        {
            CacheReferences();
        }

        public override void Attack()
        {
            CacheReferences();

            if (Time.time < nextAttackTime)
                return;

            nextAttackTime = Time.time + attackCooldown;

            if (TryAttackGrabbedEnemy())
                return;

            AttackEnemiesInRadius();
        }

        private void CacheReferences()
        {
            if (playerGrabController == null)
                playerGrabController = GetComponentInParent<PlayerGrabController>();

            if (playerHealth == null)
                playerHealth = GetComponentInParent<Health>();
        }

        private bool TryAttackGrabbedEnemy()
        {
            if (playerGrabController == null)
                return false;

            EnemyGrabHandler grabbedEnemy = playerGrabController.GrabbedEnemy;

            if (grabbedEnemy == null)
                return false;

            Health grabbedHealth = grabbedEnemy.Health;

            if (grabbedHealth == null || grabbedHealth.IsDead)
                return true;

            grabbedHealth.TakeDamage(damage);

            if (grabbedHealth.IsDead)
                HealPlayerAfterExecution();

            return true;
        }

        private void AttackEnemiesInRadius()
        {
            Vector3 center = attackPoint != null
                ? attackPoint.position
                : transform.position;

            Collider[] hits = Physics.OverlapSphere(
                center,
                attackRadius,
                enemyLayer
            );

            damagedTargets.Clear();

            foreach (Collider hit in hits)
            {
                Health health = hit.GetComponentInParent<Health>();

                if (health == null)
                    continue;

                if (health.IsDead)
                    continue;

                if (!damagedTargets.Add(health))
                    continue;

                health.TakeDamage(damage);
            }
        }

        private void HealPlayerAfterExecution()
        {
            if (playerHealth == null)
                return;

            playerHealth.Heal(grabbedEnemyHealAmount);
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 center = attackPoint != null
                ? attackPoint.position
                : transform.position;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(center, attackRadius);
        }
    }
}