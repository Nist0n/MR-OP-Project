using System;
using Enemies.StateMachine;

namespace Enemies
{
    public class FirstEnemy : EnemyCore
    {
        public FlyingState flyingState;
        
        private void Start()
        {
            SetupInstances();
            Set(flyingState);
        }

        private void Update()
        {
            State.DoBranch();
        }
    }
}
