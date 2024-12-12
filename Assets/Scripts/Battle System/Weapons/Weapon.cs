using Player;
using UnityEngine;

namespace Battle_System.Weapons
{
    public abstract class Weapon : MonoBehaviour
    {
        public PlayerConfig Player;
    
        public float Damage;

        private float _damageBooster;

        public WeaponType Type;

        public void TakeWeapon(GameObject player)
        {
            Player = player.GetComponent<PlayerConfig>();
        }

        protected void UpdateDamage()
        {
            if (Player)
            {
                if (_damageBooster < Player.DamageBooster)
                {
                    _damageBooster = Player.DamageBooster;
                    float damage = Damage + _damageBooster;
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