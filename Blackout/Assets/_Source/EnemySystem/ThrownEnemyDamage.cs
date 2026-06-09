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
            {
                Deactivate();
            }
        }

        public void Activate()
        {
            isActive = true;
            hasHit = false;
            deactivateTime = Time.time + activeDuration;
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
                HitWall();
                return;
            }

            if (IsInLayerMask(otherLayer, enemyLayer))
            {
                HitEnemy(collision);
            }
        }

        private void HitWall()
        {
            hasHit = true;
            health.TakeDamage(damageToSelfOnWall);
            Deactivate();
        }

        private void HitEnemy(Collision collision)
        {
            Health otherHealth = collision.gameObject.GetComponentInParent<Health>();

            if (otherHealth == null || otherHealth == health)
                return;

            hasHit = true;

            otherHealth.TakeDamage(damageToOtherEnemy);
            health.TakeDamage(damageToSelfOnEnemy);

            Deactivate();
        }

        private void Deactivate()
        {
            isActive = false;
        }

        private bool IsInLayerMask(int layer, LayerMask layerMask)
        {
            return (layerMask.value & (1 << layer)) != 0;
        }
    }
}