using UnityEngine;

namespace Enemies.StateMachine
{
    public class TakingDamageState : EnemyState
    {
        public override void Enter()
        {
            
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
        
        public void PushAway(Vector3 pushFrom, float pushPower)
        {
            // Определяем в каком направлении должен отлететь объект
            // А также нормализуем этот вектор, чтобы можно было точно указать силу "отскока"
            var pushDirection = -(pushFrom - Core.transform.position);
            
            pushDirection.Normalize();

            // Толкаем объект в нужном направлении с силой pushPower
            Core.Rb.AddForce(pushDirection * pushPower);
        }
    }
}
