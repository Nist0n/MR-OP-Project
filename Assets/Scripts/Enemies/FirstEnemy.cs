using System;
using System.Collections;
using Enemies.StateMachine;
using UnityEngine;
using UnityEngine.UI;

namespace Enemies
{
    public class FirstEnemy : EnemyCore
    {
        public FlyingState flying;
        
        public AttackingState attacking;
        
        public DeathState death;
        
        public TakingDamageState takingDamage;

        private void Start()
        {
            Health = MaxHealth;
            SetupInstances();
            Set(flying);
        }

        private void Update()
        {
            Health = Mathf.Clamp(Health, 0, MaxHealth);

            if (State.IsComplete)
            {
                if (IsAttacking)
                {
                    Set(attacking);
                }
                else
                {
                    if (IsDamaged)
                    {
                        Set(takingDamage);
                    }
                    else
                    {
                        Set(flying);
                    }
                }
            }
            
            if (Health <= 0)
            {
                Set(death);
                Invoke(nameof(KillEnemy), 1f);
            }
            
            if (Front.color.a != 0)
            {
                UpdateHpBar();
            }

            State.DoBranch();
        }
    }
}
