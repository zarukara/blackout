namespace EnemySystem
{
    public class EnemyGrabbedState : IEnemyState
    {
        private readonly EnemyContext context;

        public EnemyStateId StateId => EnemyStateId.Grabbed;

        public EnemyGrabbedState(EnemyContext context)
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