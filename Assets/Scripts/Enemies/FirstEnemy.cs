using System;
using System.Collections;
using Enemies.StateMachine;
using UnityEngine;
using UnityEngine.UI;

namespace Enemies
{
    public class FirstEnemy : EnemyCore
    {
        [SerializeField] private Image back;
        
        [SerializeField] private Image front;
        
        public FlyingState flying;
        
        public AttackingState attacking;
        
        public DeathState death;
        
        public TakingDamageState takingDamage;
        
        private Coroutine _hpBarCoroutine;
        
        private float _lerpTimer;
        
        private void Start()
        {
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
                Invoke("KillEnemy", 1f);
            }
            
            if (front.color.a != 0)
            {
                UpdateHpBar();
            }

            State.DoBranch();
        }
        
        private IEnumerator ReceiveDamage(float damage)
        {
            IsDamaged = true;
            Health -= damage;
            if (_hpBarCoroutine == null)
            {
                _hpBarCoroutine = StartCoroutine(ShowHpBar());
            }
            _lerpTimer = 0;
            yield return new WaitForSeconds(0.1f);
            IsDamaged = false;
        }
        
        public void ReceiveDamageActivate(float damage)
        {
            if (_hpBarCoroutine != null)
            {
                StopCoroutine(_hpBarCoroutine);
                _hpBarCoroutine = null;
            }
            StartCoroutine(ReceiveDamage(damage));
        }
        
        private IEnumerator ShowHpBar()
        {
            front.color = Color.red;
            back.color = Color.white;
            yield return new WaitForSeconds(3f);
            front.color = Color.clear;
            back.color = Color.clear;
        }
        
        private void UpdateHpBar()
        {
            float fillFrontBar = front.fillAmount;
            float fillBackBar = back.fillAmount;
            float hFraction = Health / MaxHealth;

            if (fillBackBar > hFraction)
            {
                front.fillAmount = hFraction;
                back.color = Color.white;
                _lerpTimer += Time.deltaTime;
                float percentComplete = _lerpTimer / 3;
                percentComplete *= percentComplete;
                back.fillAmount = Mathf.Lerp(fillBackBar, hFraction, percentComplete);
            }
            
            if (fillFrontBar < hFraction)
            {
                back.color = Color.green;
                back.fillAmount = hFraction;
                _lerpTimer += Time.deltaTime;
                float percentComplete = _lerpTimer / 3;
                percentComplete *= percentComplete;
                front.fillAmount = Mathf.Lerp(fillFrontBar, back.fillAmount, percentComplete);
            }
        }
        
        private void KillEnemy()
        {
            Destroy(gameObject);
        }
    }
}
