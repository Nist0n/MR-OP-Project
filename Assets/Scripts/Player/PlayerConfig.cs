using System;
using UnityEngine;

namespace Player
{
    public class PlayerConfig : MonoBehaviour
    {
        private float _health;

        private float _maxHealth = 100;

        private float _level;

        private float _exp;

        public float Gold { get; private set; }

        public float DamageBooster { get; private set; }

        private float _expForLevelUp = 4f;

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
            
            CheckForLevelUp();
        }

        public void ReceiveDamage(float damage)
        {
            _health -= damage;
        }

        private void CheckForLevelUp()
        {
            if (_exp >= _expForLevelUp)
            {
                _level++;
                _exp -= _expForLevelUp;
                _expForLevelUp += 3f * _level * _level;
                LevelUp();
            }
        }

        private void LevelUp()
        {
            float hp = _level * _level / 5f + 2f * _level;
            float damage = _level * _level / 5f;
            _maxHealth += hp;
            _health += hp;
            DamageBooster += damage;
        }

        public void GetRewards(float gold, float exp)
        {
            _exp += exp;
            Gold += gold;
        }
    }
}
