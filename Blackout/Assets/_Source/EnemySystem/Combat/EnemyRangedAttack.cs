using ProjectileSystem;
using UnityEngine;

namespace EnemySystem
{
    public class EnemyRangedAttack : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ProjectilePool projectilePool;
        [SerializeField] private Transform firePoint;

        [Header("Attack")]
        [SerializeField] private int damage = 10;
        [SerializeField] private float attackDistance = 8f;
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private float projectileSpeed = 18f;

        [Header("Spread")]
        [SerializeField] private int projectilesPerShot = 1;
        [SerializeField] private float spreadAngle = 2f;

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

            if (projectilePool == null || firePoint == null)
                return;

            if (Time.time < nextAttackTime)
                return;

            if (!CanAttack(target))
                return;

            nextAttackTime = Time.time + attackCooldown;

            Vector3 direction = target.position - firePoint.position;
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.001f)
                return;

            Shoot(direction.normalized);
        }

        private void Shoot(Vector3 baseDirection)
        {
            int projectileCount = Mathf.Max(1, projectilesPerShot);

            if (projectileCount == 1)
            {
                LaunchProjectile(baseDirection);
                return;
            }

            for (int i = 0; i < projectileCount; i++)
            {
                float t = projectileCount == 1 ? 0.5f : i / (float)(projectileCount - 1);
                float angle = Mathf.Lerp(-spreadAngle * 0.5f, spreadAngle * 0.5f, t);

                Vector3 spreadDirection = Quaternion.Euler(0f, angle, 0f) * baseDirection;
                LaunchProjectile(spreadDirection.normalized);
            }
        }

        private void LaunchProjectile(Vector3 direction)
        {
            Projectile projectile = projectilePool.GetProjectile();

            projectile.Launch(
                firePoint.position,
                direction,
                projectileSpeed,
                damage,
                gameObject
            );
        }
    }
}