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

        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private EnemyMeleeAttack meleeAttack;

        [Header("Locomotion")]
        [SerializeField] private float speedDampTime = 0.1f;

        [Header("Combat")]
        [SerializeField] private bool startInCombat = true;

        private bool isSubscribed;

        private void Awake()
        {
            CacheReferences();
        }

        private void OnEnable()
        {
            CacheReferences();
            Subscribe();

            SetCombat(startInCombat);
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
            if (animator == null)
                return;

            animator.SetBool(
                IsInCombatHash,
                value
            );
        }

        public void PlayAttack()
        {
            if (animator == null)
                return;

            animator.ResetTrigger(AttackHash);
            animator.SetTrigger(AttackHash);
        }

        private void UpdateMovement()
        {
            if (animator == null)
                return;

            float speed = 0f;

            if (navMeshAgent != null &&
                navMeshAgent.enabled &&
                navMeshAgent.isOnNavMesh)
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
            }

            isSubscribed = false;
        }

        private void OnAttackPerformed()
        {
            PlayAttack();
        }
    }
}