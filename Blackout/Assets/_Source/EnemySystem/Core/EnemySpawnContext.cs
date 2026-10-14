using ProjectileSystem;
using UnityEngine;
using VfxSystem;

namespace EnemySystem
{
    public readonly struct EnemySpawnContext
    {
        public EnemySpawnContext(Transform target, ProjectilePool projectilePool, HitBloodVfxPool hitBloodPool = null)
        {
            Target = target;
            ProjectilePool = projectilePool;
            HitBloodPool = hitBloodPool;
        }

        public Transform Target { get; }
        public ProjectilePool ProjectilePool { get; }
        public HitBloodVfxPool HitBloodPool { get; }
    }
}
