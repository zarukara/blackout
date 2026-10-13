using UnityEngine;
using UnityEngine.AI;

namespace EnemySystem
{
    [DisallowMultipleComponent]
    public class EnemyAnimationController : MonoBehaviour
    {
        private static readonly int SpeedHash =
            Animator.StringToHash("Speed");

        private static readonly int IsInCombatHash =
            Animator.StringToHash("IsInCombat");

        private static readonly int AttackHash =
            Animator.StringToHash("Attack");
        private static readonly int LocomotionSpeedHash = Animator.StringToHash("LocomotionSpeed");

        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private EnemyMeleeAttack meleeAttack;

        [Header("Locomotion")]
        [SerializeField] private float speedDampTime = 0.1f;
        [SerializeField, Min(.01f)] private float locomotionReferenceSpeed = 3.8f;

        [Header("Combat")]
        [SerializeField] private bool startInCombat = true;

        private bool isSubscribed;
        private bool hasSpeed;
        private bool hasCombat;
        private bool hasAttack;
        private bool hasLocomotionSpeed;

        private void Awake()
        {
            CacheReferences();
        }

        private void OnEnable()
        {
            CacheReferences();
            CacheParameters();
            Subscribe();

            SetCombat(startInCombat);
            if (animator != null) animator.applyRootMotion = false;
        }

        private void Update()
        {
            UpdateMovement();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnValidate()
        {
            CacheReferences();
        }

        [ContextMenu("Cache References")]
        public void CacheReferences()
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>(true);

            if (navMeshAgent == null)
                navMeshAgent = GetComponent<NavMeshAgent>();

            if (meleeAttack == null)
                meleeAttack = GetComponent<EnemyMeleeAttack>();
        }

        public void SetCombat(bool value)
        {
            if (animator == null || !hasCombat)
                return;

            animator.SetBool(
                IsInCombatHash,
                value
            );
        }

        public void PlayAttack()
        {
            if (animator == null || !hasAttack)
                return;

            animator.ResetTrigger(AttackHash);
            animator.SetTrigger(AttackHash);
        }

        private void UpdateMovement()
        {
            if (animator == null || !hasSpeed)
                return;

            float speed = 0f;

            if (navMeshAgent != null &&
                navMeshAgent.enabled &&
                navMeshAgent.isOnNavMesh && !navMeshAgent.isStopped)
            {
                Vector3 velocity =
                    navMeshAgent.velocity;

                velocity.y = 0f;

                speed = velocity.magnitude;
            }

            animator.SetFloat(
                SpeedHash,
                speed,
                speedDampTime,
                Time.deltaTime
            );
            if (hasLocomotionSpeed)
                animator.SetFloat(LocomotionSpeedHash, Mathf.Max(.05f, speed / locomotionReferenceSpeed), speedDampTime, Time.deltaTime);
        }

        private void Subscribe()
        {
            if (isSubscribed ||
                meleeAttack == null)
            {
                return;
            }

            meleeAttack.AttackPerformed +=
                OnAttackPerformed;
            meleeAttack.AttackCancelled += OnAttackCancelled;

            isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!isSubscribed)
                return;

            if (meleeAttack != null)
            {
                meleeAttack.AttackPerformed -=
                    OnAttackPerformed;
                meleeAttack.AttackCancelled -= OnAttackCancelled;
            }

            isSubscribed = false;
        }

        private void OnAttackPerformed()
        {
            PlayAttack();
        }

        private void OnAttackCancelled()
        {
            if (animator == null || !hasAttack || !animator.isActiveAndEnabled) return;
            animator.ResetTrigger(AttackHash);
            int idle = Animator.StringToHash("Base Layer.IdleCombat");
            if (animator.HasState(0, idle)) animator.CrossFadeInFixedTime(idle, .05f);
        }

        private void CacheParameters()
        {
            hasSpeed = hasCombat = hasAttack = hasLocomotionSpeed = false;
            if (animator == null || animator.runtimeAnimatorController == null) return;
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.nameHash == SpeedHash && parameter.type == AnimatorControllerParameterType.Float) hasSpeed = true;
                if (parameter.nameHash == IsInCombatHash && parameter.type == AnimatorControllerParameterType.Bool) hasCombat = true;
                if (parameter.nameHash == AttackHash && parameter.type == AnimatorControllerParameterType.Trigger) hasAttack = true;
                if (parameter.nameHash == LocomotionSpeedHash && parameter.type == AnimatorControllerParameterType.Float) hasLocomotionSpeed = true;
            }
        }
    }
}
