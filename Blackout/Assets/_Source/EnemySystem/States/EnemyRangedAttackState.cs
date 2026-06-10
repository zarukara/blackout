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
        }

        public void Tick()
        {
            if (!context.HasTarget())
                return;

            if (context.RangedAttack == null)
            {
                context.StateMachine.ChangeState(EnemyStateId.Chase);
                return;
            }

            if (!context.RangedAttack.CanAttack(context.Target))
            {
                context.StateMachine.ChangeState(EnemyStateId.Chase);
                return;
            }

            context.Movement.RotateToTarget(context.Target);
            context.Movement.ApplyGravity();
            context.RangedAttack.TryAttack(context.Target);
        }

        public void Exit()
        {
        }
    }
}