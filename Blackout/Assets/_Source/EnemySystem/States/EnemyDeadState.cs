using UnityEngine;

namespace EnemySystem
{
    public class EnemyDeadState : IEnemyState
    {
        private readonly EnemyContext context;

        public EnemyStateId StateId => EnemyStateId.Dead;

        public EnemyDeadState(EnemyContext context)
        {
            this.context = context;
        }

        public void Enter()
        {
            if (context.Movement != null)
                context.Movement.DisableController();

            if (context.Rigidbody != null)
            {
                if (!context.Rigidbody.isKinematic)
                {
                    context.Rigidbody.linearVelocity = Vector3.zero;
                    context.Rigidbody.angularVelocity = Vector3.zero;
                }

                context.Rigidbody.isKinematic = true;
                context.Rigidbody.useGravity = false;
            }
        }

        public void Tick()
        {
        }

        public void Exit()
        {
        }
    }
}