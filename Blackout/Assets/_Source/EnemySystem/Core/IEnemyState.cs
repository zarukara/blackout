namespace EnemySystem
{
    public interface IEnemyState
    {
        EnemyStateId StateId { get; }

        void Enter();
        void Tick();
        void Exit();
    }
}