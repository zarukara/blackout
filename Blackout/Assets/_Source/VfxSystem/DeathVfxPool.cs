using UnityEngine;

namespace VfxSystem
{
    [DisallowMultipleComponent]
    public sealed class DeathVfxPool : MonoBehaviour
    {
        [SerializeField] private DeathBloodBurst burstPrefab;
        [SerializeField] private BloodPoolVfx bloodPoolPrefab;
        [SerializeField, Min(1)] private int burstCapacity = 12;
        [SerializeField, Min(1)] private int bloodPoolCapacity = 40;
        [SerializeField] private LayerMask groundLayers;
        [SerializeField, Min(0.01f)] private float groundRayDistance = 50f;
        [SerializeField, Min(0f)] private float surfaceOffset = 0.015f;

        private DeathBloodBurst[] bursts;
        private BloodPoolVfx[] bloodPools;
        private int nextBurst;
        private int nextBloodPool;

        public bool IsReady => isActiveAndEnabled && bursts != null && bloodPools != null;

        private void Awake()
        {
            if (burstPrefab == null || bloodPoolPrefab == null)
            {
                Debug.LogError("DeathVfxPool needs burst and blood pool prefabs.", this);
                enabled = false;
                return;
            }

            bursts = new DeathBloodBurst[Mathf.Max(1, burstCapacity)];
            bloodPools = new BloodPoolVfx[Mathf.Max(1, bloodPoolCapacity)];
            for (int i = 0; i < bursts.Length; i++)
            {
                bursts[i] = Instantiate(burstPrefab, transform);
                bursts[i].gameObject.SetActive(false);
            }
            for (int i = 0; i < bloodPools.Length; i++)
            {
                bloodPools[i] = Instantiate(bloodPoolPrefab, transform);
                bloodPools[i].gameObject.SetActive(false);
            }
        }

        public void Play(Vector3 bodyPosition)
        {
            if (!IsReady) return;
            // Fixed capacity: oldest bursts and persistent stains are recycled in ring order.
            DeathBloodBurst burst = bursts[nextBurst];
            nextBurst = (nextBurst + 1) % bursts.Length;
            burst.gameObject.SetActive(true);
            burst.Play(bodyPosition);

            if (!Physics.Raycast(bodyPosition + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit,
                    groundRayDistance, groundLayers, QueryTriggerInteraction.Ignore) || hit.normal.y < 0.5f)
                return;

            BloodPoolVfx stain = bloodPools[nextBloodPool];
            // Separate coplanar overlapping stains by a tiny stable depth offset.
            float offset = surfaceOffset + nextBloodPool * 0.0002f;
            nextBloodPool = (nextBloodPool + 1) % bloodPools.Length;
            stain.gameObject.SetActive(true);
            stain.Play(hit.point + hit.normal * offset, hit.normal);
        }

        private void Update()
        {
            if (!IsReady) return;
            foreach (DeathBloodBurst burst in bursts)
            {
                if (!burst.gameObject.activeSelf) continue;
                burst.Tick(Time.deltaTime);
                if (burst.IsAlive) continue;
                burst.Clear();
                burst.gameObject.SetActive(false);
            }
            foreach (BloodPoolVfx stain in bloodPools)
                if (stain.gameObject.activeSelf) stain.Tick(Time.deltaTime);
        }

        private void OnDisable()
        {
            if (bursts != null)
                foreach (DeathBloodBurst burst in bursts)
                {
                    burst.Clear();
                    burst.gameObject.SetActive(false);
                }
            if (bloodPools != null)
                foreach (BloodPoolVfx stain in bloodPools) stain.gameObject.SetActive(false);
        }
    }
}
