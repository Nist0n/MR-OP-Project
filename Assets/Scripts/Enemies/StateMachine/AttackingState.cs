using Audio;
using System.Collections;
using UnityEngine;

namespace Enemies.StateMachine
{
    public class AttackingState : EnemyState
    {
        private bool _startAttack = false;
        
        private bool _isAttackingState; // _isAttackingState needed to avoid repeatedly calling the ExitAttack()

        private Vector3 _plusPosition;

        private float _timer;

        private float _timeToAttack = 1f; // Time after witch enemy stops attacking 
        
        public override void Enter()
        {
            _startAttack = false;
            
            StartCoroutine(WaitForAttack()); // Trigger Attack
            
            animator.Play("Bite Attack");

            AudioManager.instance.PlaySFX("Attacking");
        }
    
        public override void Do()
        {
            if ((isDamaged || !Core.IsAttacking) && _isAttackingState) // Switch state condition
            {
                StopCoroutine(WaitForAttack());
                ExitAttack();
                return;
            }
            
            if (_startAttack && _timer <= _timeToAttack)
            {
                Core.EnemyObject.transform.position =
                    Vector3.MoveTowards(Core.EnemyObject.transform.position, _plusPosition, 3f * Time.deltaTime);

                _timer += Time.deltaTime;
            }
            else if(_timer >= _timeToAttack) // End attack
            {
                _timer = 0;
                ExitAttack();
            }
        }
    
        public override void Exit()
        {
            animator.Play("Blend tree");
        }

        private IEnumerator WaitForAttack()
        {
            _isAttackingState = true;
            
            yield return new WaitForSeconds(1.5f); // Preparing for attack
            
            _startAttack = true;
            
            _plusPosition = -(Core.EnemyObject.transform.position - target.transform.position); // Find direction to Player
            
            _plusPosition.Normalize();

            _plusPosition *= 0.4f; // Increase attack path

            _plusPosition += target.transform.position;
        }

        private void ExitAttack()
        {
            _isAttackingState = false;
            _startAttack = false;
            StartCoroutine(Core.RefreshAttack());
            IsComplete = true;
        }
    }
}
