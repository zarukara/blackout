using UnityEngine;

namespace ProjectileSystem
{
    [DisallowMultipleComponent]
    public sealed class ProjectileVfx : MonoBehaviour
    {
        [SerializeField] private ParticleSystem core;
        [SerializeField] private TrailRenderer trail;

        private void OnEnable()
        {
            // Pool checkout activates the object before Launch positions it.
            Clear();
        }

        private void OnDisable()
        {
            Clear();
        }

        public void Play(float lifetime)
        {
            // Clear again at the new origin before recording any trail positions.
            Clear();

            if (core != null)
            {
                core.Play(false);
                core.Emit(new ParticleSystem.EmitParams { startLifetime = lifetime }, 1);
            }

            if (trail != null)
                trail.emitting = true;
        }

        public void Clear()
        {
            if (trail != null)
            {
                trail.emitting = false;
                trail.Clear();
            }

            if (core != null)
                core.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
