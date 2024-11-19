using System;
using Enemies.StateMachine;
using Player;
using UnityEngine;

namespace Enemies
{
    public class FirstEnemy : EnemyCore
    {
        private GameObject _player;
        
        public FlyingState flying;
        
        public AttackingState attacking;
        
        public DeathState death;
        
        public TakingDamageState takingDamage;

        private void Start()
        {
            NotRefreshing = true;
            _player = GameObject.FindGameObjectWithTag("Player");
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

            float sqrDistance = (position - transform.position).sqrMagnitude;

            if (sqrDistance > 0.5f || !NotRefreshing) // Not close enough
            {
                return;
            }

            IsAttacking = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Attached");
            if (other.CompareTag("Player"))
            {
                Debug.Log("Player");
                other.GetComponent<PlayerConfig>().ReceiveDamage(Damage);
            }
        }
    }
}
