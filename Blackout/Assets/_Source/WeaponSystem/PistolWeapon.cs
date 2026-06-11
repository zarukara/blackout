using PlayerSystem;
using ProjectileSystem;
using UnityEngine;

namespace WeaponSystem
{
    public class PistolWeapon : APlayerWeapon
    {
        [Header("References")]
        [SerializeField] private ProjectilePool projectilePool;
        [SerializeField] private Transform firePoint;
        [SerializeField] private PlayerTargetingController targetingController;

        [Header("Stats")]
        [SerializeField] private int damage = 25;
        [SerializeField] private float projectileSpeed = 25f;
        [SerializeField] private float attackCooldown = 0.25f;
        [SerializeField] private float spreadAngle = 0f;

        private float nextAttackTime;
        private GameObject ownerObject;

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

            if (projectilePool == null || firePoint == null)
                return;

            nextAttackTime = Time.time + attackCooldown;

            Vector3 direction = GetShootDirection();

            Projectile projectile = projectilePool.GetProjectile();

            projectile.Launch(
                firePoint.position,
                direction,
                projectileSpeed,
                damage,
                ownerObject
            );
        }

        private Vector3 GetShootDirection()
        {
            Vector3 fallbackDirection = firePoint != null
                ? firePoint.forward
                : transform.forward;

            Vector3 direction = targetingController != null
                ? targetingController.GetShootDirectionFrom(firePoint.position, fallbackDirection)
                : fallbackDirection;

            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.001f)
                direction = fallbackDirection;

            direction.Normalize();

            if (spreadAngle <= 0f)
                return direction;

            float randomAngle = Random.Range(-spreadAngle, spreadAngle);
            return (Quaternion.Euler(0f, randomAngle, 0f) * direction).normalized;
        }

        private void CacheReferences()
        {
            if (targetingController == null)
                targetingController = GetComponentInParent<PlayerTargetingController>();

            if (ownerObject == null)
            {
                PlayerFacade playerFacade = GetComponentInParent<PlayerFacade>();

                ownerObject = playerFacade != null
                    ? playerFacade.gameObject
                    : gameObject;
            }
        }
    }
}