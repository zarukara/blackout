using ProjectileSystem;
using UnityEngine;
using VfxSystem;

namespace EnemySystem
{
    public readonly struct EnemySpawnContext
    {
        public EnemySpawnContext(Transform target, ProjectilePool projectilePool, HitBloodVfxPool hitBloodPool = null,
            DeathVfxPool deathVfxPool = null)
        {
            Target = target;
            ProjectilePool = projectilePool;
            HitBloodPool = hitBloodPool;
            DeathVfxPool = deathVfxPool;
        }

        public Transform Target { get; }
        public ProjectilePool ProjectilePool { get; }
        public HitBloodVfxPool HitBloodPool { get; }
        public DeathVfxPool DeathVfxPool { get; }
    }
}
