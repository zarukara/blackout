namespace EnemySystem
{
    public class EnemyChaseState : IEnemyState
    {
        private readonly EnemyContext context;

        public EnemyStateId StateId =>
            EnemyStateId.Chase;

        public EnemyChaseState(
            EnemyContext context)
        {
            this.context = context;
        }

        public void Enter()
        {
            context.Movement.EnableController();
        }

        public void Tick()
        {
            if (context.MeleeAttack != null)
            {
                context.MeleeAttack.TickChase(context.Target);
                return;
            }

            if (!context.HasTarget())
                return;
            if (context.RangedAttack != null &&
                context.RangedAttack.IsTargetInRange(
                    context.Target))
            {
                context.StateMachine.ChangeState(
                    EnemyStateId.RangedAttack
                );

                return;
            }

            context.Movement.MoveToTarget(
                context.Target
            );
        }

        public void Exit()
        {
        }
    }
}
