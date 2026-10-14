using UnityEngine;

namespace VfxSystem
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(100)]
    public sealed class HitBloodVfx : MonoBehaviour
    {
        [SerializeField] private ParticleSystem impactSplash;
        [SerializeField] private ParticleSystem droplets;

        private Transform followTarget;
        private Vector3 localHitPoint;

        public bool IsAlive => impactSplash.IsAlive(false) || droplets.IsAlive(false);

        public void Play(Vector3 point, Vector3 direction, Transform target = null)
        {
            Clear();
            followTarget = target;
            if (followTarget != null) localHitPoint = followTarget.InverseTransformPoint(point);
            if (direction.sqrMagnitude < 0.001f) direction = Vector3.up;
            transform.SetPositionAndRotation(point, Quaternion.LookRotation(direction.normalized));
            impactSplash.Play(false);
            droplets.Play(false);
        }

        private void LateUpdate()
        {
            if (followTarget == null) return;
            if (!followTarget.gameObject.activeInHierarchy)
            {
                followTarget = null;
                return;
            }

            // Follow the animated body; world-space droplets keep their own trajectories.
            transform.position = followTarget.TransformPoint(localHitPoint);
        }

        public void Clear()
        {
            followTarget = null;
            impactSplash.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            droplets.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
