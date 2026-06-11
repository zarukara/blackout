using System.Collections.Generic;
using CombatSystem;
using UnityEngine;

namespace EnemySystem
{
    [DisallowMultipleComponent]
    public class TargetableEnemy : MonoBehaviour
    {
        [Header("Renderers")]
        [SerializeField] private Renderer[] targetRenderers;

        [Header("Outline")]
        [SerializeField] private Material outlineMaterial;

        [Header("Target Point")]
        [SerializeField] private Transform aimPoint;
        [SerializeField] private float fallbackAimHeight = 1f;

        private Health health;
        private Material[][] baseMaterials;
        private bool isTargeted;

        public bool IsAvailable => health != null && !health.IsDead && isActiveAndEnabled;

        private void Awake()
        {
            CacheReferences();
            CacheBaseMaterials();
            ApplyTargetState(false);
        }

        private void OnValidate()
        {
            CacheReferences();
        }

        private void OnDisable()
        {
            ApplyTargetState(false);
            isTargeted = false;
        }

        public Vector3 GetAimPoint()
        {
            if (aimPoint != null)
                return aimPoint.position;

            return transform.position + Vector3.up * fallbackAimHeight;
        }

        public void SetTargeted(bool targeted)
        {
            if (isTargeted == targeted)
                return;

            isTargeted = targeted;
            ApplyTargetState(isTargeted);
        }

        private void CacheReferences()
        {
            if (health == null)
                health = GetComponent<Health>();

            if (targetRenderers == null || targetRenderers.Length == 0)
                targetRenderers = GetComponentsInChildren<Renderer>(true);
        }

        private void CacheBaseMaterials()
        {
            if (targetRenderers == null)
                return;

            baseMaterials = new Material[targetRenderers.Length][];

            for (int i = 0; i < targetRenderers.Length; i++)
            {
                Renderer targetRenderer = targetRenderers[i];

                if (targetRenderer == null)
                    continue;

                Material[] currentMaterials = targetRenderer.sharedMaterials;
                List<Material> cleanMaterials = new();

                foreach (Material material in currentMaterials)
                {
                    if (material == null)
                        continue;

                    if (outlineMaterial != null && material == outlineMaterial)
                        continue;

                    cleanMaterials.Add(material);
                }

                baseMaterials[i] = cleanMaterials.ToArray();
            }
        }

        private void ApplyTargetState(bool targeted)
        {
            if (targeted)
                AddOutlineMaterial();
            else
                RestoreBaseMaterials();
        }

        private void AddOutlineMaterial()
        {
            if (outlineMaterial == null)
                return;

            if (targetRenderers == null || baseMaterials == null)
                return;

            for (int i = 0; i < targetRenderers.Length; i++)
            {
                Renderer targetRenderer = targetRenderers[i];

                if (targetRenderer == null)
                    continue;

                if (i >= baseMaterials.Length || baseMaterials[i] == null)
                    continue;

                Material[] outlinedMaterials = new Material[baseMaterials[i].Length + 1];

                for (int j = 0; j < baseMaterials[i].Length; j++)
                    outlinedMaterials[j] = baseMaterials[i][j];

                outlinedMaterials[^1] = outlineMaterial;

                targetRenderer.sharedMaterials = outlinedMaterials;
            }
        }

        private void RestoreBaseMaterials()
        {
            if (targetRenderers == null || baseMaterials == null)
                return;

            for (int i = 0; i < targetRenderers.Length; i++)
            {
                Renderer targetRenderer = targetRenderers[i];

                if (targetRenderer == null)
                    continue;

                if (i >= baseMaterials.Length || baseMaterials[i] == null)
                    continue;

                targetRenderer.sharedMaterials = baseMaterials[i];
            }
        }
    }
}