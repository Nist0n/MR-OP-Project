using Player;
using UnityEngine;

namespace Battle_System.Weapons
{
    public abstract class Weapon : MonoBehaviour
    {
        public PlayerConfig Player;
    
        public float Damage;

        public WeaponType Type;

        public void TakeWeapon(GameObject player)
        {
            Player = player.GetComponent<PlayerConfig>();
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