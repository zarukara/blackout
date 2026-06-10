namespace EnemySystem
{
    public class EnemyActor : AEnemy
    {
        protected override void RegisterStates()
        {
            StateMachine.RegisterState(new EnemyChaseState(Context));
            StateMachine.RegisterState(new EnemyMeleeAttackState(Context));
            StateMachine.RegisterState(new EnemyRangedAttackState(Context));
            StateMachine.RegisterState(new EnemyGrabbedState(Context));
            StateMachine.RegisterState(new EnemyThrownState(Context));
            StateMachine.RegisterState(new EnemyDeadState(Context));
        }
    }
}