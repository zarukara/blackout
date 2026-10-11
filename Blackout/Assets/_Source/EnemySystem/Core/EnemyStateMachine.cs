using System.Collections.Generic;
using UnityEngine;

namespace EnemySystem
{
    public class EnemyStateMachine : MonoBehaviour
    {
        private readonly Dictionary<EnemyStateId, IEnemyState> states = new();

        private IEnemyState currentState;
        private bool isInitialized;
        private EnemyContext context;

        public EnemyStateId CurrentStateId { get; private set; }

        private void Awake() => context = GetComponent<EnemyContext>();

        private void OnEnable()
        {
            if (!isInitialized || context == null || context.MeleeAttack == null) return;
            EnemyStateId state = context.Health.IsDead ? EnemyStateId.Dead :
                context.GrabHandler.IsGrabbed ? EnemyStateId.Grabbed :
                context.GrabHandler.IsThrown ? EnemyStateId.Thrown : EnemyStateId.Chase;
            ChangeState(state);
        }

        private void OnDisable()
        {
            if (context == null || context.MeleeAttack == null) return;
            currentState?.Exit();
            context.MeleeAttack.CancelAttack();
            currentState = null;
            context.Movement.DisableController();
        }

        public void RegisterState(IEnemyState state)
        {
            if (state == null)
                return;

            states[state.StateId] = state;
        }

        public void Initialize(EnemyStateId startState)
        {
            isInitialized = true;
            ChangeState(startState);
        }

        private void Update()
        {
            if (!isInitialized)
                return;

            if (context != null && context.MeleeAttack != null &&
                (context.Enemy == null || !context.Enemy.isActiveAndEnabled || !context.MeleeAttack.isActiveAndEnabled))
            {
                context.MeleeAttack.CancelAttack();
                context.Movement.Stop();
                return;
            }

            currentState?.Tick();
        }

        public void ChangeState(EnemyStateId stateId)
        {
            if (context != null && context.MeleeAttack != null && context.Health != null && context.Health.IsDead)
                stateId = EnemyStateId.Dead;
            if (context != null && context.MeleeAttack != null && stateId != EnemyStateId.MeleeAttack)
                context.MeleeAttack.CancelAttack();

            if (!states.TryGetValue(stateId, out IEnemyState nextState))
            {
                Debug.LogWarning($"Enemy state is not registered: {stateId}", this);
                return;
            }

            if (currentState != null && currentState.StateId == stateId)
                return;

            currentState?.Exit();

            currentState = nextState;
            CurrentStateId = stateId;

            currentState.Enter();
        }
    }
}
