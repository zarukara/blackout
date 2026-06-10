using System.Collections.Generic;
using UnityEngine;

namespace ProjectileSystem
{
    public class ProjectilePool : MonoBehaviour
    {
        [Header("Pool")]
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private int initialSize = 20;
        [SerializeField] private Transform container;

        private readonly Queue<Projectile> availableProjectiles = new();

        private void Awake()
        {
            if (container == null)
                container = transform;

            CreateInitialPool();
        }

        public Projectile GetProjectile()
        {
            if (availableProjectiles.Count == 0)
                CreateProjectile();

            Projectile projectile = availableProjectiles.Dequeue();
            projectile.gameObject.SetActive(true);

            return projectile;
        }

        public void ReturnProjectile(Projectile projectile)
        {
            if (projectile == null)
                return;

            projectile.gameObject.SetActive(false);
            projectile.transform.SetParent(container);
            availableProjectiles.Enqueue(projectile);
        }

        private void CreateInitialPool()
        {
            for (int i = 0; i < initialSize; i++)
                CreateProjectile();
        }

        private Projectile CreateProjectile()
        {
            Projectile projectile = Instantiate(projectilePrefab, container);
            projectile.Initialize(this);
            projectile.gameObject.SetActive(false);

            availableProjectiles.Enqueue(projectile);

            return projectile;
        }
    }
}