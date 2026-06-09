namespace EnemySystem
{
    public class EnemyThrownState : IEnemyState
    {
        private readonly EnemyContext context;

        public EnemyStateId StateId => EnemyStateId.Thrown;

        public EnemyThrownState(EnemyContext context)
        {
            this.context = context;
        }

        public void Enter()
        {
            context.Movement.DisableController();
        }

        public void Tick()
        {
        }

        public void Exit()
        {
        }
    }
}