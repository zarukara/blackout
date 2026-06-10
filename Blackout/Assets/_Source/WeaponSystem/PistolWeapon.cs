using ProjectileSystem;
using UnityEngine;

namespace WeaponSystem
{
    public class PistolWeapon : APlayerWeapon
    {
        [Header("References")]
        [SerializeField] private ProjectilePool projectilePool;
        [SerializeField] private Transform firePoint;

        [Header("Stats")]
        [SerializeField] private int damage = 25;
        [SerializeField] private float projectileSpeed = 25f;
        [SerializeField] private float attackCooldown = 0.25f;
        [SerializeField] private float spreadAngle = 1.5f;

        private float nextAttackTime;

        public override void Attack()
        {
            if (Time.time < nextAttackTime)
                return;

            if (projectilePool == null || firePoint == null)
                return;

            nextAttackTime = Time.time + attackCooldown;

            Vector3 direction = firePoint.forward;

            if (spreadAngle > 0f)
            {
                float randomAngle = Random.Range(-spreadAngle, spreadAngle);
                direction = Quaternion.Euler(0f, randomAngle, 0f) * direction;
            }

            Projectile projectile = projectilePool.GetProjectile();

            projectile.Launch(
                firePoint.position,
                direction.normalized,
                projectileSpeed,
                damage,
                gameObject
            );
        }
    }
}