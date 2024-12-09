using System;
using Enemies.StateMachine;
using Player;
using UnityEngine;

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
            Target = GameObject.FindGameObjectWithTag("Target");
            NotRefreshing = true;
            SetStats();
            SetupInstances();
            Set(flying);
        }

        private void Update()
        {
            EnemyObject.transform.LookAt(Target.transform);
            
            Health = Mathf.Clamp(Health, 0, MaxHealth);

            if (State.IsComplete)
            {
                if (IsDamaged)
                {
                    Set(takingDamage);
                }
                else
                {
                    if (IsAttacking)
                    {
                        Set(attacking);
                    }
                    else
                    {
                        Set(flying);
                    }
                }
            }
            
            PlayerInRange();
            
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

        private void PlayerInRange()
        {
            var position = Target.transform.position;

            float sqrDistance = (position - EnemyObject.transform.position).sqrMagnitude;

            if (sqrDistance > 0.5f || !NotRefreshing) // Not close enough
            {
                return;
            }

            IsAttacking = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Target"))
            {
                Debug.Log("Player");
                other.GetComponent<PlayerConfig>().ReceiveDamage(Damage);
            }
        }
    }
}
