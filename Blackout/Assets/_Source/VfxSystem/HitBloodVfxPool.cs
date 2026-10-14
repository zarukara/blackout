using UnityEngine;

namespace VfxSystem
{
    [DisallowMultipleComponent]
    public sealed class HitBloodVfxPool : MonoBehaviour
    {
        [SerializeField] private HitBloodVfx prefab;
        [SerializeField, Min(1)] private int capacity = 32;

        private HitBloodVfx[] instances;
        private int nextIndex;

        private void Awake()
        {
            if (prefab == null)
            {
                Debug.LogError("HitBloodVfxPool needs a HitBloodVfx prefab.", this);
                enabled = false;
                return;
            }

            instances = new HitBloodVfx[Mathf.Max(1, capacity)];
            for (int i = 0; i < instances.Length; i++)
            {
                instances[i] = Instantiate(prefab, transform);
                instances[i].gameObject.SetActive(false);
            }
        }

        public void Play(Vector3 point, Vector3 direction, Transform target = null)
        {
            if (!isActiveAndEnabled || instances == null) return;

            int index = nextIndex;
            for (int offset = 0; offset < instances.Length; offset++)
            {
                int candidate = (nextIndex + offset) % instances.Length;
                if (instances[candidate].gameObject.activeSelf) continue;
                index = candidate;
                break;
            }

            // At capacity, recycle in ring order instead of allocating more effects.
            nextIndex = (index + 1) % instances.Length;
            HitBloodVfx effect = instances[index];
            effect.gameObject.SetActive(true);
            effect.Play(point, direction, target);
        }

        private void Update()
        {
            if (instances == null) return;
            foreach (HitBloodVfx effect in instances)
            {
                if (!effect.gameObject.activeSelf || effect.IsAlive) continue;
                effect.Clear();
                effect.gameObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            if (instances == null) return;
            foreach (HitBloodVfx effect in instances)
            {
                effect.Clear();
                effect.gameObject.SetActive(false);
            }
        }
    }
}
