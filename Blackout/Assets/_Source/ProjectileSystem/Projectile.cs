using CombatSystem;
using UnityEngine;

namespace ProjectileSystem
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class Projectile : MonoBehaviour
    {
        [Header("Lifetime")]
        [SerializeField] private float lifetime = 3f;

        [Header("Hit")]
        [SerializeField] private LayerMask hitLayers;

        private ProjectilePool pool;
        private Rigidbody rigidbodyComponent;

        private int damage;
        private GameObject owner;
        private float deactivateTime;
        private bool isLaunched;

        private void Awake()
        {
            rigidbodyComponent = GetComponent<Rigidbody>();
            PrepareRigidbody();
        }

        private void Update()
        {
            if (!isLaunched)
                return;

            if (Time.time >= deactivateTime)
                ReturnToPool();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isLaunched)
                return;

            if (other.gameObject == owner)
                return;

            if (!IsInLayerMask(other.gameObject.layer, hitLayers))
                return;

            TryDealDamage(other);
            ReturnToPool();
        }

        public void Initialize(ProjectilePool projectilePool)
        {
            pool = projectilePool;
        }

        public void Launch(
            Vector3 position,
            Vector3 direction,
            float speed,
            int damage,
            GameObject owner
        )
        {
            this.damage = damage;
            this.owner = owner;

            transform.position = position;
            transform.rotation = Quaternion.LookRotation(direction.normalized);
            transform.SetParent(null);

            isLaunched = true;
            deactivateTime = Time.time + lifetime;

            rigidbodyComponent.linearVelocity = Vector3.zero;
            rigidbodyComponent.angularVelocity = Vector3.zero;
            rigidbodyComponent.linearVelocity = direction.normalized * speed;
        }

        private void TryDealDamage(Collider other)
        {
            Health health = other.GetComponentInParent<Health>();

            if (health == null)
                return;

            health.TakeDamage(damage);
        }

        private void ReturnToPool()
        {
            isLaunched = false;
            owner = null;

            rigidbodyComponent.linearVelocity = Vector3.zero;
            rigidbodyComponent.angularVelocity = Vector3.zero;

            pool.ReturnProjectile(this);
        }

        private void PrepareRigidbody()
        {
            rigidbodyComponent.useGravity = false;
            rigidbodyComponent.isKinematic = false;
            rigidbodyComponent.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        private bool IsInLayerMask(int layer, LayerMask layerMask)
        {
            return (layerMask.value & (1 << layer)) != 0;
        }
    }
}