using UnityEngine;

namespace VfxSystem
{
    [DisallowMultipleComponent]
    public sealed class DeathBloodBurst : MonoBehaviour
    {
        [SerializeField] private ParticleSystem splash;
        [SerializeField] private ParticleSystem droplets;
        [SerializeField] private GoreFragment[] fragments = System.Array.Empty<GoreFragment>();

        public bool IsAlive
        {
            get
            {
                if (splash.IsAlive(false) || droplets.IsAlive(false)) return true;
                foreach (GoreFragment fragment in fragments)
                    if (fragment.IsAlive) return true;
                return false;
            }
        }

        public void Play(Vector3 point)
        {
            Clear();
            transform.SetPositionAndRotation(point, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            splash.Play(false);
            droplets.Play(false);
            float rotation = Random.Range(0f, Mathf.PI * 2f);
            for (int i = 0; i < fragments.Length; i++)
            {
                float angle = rotation + i * Mathf.PI * 2f / fragments.Length + Random.Range(-0.3f, 0.3f);
                Vector3 outward = new(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                Vector3 velocity = outward * Random.Range(2f, 4f) + Vector3.up * Random.Range(1.5f, 3f);
                fragments[i].Play(point + outward * 0.15f, velocity, Random.Range(3.5f, 5f));
            }
        }

        public void Tick(float deltaTime)
        {
            foreach (GoreFragment fragment in fragments) fragment.Tick(deltaTime);
        }

        public void Clear()
        {
            splash.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            droplets.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            foreach (GoreFragment fragment in fragments) fragment.Clear();
        }
    }
}
