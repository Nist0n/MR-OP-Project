using System;
using UnityEngine;

namespace Player
{
    public class PlayerConfig : MonoBehaviour
    {
        private float _health;

        private float _maxHealth = 100;

        private int _level;

        private float _exp;

        private float _gold;

        private void Start()
        {
            _health = _maxHealth;
        }

        private void Update()
        {
            _health = Mathf.Clamp(_health, 0, _maxHealth);

            if (_health <= 0)
            {
            }
        }

        public void ReceiveDamage(float damage)
        {
            _health -= damage;
        }

        private void LevelUp()
        {
            
        }

        public void GetRewards(float gold, float exp)
        {
            _exp += exp;
            _gold += gold;
        }
    }
}
