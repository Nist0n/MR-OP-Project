using UnityEngine;

namespace Enemies.StateMachine
{
    public class FlyingState : EnemyState
    {
        public override void Enter()
        {
            
        }
    
        public override void Do()
        {
            Core.transform.position = Vector3.MoveTowards(Core.transform.position,target.transform.position, speed * Time.deltaTime);
        }
    
        public override void Exit()
        {
        
        }
    }
}
