using System.Collections.Generic;
using UnityEngine;

namespace EnemySystem
{
    public class EnemyStateMachine : MonoBehaviour
    {
        private readonly Dictionary<EnemyStateId, IEnemyState> states = new();

        private IEnemyState currentState;
        private bool isInitialized;

        public EnemyStateId CurrentStateId { get; private set; }

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

            currentState?.Tick();
        }

        public void ChangeState(EnemyStateId stateId)
        {
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