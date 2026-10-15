using UnityEngine;

namespace VfxSystem
{
    [DisallowMultipleComponent]
    public sealed class BloodPoolVfx : MonoBehaviour
    {
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private Mesh[] shapes;
        [SerializeField] private Vector2 diameterRange = new(2.5f, 4f);
        [SerializeField, Min(0.01f)] private float initialDiameter = 0.3f;
        [SerializeField, Min(0.01f)] private float spreadDuration = 1f;
        [SerializeField, Min(0f)] private float spreadDelay = 0.1f;

        private Vector3 initialScale;
        private Vector3 finalScale;
        private float age;
        private bool spreading;

        public void Play(Vector3 point, Vector3 normal)
        {
            if (shapes != null && shapes.Length > 0)
                meshFilter.sharedMesh = shapes[Random.Range(0, shapes.Length)];

            transform.SetPositionAndRotation(point,
                Quaternion.FromToRotation(Vector3.up, normal) * Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            float diameter = Random.Range(diameterRange.x, diameterRange.y);
            float proportion = Random.Range(0.85f, 1f);
            initialScale = new Vector3(initialDiameter, 1f, initialDiameter * proportion);
            finalScale = new Vector3(diameter, 1f, diameter * proportion);
            transform.localScale = initialScale;
            age = 0f;
            spreading = true;
        }

        public void Tick(float deltaTime)
        {
            if (!spreading) return;
            age += deltaTime;
            float progress = Mathf.Clamp01((age - spreadDelay) / spreadDuration);
            float eased = 1f - Mathf.Pow(1f - progress, 3f);
            transform.localScale = Vector3.LerpUnclamped(initialScale, finalScale, eased);
            if (progress >= 1f) spreading = false;
        }
    }
}
