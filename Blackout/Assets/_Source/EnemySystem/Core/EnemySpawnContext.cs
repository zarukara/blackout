using ProjectileSystem;
using UnityEngine;

namespace EnemySystem
{
    public readonly struct EnemySpawnContext
    {
        public EnemySpawnContext(Transform target, ProjectilePool projectilePool)
        {
            Target = target;
            ProjectilePool = projectilePool;
        }

        public Transform Target { get; }
        public ProjectilePool ProjectilePool { get; }
    }
}
