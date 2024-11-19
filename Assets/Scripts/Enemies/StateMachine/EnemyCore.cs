using System.Collections;
using Enemies.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace Enemies.StateMachine
{
    public class EnemyCore : MonoBehaviour
    {
        public AnimationController AnimationControl;
        
        public GameObject EnemyObject;
        
        public bool NotRefreshing;
        
        public Coroutine HpBarCoroutine;
        
        public float LerpTimer;
        
        public Image Back;
        
        public Image Front;
        
        public bool IsInvulnerable;
        
        public float PushForce;
        
        public Rigidbody Rb;
        
        public float MaxHealth;
        
        public GameObject Target;
        
        public EnemyType TypeOfEnemy;
        
        public Animator AnimatorController;
        
        public float Speed;
        
        public float Damage;
        
        public float Health;
        
        public bool IsDamaged;
        
        public bool IsAttacking;

        public StateMachine Machine;
    
        public EnemyState State => Machine.State;
    
        protected void Set(EnemyState newState, bool forceReset = false)
        {
            Machine.Set(newState, forceReset);
        }

        protected void SetupInstances()
        {
            Machine = new StateMachine();

            EnemyState[] allchildStates = GetComponentsInChildren<EnemyState>();
            foreach (EnemyState state in allchildStates)
            {
                state.SetCore(this);
            }
        }
        
        private IEnumerator ReceiveDamage(float damage, Vector3 pushFrom)
        {
            IsDamaged = true;
            PushAway(pushFrom, PushForce);
            Health -= damage;
            if (HpBarCoroutine == null)
            {
                HpBarCoroutine = StartCoroutine(ShowHpBar());
            }
            LerpTimer = 0;
            yield return new WaitForSeconds(1f);
            IsDamaged = false;
        }
        
        public void ReceiveDamageActivate(float damage, Vector3 pushFrom)
        {
            if (IsInvulnerable)
            {
                return;
            }
            
            if (HpBarCoroutine != null)
            {
                StopCoroutine(HpBarCoroutine);
                HpBarCoroutine = null;
            }
            StartCoroutine(ReceiveDamage(damage, pushFrom));
        }
        
        private IEnumerator ShowHpBar()
        {
            Front.color = Color.red;
            Back.color = Color.white;
            yield return new WaitForSeconds(3f);
            Front.color = Color.clear;
            Back.color = Color.clear;
        }
        
        protected void UpdateHpBar()
        {
            float fillFrontBar = Front.fillAmount;
            float fillBackBar = Back.fillAmount;
            float hFraction = Health / MaxHealth;

            if (fillBackBar > hFraction)
            {
                Front.fillAmount = hFraction;
                Back.color = Color.white;
                LerpTimer += Time.deltaTime;
                float percentComplete = LerpTimer / 3;
                percentComplete *= percentComplete;
                Back.fillAmount = Mathf.Lerp(fillBackBar, hFraction, percentComplete);
            }
            
            if (fillFrontBar < hFraction)
            {
                Back.color = Color.green;
                Back.fillAmount = hFraction;
                LerpTimer += Time.deltaTime;
                float percentComplete = LerpTimer / 3;
                percentComplete *= percentComplete;
                Front.fillAmount = Mathf.Lerp(fillFrontBar, Back.fillAmount, percentComplete);
            }
        }
        
        private void PushAway(Vector3 pushFrom, float pushPower)
        {
            var pushDirection = -(pushFrom - transform.position);
            
            pushDirection.Normalize();
            
            Rb.AddForce(pushDirection * pushPower);
        }

        public IEnumerator Invulnerability()
        {
            IsInvulnerable = true;
            yield return new WaitForSeconds(2f);
            IsInvulnerable = false;
        }
        
        protected void KillEnemy()
        {
            Destroy(gameObject);
        }

        public IEnumerator RefreshAttack()
        {
            NotRefreshing = false;
            IsAttacking = false;
            yield return new WaitForSeconds(2f);
            if (IsAttacking)
            {
                NotRefreshing = true;
            }
        }
    }

    public enum EnemyType
    {
        Mosquito
    }
}
