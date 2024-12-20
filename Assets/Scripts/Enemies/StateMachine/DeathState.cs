using Audio;

namespace Enemies.StateMachine
{
    public class DeathState : EnemyState
    {
        public override void Enter()
        {
            AudioManager.instance.PlaySFX("Death");
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
