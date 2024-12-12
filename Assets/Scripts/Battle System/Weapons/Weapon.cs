using Player;
using UnityEngine;

namespace Battle_System.Weapons
{
    public abstract class Weapon : MonoBehaviour
    {
        public PlayerConfig Player;
    
        public float Damage;

        protected float DamageBooster;

        public WeaponType Type;

        public void TakeWeapon(GameObject player)
        {
            Player = player.GetComponent<PlayerConfig>();
        }

        protected void UpdateDamage()
        {
            if (Player)
            {
                if (DamageBooster < Player.DamageBooster)
                {
                    DamageBooster = Player.DamageBooster;
                    float damage = Damage + DamageBooster;
                    Damage = damage;
                }
            }
        }
        
        public void DropWeapon()
        {
            Player = null;
        }
    }

    public enum WeaponType
    {
        Racket,
        Blaster,
        MagicOrb
    }
}