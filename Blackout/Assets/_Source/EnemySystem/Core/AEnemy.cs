using CombatSystem;
using UnityEngine;

namespace EnemySystem
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(EnemyContext))]
    [RequireComponent(typeof(EnemyStateMachine))]
    public abstract class AEnemy : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("State")]
        [SerializeField] private EnemyStateId startState = EnemyStateId.Chase;

        protected EnemyContext Context { get; private set; }
        protected EnemyStateMachine StateMachine { get; private set; }
        protected Health Health { get; private set; }

        protected virtual void Awake()
        {
            Context = GetComponent<EnemyContext>();
            StateMachine = GetComponent<EnemyStateMachine>();
            Health = GetComponent<Health>();

            Context.Initialize(this, target);

            RegisterStates();
        }

        protected virtual void OnEnable()
        {
            if (Health != null)
                Health.Died += OnDied;
        }

        protected virtual void Start()
        {
            StateMachine.Initialize(startState);
        }

        protected virtual void OnDisable()
        {
            if (Health != null)
                Health.Died -= OnDied;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;

            if (Context != null)
                Context.SetTarget(newTarget);
        }

        protected abstract void RegisterStates();

        private void OnDied()
        {
            if (StateMachine != null)
                StateMachine.ChangeState(EnemyStateId.Dead);
        }
    }
}