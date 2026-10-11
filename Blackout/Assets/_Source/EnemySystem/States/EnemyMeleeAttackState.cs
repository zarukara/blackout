namespace EnemySystem
{
    public class EnemyMeleeAttackState : IEnemyState
    {
        private readonly EnemyContext context;
        public EnemyStateId StateId => EnemyStateId.MeleeAttack;

        public EnemyMeleeAttackState(EnemyContext context) => this.context = context;

        public void Enter()
        {
            context.Movement.Stop();
            // Also supports initialization directly into this state.
            if (context.MeleeAttack != null && !context.MeleeAttack.IsAttacking)
                context.MeleeAttack.TryBeginAttack(context.Target);
        }

        public void Tick()
        {
            context.MeleeAttack?.TickAttack();
            if (context.Health.IsDead) return;
            if (context.MeleeAttack == null || !context.MeleeAttack.IsAttacking)
                context.StateMachine.ChangeState(EnemyStateId.Chase);
        }

        public void Exit() => context.MeleeAttack?.CancelAttack();
    }
}
