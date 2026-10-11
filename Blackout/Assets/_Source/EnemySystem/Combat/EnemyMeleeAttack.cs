using System;
using CombatSystem;
using UnityEngine;

namespace EnemySystem
{
    public enum MeleeAttackPhase { Idle, Windup, Strike, Recovery }

    [DisallowMultipleComponent]
    public class EnemyMeleeAttack : MonoBehaviour
    {
        [Header("Attack")]
        [SerializeField] private int damage = 10;
        [SerializeField, Min(0.1f)] private float attackDistance = 1.8f;
        [SerializeField, Min(0f)] private float attackCooldown = 1f;
        [SerializeField, Range(1f, 180f)] private float attackAngle = 90f;
        [SerializeField, Min(0f)] private float maxHeightDifference = 1f;
        [SerializeField] private LayerMask obstructionLayers = ~0;

        [Header("Phases")]
        [SerializeField, Min(0.01f)] private float windupDuration = 0.35f;
        [SerializeField, Min(0.01f)] private float strikeDuration = 0.15f;
        [SerializeField, Min(0.01f)] private float recoveryDuration = 0.5f;
        [SerializeField, Range(0f, 1f)] private float aimLockFraction = 0.65f;
        [SerializeField, Min(0f)] private float windupTurnSpeed = 180f;
        [Tooltip("Fraction of Strike at which the single contact check occurs; fallback needs no animation event.")]
        [SerializeField, Range(0f, 1f)] private float contactFraction = 0.5f;
        [SerializeField] private bool enableLunge = true;
        [SerializeField, Min(0f)] private float lungeDistance = 0.4f;

        private EnemyContext context;
        private Health health;
        private Health targetHealth;
        private CharacterController targetController;
        private EnemyMovement movement;
        private MeleeApproach approach;
        private MeleePlayerContact contact;
        private MeleeAttackCoordinator coordinator;
        private readonly RaycastHit[] obstructionHits = new RaycastHit[64];
        private float nextAttackTime;
        private float phaseStarted;
        private float lungeProgress;
        private Vector3 lockedDirection;
        private bool contactChecked;

        public float AttackDistance => attackDistance;
        public Transform Target { get; private set; }
        public MeleeAttackPhase Phase { get; private set; }
        public bool IsAttacking => Phase != MeleeAttackPhase.Idle;
        public bool IsAvailable => isActiveAndEnabled && health != null && !health.IsDead &&
            context != null && context.Enemy != null && context.Enemy.isActiveAndEnabled &&
            context.StateMachine != null && context.StateMachine.isActiveAndEnabled &&
            (context.GrabHandler == null || (!context.GrabHandler.IsGrabbed && !context.GrabHandler.IsThrown));

        // Existing subscribers use this to trigger the animation. It means Windup started.
        public event Action AttackPerformed;
        public event Action AttackCancelled;

        private void Awake()
        {
            context = GetComponent<EnemyContext>();
            health = GetComponent<Health>();
            movement = GetComponent<EnemyMovement>();
            approach = GetComponent<MeleeApproach>();
            if (approach == null) approach = gameObject.AddComponent<MeleeApproach>();
            contact = GetComponent<MeleePlayerContact>();
            if (contact == null) contact = gameObject.AddComponent<MeleePlayerContact>();
        }

        private void OnEnable()
        {
            if (health != null) health.Died += CancelAttack;
            Phase = MeleeAttackPhase.Idle;
            Target = null;
            nextAttackTime = Time.time;
        }

        private void OnDisable()
        {
            if (health != null) health.Died -= CancelAttack;
            CancelAttack();
            contact?.SetTarget(null);
            approach?.ResetApproach();
            Target = null;
            targetHealth = null;
        }

        public bool CanAttack(Transform target)
        {
            if (target == null || !target.gameObject.activeInHierarchy) return false;
            Vector3 direction = target.position - transform.position;
            CharacterController body = target == Target ? targetController : target.GetComponentInParent<CharacterController>();
            float targetHeight = body != null ? body.bounds.center.y : target.position.y + 0.9f;
            if (Mathf.Abs(targetHeight - (transform.position.y + 0.9f)) > maxHeightDifference) return false;
            direction.y = 0f;
            return direction.sqrMagnitude <= attackDistance * attackDistance;
        }

        public void TickChase(Transform target)
        {
            BindTarget(target);
            if (!IsAvailable || !HasLivingTarget())
            {
                CancelAttack();
                movement.Stop();
                return;
            }
            bool near = (target.position - transform.position).sqrMagnitude < 25f;
            bool mayAttack = false;
            if (near && Time.time >= nextAttackTime)
                mayAttack = coordinator.RequestAttack(this);
            else coordinator.Release(this);

            if (mayAttack && CanAttack(target) && HasLineOfSight() && TryBeginAttack(target))
            {
                context.StateMachine.ChangeState(EnemyStateId.MeleeAttack);
                return;
            }
            approach.Tick(target, mayAttack);
        }

        public void TryAttack(Transform target)
        {
            if (TryBeginAttack(target)) context.StateMachine.ChangeState(EnemyStateId.MeleeAttack);
        }

        public bool TryBeginAttack(Transform target)
        {
            BindTarget(target);
            if (!IsAvailable || IsAttacking || Time.time < nextAttackTime || !HasLivingTarget() ||
                !CanAttack(target) || !movement.IsReady || !HasLineOfSight()) return false;
            if (!coordinator.TryAcquire(this, windupDuration + strikeDuration + recoveryDuration)) return false;

            movement.Stop();
            Phase = MeleeAttackPhase.Windup;
            phaseStarted = Time.time;
            nextAttackTime = Time.time + attackCooldown;
            lockedDirection = transform.forward;
            contactChecked = false;
            lungeProgress = 0f;
            AttackPerformed?.Invoke();
            return IsAttacking && IsAvailable;
        }

        public void TickAttack()
        {
            if (!IsAttacking) return;
            if (!IsAvailable || !HasLivingTarget() || context.Target != Target ||
                coordinator == null || !coordinator.OwnsPermission(this))
            {
                CancelAttack();
                return;
            }

            float elapsed = Time.time - phaseStarted;
            if (Phase == MeleeAttackPhase.Windup)
            {
                if (elapsed < windupDuration * aimLockFraction)
                {
                    Vector3 direction = Target.position - transform.position;
                    direction.y = 0f;
                    if (direction.sqrMagnitude > 0.001f)
                        transform.rotation = Quaternion.RotateTowards(transform.rotation,
                            Quaternion.LookRotation(direction), windupTurnSpeed * Time.deltaTime);
                    lockedDirection = transform.forward;
                }
                if (elapsed >= windupDuration)
                {
                    Phase = MeleeAttackPhase.Strike;
                    phaseStarted = Time.time;
                }
                return;
            }

            if (Phase == MeleeAttackPhase.Strike)
            {
                float progress = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, strikeDuration));
                if (enableLunge && progress > lungeProgress)
                    movement.MoveAttack(lockedDirection, lungeDistance * (progress - lungeProgress), Target, obstructionLayers);
                lungeProgress = progress;
                if (!contactChecked && progress >= contactFraction)
                {
                    contactChecked = true;
                    Vector3 direction = Target.position - transform.position;
                    direction.y = 0f;
                    if (CanAttack(Target) && Vector3.Angle(lockedDirection, direction) <= attackAngle * 0.5f && HasLineOfSight())
                        targetHealth.TakeDamage(damage);
                    // Damage listeners can destroy the target or disable/cancel the attacker.
                    if (!IsAvailable || !IsAttacking || !HasLivingTarget())
                    {
                        CancelAttack();
                        return;
                    }
                }
                if (elapsed >= strikeDuration)
                {
                    Phase = MeleeAttackPhase.Recovery;
                    phaseStarted = Time.time;
                    movement.Stop();
                }
                return;
            }

            if (elapsed >= recoveryDuration)
            {
                Phase = MeleeAttackPhase.Idle;
                coordinator.Release(this);
            }
        }

        public void CancelAttack()
        {
            bool wasAttacking = IsAttacking;
            Phase = MeleeAttackPhase.Idle;
            coordinator?.Release(this);
            if (wasAttacking)
            {
                movement?.Stop();
                AttackCancelled?.Invoke();
            }
        }

        private void BindTarget(Transform target)
        {
            contact.SetTarget(target);
            if (Target == target && (target == null || coordinator != null)) return;
            CancelAttack();
            Target = target;
            targetHealth = target != null ? target.GetComponentInParent<Health>() : null;
            targetController = target != null ? target.GetComponentInParent<CharacterController>() : null;
            coordinator = null;
            approach.ResetApproach();
            if (target != null)
            {
                coordinator = target.GetComponent<MeleeAttackCoordinator>();
                if (coordinator == null) coordinator = target.gameObject.AddComponent<MeleeAttackCoordinator>();
            }
        }

        private bool HasLivingTarget() => Target != null && Target.gameObject.activeInHierarchy &&
            targetHealth != null && !targetHealth.IsDead;

        private bool HasLineOfSight()
        {
            Vector3 start = transform.position + Vector3.up * 0.9f;
            Vector3 end = targetController != null ? targetController.bounds.center : Target.position + Vector3.up * 0.9f;
            Vector3 direction = end - start;
            int count = Physics.RaycastNonAlloc(start, direction.normalized, obstructionHits, direction.magnitude,
                obstructionLayers, QueryTriggerInteraction.Ignore);
            if (count == obstructionHits.Length) return false;
            for (int i = 0; i < count; i++)
            {
                Transform hit = obstructionHits[i].transform;
                if (!hit.IsChildOf(transform) && !hit.IsChildOf(targetHealth.transform)) return false;
            }
            return true;
        }

        private void OnValidate()
        {
            attackDistance = Mathf.Max(0.1f, attackDistance);
            windupDuration = Mathf.Max(0.01f, windupDuration);
            strikeDuration = Mathf.Max(0.01f, strikeDuration);
            recoveryDuration = Mathf.Max(0.01f, recoveryDuration);
        }
    }
}
