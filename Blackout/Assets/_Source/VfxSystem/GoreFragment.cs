using UnityEngine;

namespace VfxSystem
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class GoreFragment : MonoBehaviour
    {
        private Rigidbody body;
        private Vector3 baseScale;
        private Vector3 flightScale;
        private float lifetime;
        private float age;

        public bool IsAlive { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            baseScale = transform.localScale;
        }

        public void Play(Vector3 position, Vector3 velocity, float duration)
        {
            Clear();
            transform.SetPositionAndRotation(position, Random.rotation);
            flightScale = baseScale * Random.Range(0.85f, 1.15f);
            transform.localScale = flightScale;
            age = 0f;
            lifetime = duration;
            IsAlive = true;
            gameObject.SetActive(true);
            body.position = position;
            body.rotation = transform.rotation;
            body.isKinematic = false;
            body.linearVelocity = velocity;
            body.angularVelocity = Random.insideUnitSphere * 10f;
        }

        public void Tick(float deltaTime)
        {
            if (!IsAlive) return;
            age += deltaTime;
            if (age >= lifetime)
            {
                Clear();
                return;
            }

            // Shrink only at the end; no per-fragment material instances or alpha sorting.
            float remaining = Mathf.Clamp01((lifetime - age) / 0.6f);
            transform.localScale = flightScale * Mathf.SmoothStep(0f, 1f, remaining);
        }

        public void Clear()
        {
            IsAlive = false;
            if (!body.isKinematic)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
            body.isKinematic = true;
            transform.localScale = baseScale;
            gameObject.SetActive(false);
        }
    }
}
