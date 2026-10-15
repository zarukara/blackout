using CombatSystem;
using UnityEngine;
using VfxSystem;

namespace EnemySystem
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    public sealed class EnemyDeathVfx : MonoBehaviour
    {
        [SerializeField] private Transform bodyPoint;
        [SerializeField] private Renderer[] visualRenderers;
        [SerializeField] private ParticleSystem[] localParticles;

        private Health health;
        private DeathVfxPool pool;
        private bool hasPlayed;

        public bool IsReady => isActiveAndEnabled && pool != null && pool.IsReady;

        private void Awake()
        {
            health = GetComponent<Health>();
            if (visualRenderers == null || visualRenderers.Length == 0)
                visualRenderers = GetComponentsInChildren<Renderer>(true);
            if (localParticles == null || localParticles.Length == 0)
                localParticles = GetComponentsInChildren<ParticleSystem>(true);
        }

        private void OnEnable() => health.Died += OnDied;
        private void OnDisable() => health.Died -= OnDied;

        public void Initialize(DeathVfxPool deathPool)
        {
            if (deathPool != null) pool = deathPool;
        }

        private void OnDied()
        {
            if (hasPlayed) return;
            hasPlayed = true;
            Vector3 point = bodyPoint != null ? bodyPoint.position : transform.position + Vector3.up;

            // These references belong only to this enemy, never to pooled world effects.
            foreach (ParticleSystem particles in localParticles)
                if (particles != null) particles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            foreach (Renderer visual in visualRenderers)
                if (visual != null) visual.enabled = false;

            if (pool != null) pool.Play(point);
        }
    }
}
