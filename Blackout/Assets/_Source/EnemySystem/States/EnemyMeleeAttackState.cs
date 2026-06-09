namespace EnemySystem
{
    public class EnemyMeleeAttackState : IEnemyState
    {
        private readonly EnemyContext context;

        public EnemyStateId StateId => EnemyStateId.MeleeAttack;

        public EnemyMeleeAttackState(EnemyContext context)
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

            if (context.MeleeAttack == null)
            {
                context.StateMachine.ChangeState(EnemyStateId.Chase);
                return;
            }

            if (!context.MeleeAttack.CanAttack(context.Target))
            {
                context.StateMachine.ChangeState(EnemyStateId.Chase);
                return;
            }

            context.Movement.RotateToTarget(context.Target);
            context.Movement.ApplyGravity();
            context.MeleeAttack.TryAttack(context.Target);
        }

        public void Exit()
        {
        }
    }
}