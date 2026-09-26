namespace EnemySystem
{
    public class EnemyRangedAttackState : IEnemyState
    {
        private readonly EnemyContext context;

        public EnemyStateId StateId => EnemyStateId.RangedAttack;

        public EnemyRangedAttackState(EnemyContext context)
        {
            this.context = context;
        }

        public void Enter()
        {
            context.Movement.EnableController();
            context.Movement.Stop();
        }

        public void Tick()
        {
            if (!context.HasTarget())
                return;

            if (context.RangedAttack == null)
            {
                context.StateMachine.ChangeState(
                    EnemyStateId.Chase
                );

                return;
            }

            if (!context.RangedAttack.IsTargetInRange(context.Target))
            {
                context.StateMachine.ChangeState(
                    EnemyStateId.Chase
                );

                return;
            }

            context.Movement.RotateToTarget(
                context.Target
            );

            context.RangedAttack.TryAttack(
                context.Target
            );
        }

        public void Exit()
        {
        }
    }
}