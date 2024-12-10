namespace Enemies.StateMachine
{
    public class DeathState : EnemyState
    {
        public override void Enter()
        {
            
        }
    
        public override void Do()
        {
            animationControl.Death();
        }
    
        public override void Exit()
        {
            
        }
    }
}
