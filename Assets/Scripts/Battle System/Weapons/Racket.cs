using UnityEngine;

namespace Battle_System.Weapons
{
    public class Racket : Weapon
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                if (gameObject.GetComponent<Rigidbody>().velocity.magnitude >= 20f)
                {
                    //IncreaseDamage
                }
                
                //Damage
            }
        }
    }
}
