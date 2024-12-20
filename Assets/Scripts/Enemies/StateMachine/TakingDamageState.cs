using Audio;
using UnityEngine;

namespace Enemies.StateMachine
{
    public class TakingDamageState : EnemyState
    {
        public override void Enter()
        {
            StartCoroutine(Core.Invulnerability());
            AudioManager.instance.PlaySFX("TakingDamage");
        }
    
        public override void Do()
        {
            animationControl.TakeDamage();
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
