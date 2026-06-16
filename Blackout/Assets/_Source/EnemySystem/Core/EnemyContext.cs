using CombatSystem;
using UnityEngine;

namespace EnemySystem
{
    [DisallowMultipleComponent]
    public class EnemyContext : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Core")]
        [SerializeField] private Health health;
        [SerializeField] private EnemyMovement movement;
        [SerializeField] private EnemyMeleeAttack meleeAttack;
        [SerializeField] private EnemyRangedAttack rangedAttack;
        [SerializeField] private EnemyGrabHandler grabHandler;

        public Transform Target => target;

        public AEnemy Enemy { get; private set; }
        public Health Health => health;
        public CharacterController CharacterController { get; private set; }
        public Rigidbody Rigidbody { get; private set; }
        public EnemyMovement Movement => movement;
        public EnemyMeleeAttack MeleeAttack => meleeAttack;
        public EnemyRangedAttack RangedAttack => rangedAttack;
        public EnemyGrabHandler GrabHandler => grabHandler;
        public EnemyStateMachine StateMachine { get; private set; }
        public ThrownEnemyDamage ThrownEnemyDamage { get; private set; }

        private void Awake()
        {
            CacheReferences();
        }

        private void OnValidate()
        {
            CacheReferences();
        }

        [ContextMenu("Cache References")]
        public void CacheReferences()
        {
            if (Enemy == null)
                Enemy = GetComponentInChildren<AEnemy>(true);

            if (health == null)
                health = GetComponentInChildren<Health>(true);

            if (CharacterController == null)
                CharacterController = GetComponentInChildren<CharacterController>(true);

            if (Rigidbody == null)
                Rigidbody = GetComponentInChildren<Rigidbody>(true);

            if (movement == null)
                movement = GetComponentInChildren<EnemyMovement>(true);

            if (meleeAttack == null)
                meleeAttack = GetComponentInChildren<EnemyMeleeAttack>(true);

            if (rangedAttack == null)
                rangedAttack = GetComponentInChildren<EnemyRangedAttack>(true);

            if (grabHandler == null)
                grabHandler = GetComponentInChildren<EnemyGrabHandler>(true);

            if (StateMachine == null)
                StateMachine = GetComponentInChildren<EnemyStateMachine>(true);

            if (ThrownEnemyDamage == null)
                ThrownEnemyDamage = GetComponentInChildren<ThrownEnemyDamage>(true);
        }

        public void Initialize(AEnemy enemy, Transform target)
        {
            Enemy = enemy;
            this.target = target;
            CacheReferences();
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public bool HasTarget()
        {
            return target != null;
        }

        public bool IsValid()
        {
            return health != null
                   && movement != null
                   && grabHandler != null;
        }
    }
}
