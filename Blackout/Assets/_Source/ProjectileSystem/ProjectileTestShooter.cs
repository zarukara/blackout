using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectileSystem
{
    public class ProjectileTestShooter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ProjectilePool projectilePool;
        [SerializeField] private Transform firePoint;

        [Header("Shoot")]
        [SerializeField] private float projectileSpeed = 20f;
        [SerializeField] private int projectileDamage = 25;

        private void Update()
        {
            if (Keyboard.current == null)
                return;

            if (Keyboard.current.tKey.wasPressedThisFrame)
                Shoot();
        }

        private void Shoot()
        {
            if (projectilePool == null || firePoint == null)
                return;

            Projectile projectile = projectilePool.GetProjectile();

            projectile.Launch(
                firePoint.position,
                firePoint.forward,
                projectileSpeed,
                projectileDamage,
                gameObject
            );
        }
    }
}