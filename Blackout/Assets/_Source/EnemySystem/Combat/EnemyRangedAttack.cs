using CombatSystem;
using ProjectileSystem;
using UnityEngine;
using UnityEngine.Serialization;
using WeaponSystem;

namespace EnemySystem
{
    public class EnemyRangedAttack : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ProjectilePool projectilePool;
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform weaponVisual;
        [SerializeField] private Transform aimPivot;
        [SerializeField] private MuzzleFlashVfx muzzleFlash;

        [Header("Stats")]
        [SerializeField] private int damage = 10;
        [SerializeField] private float projectileSpeed = 20f;
        [FormerlySerializedAs("attackDistance")]
        [SerializeField] private float attackRange = 12f;
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private float spreadAngle = 0f;

        private float nextAttackTime;
        private GameObject ownerObject;
        private EnemyContext context;
        private Transform pendingTarget;
        private bool reportedMissingDependency;
        private Quaternion restingWeaponRotation;

        public float AttackRange => attackRange;

        private void Awake()
        {
            CacheReferences();
            context = GetComponent<EnemyContext>();
            if (weaponVisual != null) restingWeaponRotation = weaponVisual.localRotation;
        }

        private void OnValidate()
        {
            CacheReferences();
        }

        public void Initialize(ProjectilePool projectilePool)
        {
            if (projectilePool != null) this.projectilePool = projectilePool;
            reportedMissingDependency = false;
        }

        private void Update()
        {
            if (weaponVisual != null) weaponVisual.localRotation = restingWeaponRotation;
        }

        private void OnDisable()
        {
            pendingTarget = null;
            if (weaponVisual != null) weaponVisual.localRotation = restingWeaponRotation;
        }

        private void LateUpdate()
        {
            Transform target = context != null ? context.Target : pendingTarget;
            if (target == null || Time.timeScale <= 0f ||
                (context != null && (!context.HasTarget() || context.Health.IsDead || !context.StateMachine.isActiveAndEnabled ||
                context.StateMachine.CurrentStateId != EnemyStateId.RangedAttack)))
            {
                pendingTarget = null;
                return;
            }
            if (firePoint == null) return;
            if (!CombatAim.IsFacing(transform, GetAimPoint(target), 10f)) { pendingTarget = null; return; }
            CombatAim.AlignMuzzle(weaponVisual, firePoint, GetAimPoint(target), aimPivot);
            if (pendingTarget == null) return;
            target = pendingTarget;
            pendingTarget = null;
            if (!target.gameObject.activeInHierarchy || !IsTargetInRange(target) || projectilePool == null) return;
            Projectile projectile = projectilePool.GetProjectile();
            if (projectile == null) return;

            projectile.Launch(firePoint.position, GetDirectionToTarget(target), projectileSpeed, damage, ownerObject);

            if (muzzleFlash != null)
                muzzleFlash.Play();
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
            if (!CanAttack(target))
                return false;

            if (projectilePool == null || firePoint == null)
            {
                if (!reportedMissingDependency)
                    Debug.LogError("EnemyRangedAttack needs the shared ProjectilePool and a muzzle. Initialize via EnemyFacade/Bootstrapper.", this);
                reportedMissingDependency = true;
                return false;
            }

            if (!CombatAim.IsFacing(transform, GetAimPoint(target), 5f)) return false;

            nextAttackTime = Time.time + attackCooldown;
            pendingTarget = target;

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
            Vector3 origin = firePoint.position;

            Vector3 direction = GetAimPoint(target) - origin;
            if (direction.sqrMagnitude < 0.001f) direction = transform.forward;
            direction.Normalize();
            if (spreadAngle > 0f)
                direction = Quaternion.AngleAxis(Random.Range(-spreadAngle, spreadAngle), Vector3.up) * direction;
            return direction.normalized;
        }

        private Vector3 GetAimPoint(Transform target)
        {
            CharacterController controller =
                target.GetComponentInChildren<CharacterController>();

            Collider targetCollider =
                target.GetComponentInChildren<Collider>();

            return controller != null
                ? controller.bounds.center
                : targetCollider != null
                    ? targetCollider.bounds.center
                    : target.position + Vector3.up;

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
