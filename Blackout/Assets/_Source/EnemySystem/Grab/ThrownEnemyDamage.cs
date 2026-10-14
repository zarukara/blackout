using CombatSystem;
using UnityEngine;

namespace EnemySystem
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Rigidbody))]
    public class ThrownEnemyDamage : MonoBehaviour
    {
        [Header("Layers")]
        [SerializeField] private LayerMask wallLayer;
        [SerializeField] private LayerMask enemyLayer;

        [Header("Damage")]
        [SerializeField] private int damageToSelfOnWall = 100;
        [SerializeField] private int damageToSelfOnEnemy = 100;
        [SerializeField] private int damageToOtherEnemy = 75;

        [Header("Throw")]
        [SerializeField] private float minImpactSpeed = 1f;
        [SerializeField] private float activeDuration = 2f;

        private Health health;

        private bool isActive;
        private bool hasHit;
        private float deactivateTime;

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        private void Update()
        {
            if (!isActive)
                return;

            if (Time.time >= deactivateTime)
                Deactivate();
        }

        public void Activate()
        {
            isActive = true;
            hasHit = false;
            deactivateTime = Time.time + activeDuration;
        }

        public void Deactivate()
        {
            isActive = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!isActive || hasHit)
                return;

            float impactSpeed = collision.relativeVelocity.magnitude;

            if (impactSpeed < minImpactSpeed)
                return;

            int otherLayer = collision.gameObject.layer;

            if (IsInLayerMask(otherLayer, wallLayer))
            {
                HitWall(collision);
                return;
            }

            if (IsInLayerMask(otherLayer, enemyLayer))
            {
                HitEnemy(collision);
            }
        }

        private void HitWall(Collision collision)
        {
            hasHit = true;
            ApplyImpactDamage(health, damageToSelfOnWall, collision, -collision.relativeVelocity);
            Deactivate();
        }

        private void HitEnemy(Collision collision)
        {
            Health otherHealth = collision.gameObject.GetComponentInParent<Health>();

            if (otherHealth == null || otherHealth == health)
                return;

            hasHit = true;

            ApplyImpactDamage(otherHealth, damageToOtherEnemy, collision, collision.relativeVelocity);
            ApplyImpactDamage(health, damageToSelfOnEnemy, collision, -collision.relativeVelocity);

            Deactivate();
        }

        private bool IsInLayerMask(int layer, LayerMask layerMask)
        {
            return (layerMask.value & (1 << layer)) != 0;
        }

        private static void ApplyImpactDamage(Health target, int damage, Collision collision, Vector3 direction)
        {
            Vector3 point = collision.contactCount > 0
                ? collision.GetContact(0).point
                : target.transform.position + Vector3.up;
            target.TakeDamage(damage, point, direction);
        }
    }
}
