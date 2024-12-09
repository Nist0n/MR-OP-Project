using System;
using Enemies.StateMachine;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace Enemies
{
    public class FirstEnemy : EnemyCore
    {
        public FlyingState Flying;
        
        public AttackingState Attacking;
        
        public DeathState Death;
        
        public TakingDamageState TakingDamage;

        private void Start()
        {
            Target = GameObject.FindGameObjectWithTag("Target");
            NotRefreshing = true;
            SetStats();
            SetupInstances();
            Set(Flying);
        }

        private void Update()
        {
            EnemyObject.transform.LookAt(Target.transform);
            
            Health = Mathf.Clamp(Health, 0, MaxHealth);

            if (State.IsComplete)
            {
                if (IsDamaged)
                {
                    Set(TakingDamage);
                }
                else
                {
                    if (IsAttacking)
                    {
                        Set(Attacking);
                    }
                    else
                    {
                        Set(Flying);
                    }
                }
            }
            
            PlayerInRange();
            
            if (Health <= 0)
            {
                Set(Death);
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
