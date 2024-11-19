using System.Collections;
using UnityEngine;

namespace Enemies.StateMachine
{
    public class AttackingState : EnemyState
    {
        private bool startAttack = false;

        private bool isAttackingState;

        private Vector3 plusPosition;

        private float timer;

        private float timeToAttack = 1f;
        
        public override void Enter()
        {
            startAttack = false;
            StartCoroutine(WaitForAttack());
            animator.Play("Bite Attack");
        }
    
        public override void Do()
        {
            if ((isDamaged || !Core.IsAttacking) && isAttackingState)
            {
                StopCoroutine(WaitForAttack());
                ExitAttack();
                return;
            }
            
            //animationControl.Attack();
            
            if (startAttack && timer <= timeToAttack)
            {
                Core.EnemyObject.transform.position =
                    Vector3.MoveTowards(Core.EnemyObject.transform.position, plusPosition, 3f * Time.deltaTime);

                timer += Time.deltaTime;
            }
            else if(timer >= timeToAttack)
            {
                timer = 0;
                ExitAttack();
            }
        }
    
        public override void Exit()
        {
            animator.Play("Blend tree");
        }

        private IEnumerator WaitForAttack()
        {
            isAttackingState = true;
            
            yield return new WaitForSeconds(1.5f);
            
            startAttack = true;
            
            plusPosition = -(Core.EnemyObject.transform.position - target.transform.position);
            
            plusPosition.Normalize();

            plusPosition *= 0.4f;

            plusPosition = target.transform.position + plusPosition;
        }

        private void ExitAttack()
        {
            isAttackingState = false;
            startAttack = false;
            StartCoroutine(Core.RefreshAttack());
            IsComplete = true;
        }
    }
}
