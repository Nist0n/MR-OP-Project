using System;
using GameProcess;
using UnityEngine;
using EventHandler = GameProcess.EventHandler;

namespace Player
{
    public class PlayerConfig : MonoBehaviour
    {
        public GameObject UIPos;
        
        private float _health;

        private float _maxHealth = 1000;

        private float _level;

        private float _exp;

        public float Gold { get; private set; }

        public float DamageBooster { get; private set; }

        private float _expForLevelUp = 4f;

        private void Start()
        {
            EventHandler.EnemyDied += OnEnemyDied;
            _health = _maxHealth;
        }

        private void Update()
        {
            if (GameManager.Instance.IsGameOver)
            {
                return;
            }
            
            _health = Mathf.Clamp(_health, 0, _maxHealth);

            if (_health <= 0)
            {
                EventHandler.OnGameLost();
            }
            
            CheckForLevelUp();
        }

        private void OnEnemyDied(float gold, float exp)
        {
            GetRewards(gold, exp);
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
            EventHandler.OnPlayerLevelUp(Mathf.FloorToInt(_level));
        }

        private void GetRewards(float gold, float exp)
        {
            _exp += exp;
            Gold += gold;
        }
    }
}
