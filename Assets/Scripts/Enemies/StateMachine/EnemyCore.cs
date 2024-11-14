using UnityEngine;

namespace Enemies.StateMachine
{
    public class EnemyCore : MonoBehaviour
    {
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

        public void SetupInstances()
        {
            Machine = new StateMachine();

            EnemyState[] allchildStates = GetComponentsInChildren<EnemyState>();
            foreach (EnemyState state in allchildStates)
            {
                state.SetCore(this);
            }
        }
    }

    public enum EnemyType
    {
        Mosquito
    }
}
