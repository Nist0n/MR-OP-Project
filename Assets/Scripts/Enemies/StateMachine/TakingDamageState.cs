using UnityEngine;

namespace Enemies.StateMachine
{
    public class TakingDamageState : EnemyState
    {
        public override void Enter()
        {
            StartCoroutine(Core.Invulnerability());
        }
    
        public override void Do()
        {
            if (!isDamaged)
            {
                IsComplete = true;
            }
        }
    
        public override void Exit()
        {
            
        }
    }
}
