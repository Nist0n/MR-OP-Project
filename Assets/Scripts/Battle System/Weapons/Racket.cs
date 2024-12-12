using System;
using Enemies;
using Enemies.StateMachine;
using UnityEngine;

namespace Battle_System.Weapons
{
    public class Racket : Weapon
    {
        private void Update()
        {
            UpdateDamage();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                if (gameObject.GetComponent<Rigidbody>().velocity.magnitude >= 20f)
                {
                    other.GetComponent<FirstEnemy>().ReceiveDamageActivate(Damage * 2, gameObject.transform.position);
                    return;
                }
                
                other.GetComponent<FirstEnemy>().ReceiveDamageActivate(Damage, gameObject.transform.position);
            }
        }
    }
}
