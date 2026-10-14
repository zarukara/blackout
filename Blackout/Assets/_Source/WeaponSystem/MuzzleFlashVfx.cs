using UnityEngine;

namespace WeaponSystem
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ParticleSystem))]
    public sealed class MuzzleFlashVfx : MonoBehaviour
    {
        private ParticleSystem particles;
        private Camera viewCamera;

        private void Awake()
        {
            particles = GetComponent<ParticleSystem>();
            viewCamera = Camera.main;
        }

        public void Play()
        {
            if (!isActiveAndEnabled || particles == null)
                return;

            if (viewCamera == null)
                viewCamera = Camera.main;

            var emission = new ParticleSystem.EmitParams();
            if (viewCamera != null)
            {
                // Orient the billboard along the muzzle's projection; gameplay aim is unchanged.
                Vector3 direction = viewCamera.transform.InverseTransformDirection(transform.forward);
                emission.rotation = -Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            }

            // Explicit emission also preserves separate shots within the same frame.
            particles.Emit(emission, 1);
        }

        private void OnDisable()
        {
            if (particles != null)
                particles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
