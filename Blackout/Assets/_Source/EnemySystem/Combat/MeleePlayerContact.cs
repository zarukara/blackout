using UnityEngine;

namespace EnemySystem
{
    // Suppresses only solid enemy/target-body contact. Queries, bullets, walls and
    // thrown-enemy collisions continue to use the original colliders and layers.
    [DisallowMultipleComponent]
    public sealed class MeleePlayerContact : MonoBehaviour
    {
        private Collider[] colliders;
        private Collider[] targetColliders;
        private bool[,] previouslyIgnored;
        private CharacterController targetController;
        private float nextRefresh;

        private void Awake() => colliders = GetComponentsInChildren<Collider>(true);

        public void SetTarget(Transform target)
        {
            CharacterController controller = target != null ? target.GetComponentInParent<CharacterController>() : null;
            if (controller == targetController) return;
            Restore();
            targetController = controller;
            if (controller == null) return;
            targetColliders = controller.GetComponentsInChildren<Collider>(true);
            previouslyIgnored = new bool[colliders.Length, targetColliders.Length];
            for (int i = 0; i < colliders.Length; i++)
                for (int j = 0; j < targetColliders.Length; j++)
                {
                    if (!IsBodyPair(i, j)) continue;
                    previouslyIgnored[i, j] = Physics.GetIgnoreCollision(colliders[i], targetColliders[j]);
                    Physics.IgnoreCollision(colliders[i], targetColliders[j], true);
                }
        }

        private bool IsBodyPair(int i, int j) => colliders[i] != null && !colliders[i].isTrigger &&
            targetColliders[j] != null && !targetColliders[j].isTrigger &&
            targetColliders[j].GetComponentInParent<AEnemy>() == null;

        private void LateUpdate()
        {
            if (targetController == null || Time.time < nextRefresh) return;
            nextRefresh = Time.time + 0.25f;
            // Unity can lose pair flags when either collider is temporarily disabled.
            for (int i = 0; i < colliders.Length; i++)
                for (int j = 0; j < targetColliders.Length; j++)
                    if (IsBodyPair(i, j) && !Physics.GetIgnoreCollision(colliders[i], targetColliders[j]))
                        Physics.IgnoreCollision(colliders[i], targetColliders[j], true);
        }

        private void OnDisable() => Restore();

        private void Restore()
        {
            if (targetController != null)
                for (int i = 0; i < colliders.Length; i++)
                    for (int j = 0; j < targetColliders.Length; j++)
                        if (IsBodyPair(i, j)) Physics.IgnoreCollision(colliders[i], targetColliders[j], previouslyIgnored[i, j]);
            targetController = null;
            targetColliders = null;
        }
    }
}
