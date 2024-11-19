using Enemies.Animations;
using UnityEngine;

namespace Enemies.StateMachine
{
    public class EnemyState : MonoBehaviour
    {
        protected AnimationController animationControl => Core.AnimationControl;
        
        protected Animator animator => Core.AnimatorController;
        
        protected float pushForce => Core.PushForce;
        
        protected bool isDamaged => Core.IsDamaged;
        protected bool isAttacking => Core.IsAttacking;

        protected GameObject target => Core.Target;
        
        protected GameObject Player;
        
        protected float health => Core.Health;
        
        protected float damage => Core.Damage;
        
        protected float speed => Core.Speed;
        
        public bool IsComplete { get; protected set; }
    
        protected EnemyCore Core;
    
        protected float startTime;
    
        public EnemyState State => Machine.State;
    
        public StateMachine Machine;
    
        public StateMachine Parent;
    
        protected void Set(EnemyState newState, bool forceReset = false)
        {
            Machine.Set(newState, forceReset);
        }

        public void SetCore(EnemyCore _core)
        {
            Machine = new StateMachine();
            Core = _core;
        }
    
        public virtual void Enter() {}
    
        public virtual void Do() {}
    
        public virtual void FixedDo() {}
    
        public virtual void Exit() {}
    
        public void DoBranch()
        {
            Do();
            State?.DoBranch();
        }
    
        public void FixedDoBranch()
        {
            FixedDo();
            State?.FixedDoBranch();
        }
    
        public void Initialise(StateMachine parent)
        {
            Parent = parent;
            IsComplete = false;
            startTime = Time.time;
        }
    }
}
