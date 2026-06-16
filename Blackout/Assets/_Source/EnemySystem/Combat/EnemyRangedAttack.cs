using ProjectileSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace EnemySystem
{
    public class EnemyRangedAttack : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ProjectilePool projectilePool;
        [SerializeField] private Transform firePoint;

        [Header("Stats")]
        [SerializeField] private int damage = 10;
        [SerializeField] private float projectileSpeed = 20f;
        [FormerlySerializedAs("attackDistance")]
        [SerializeField] private float attackRange = 12f;
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private float spreadAngle = 0f;

        private float nextAttackTime;
        private GameObject ownerObject;

        public float AttackRange => attackRange;

        private void Awake()
        {
            CacheReferences();
        }

        private void OnValidate()
        {
            CacheReferences();
        }

        public void Initialize(ProjectilePool projectilePool)
        {
            this.projectilePool = projectilePool;
        }

        public bool CanAttack(Transform target)
        {
            if (target == null)
                return false;

            if (Time.time < nextAttackTime)
                return false;

            return IsTargetInRange(target);
        }

        public void Attack(Transform target)
        {
            TryAttack(target);
        }

        public bool TryAttack(Transform target)
        {
            CacheReferences();

            if (!CanAttack(target))
                return false;

            if (projectilePool == null)
            {
                Debug.LogError("ProjectilePool is missing in EnemyRangedAttack.", this);
                return false;
            }

            if (firePoint == null)
            {
                Debug.LogError("FirePoint is missing in EnemyRangedAttack.", this);
                return false;
            }

            nextAttackTime = Time.time + attackCooldown;

            Vector3 direction = GetDirectionToTarget(target);

            Projectile projectile = projectilePool.GetProjectile();

            projectile.Launch(
                firePoint.position,
                direction,
                projectileSpeed,
                damage,
                ownerObject
            );

            return true;
        }

        public bool IsTargetInRange(Transform target)
        {
            if (target == null)
                return false;

            Vector3 directionToTarget = target.position - transform.position;
            directionToTarget.y = 0f;

            return directionToTarget.sqrMagnitude <= attackRange * attackRange;
        }

        private Vector3 GetDirectionToTarget(Transform target)
        {
            Vector3 origin = firePoint != null
                ? firePoint.position
                : transform.position;

            Vector3 direction = target.position - origin;
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.001f)
                direction = transform.forward;

            direction.Normalize();

            if (spreadAngle <= 0f)
                return direction;

            float randomAngle = Random.Range(-spreadAngle, spreadAngle);
            return (Quaternion.Euler(0f, randomAngle, 0f) * direction).normalized;
        }

        private void CacheReferences()
        {
            if (firePoint == null)
            {
                Transform foundFirePoint = transform.Find("ShootPoint");

                if (foundFirePoint != null)
                    firePoint = foundFirePoint;
            }

            if (ownerObject == null)
            {
                EnemyFacade enemyFacade = GetComponentInParent<EnemyFacade>();

                ownerObject = enemyFacade != null
                    ? enemyFacade.gameObject
                    : gameObject;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
