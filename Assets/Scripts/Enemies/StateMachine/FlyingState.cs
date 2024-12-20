using Audio;
using UnityEngine;

namespace Enemies.StateMachine
{
    public class FlyingState : EnemyState
    {
        public override void Enter()
        {
            Core.NotRefreshing = true;
            AudioManager.instance.PlaySFX("Flying");
        }
    
        public override void Do()
        {
            animationControl.Fly();
            Core.EnemyObject.transform.position = Vector3.MoveTowards(Core.EnemyObject.transform.position,target.transform.position, speed * Time.deltaTime);
            if (isDamaged || isAttacking)
            {
                IsComplete = true;
            }
        }
    
        public override void Exit()
        {

        }
    }
}
