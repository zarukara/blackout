namespace EnemySystem
{
    public class EnemyChaseState : IEnemyState
    {
        private readonly EnemyContext context;

        public EnemyStateId StateId => EnemyStateId.Chase;

        public EnemyChaseState(EnemyContext context)
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

            if (context.MeleeAttack != null && context.MeleeAttack.CanAttack(context.Target))
            {
                context.StateMachine.ChangeState(EnemyStateId.MeleeAttack);
                return;
            }

            if (context.RangedAttack != null && context.RangedAttack.CanAttack(context.Target))
            {
                context.StateMachine.ChangeState(EnemyStateId.RangedAttack);
                return;
            }

            context.Movement.MoveToTarget(context.Target);
        }

        public void Exit()
        {
        }
    }
}