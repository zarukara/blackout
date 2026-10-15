using CombatSystem;
using UnityEngine;
using VfxSystem;

namespace EnemySystem
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    public sealed class EnemyHitVfx : MonoBehaviour
    {
        [SerializeField] private Transform fallbackHitPoint;

        private Health health;
        private EnemyDeathVfx deathVfx;
        private HitBloodVfxPool pool;

        private void Awake()
        {
            health = GetComponent<Health>();
            deathVfx = GetComponent<EnemyDeathVfx>();
        }
        private void OnEnable() => health.Damaged += OnDamaged;
        private void OnDisable() => health.Damaged -= OnDamaged;

        public void Initialize(HitBloodVfxPool hitBloodPool)
        {
            if (hitBloodPool != null) pool = hitBloodPool;
        }

        private void OnDamaged(DamageInfo hit)
        {
            if (pool == null || hit.Amount <= 0) return;
            if (hit.IsLethal && deathVfx != null && deathVfx.IsReady) return;

            Vector3 point = hit.HasContact ? hit.Point
                : fallbackHitPoint != null ? fallbackHitPoint.position : transform.position + Vector3.up;
            Vector3 direction = hit.HasContact ? hit.Direction : transform.forward;
            pool.Play(point, direction, fallbackHitPoint != null ? fallbackHitPoint : transform);
        }
    }
}
