using System;
using GameProcess;
using UnityEngine;
using UnityEngine.UI;
using EventHandler = GameProcess.EventHandler;

namespace Player
{
    public class PlayerConfig : MonoBehaviour
    {
        [SerializeField] private Image back;

        [SerializeField] private Image front;

        public GameObject HpBar;

        public GameObject UIPos;
        
        private float _health;

        private float _maxHealth = 1000;

        private float _level;

        private float _exp;

        private float _lerpTimer;

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

            UpdateHpBar();
        }

        private void OnEnemyDied(float gold, float exp)
        {
            GetRewards(gold, exp);
        }

        public void ReceiveDamage(float damage)
        {
            _health -= damage;
            _lerpTimer = 0;
        }

        public void HealMax()
        {
            _health = _maxHealth;
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

        private void UpdateHpBar()
        {
            float fillFrontBar = front.fillAmount;
            float fillBackBar = back.fillAmount;
            float hFraction = _health / _maxHealth;

            if (fillBackBar > hFraction)
            {
                front.fillAmount = hFraction;
                back.color = Color.red;
                _lerpTimer += Time.deltaTime;
                float percentComplete = _lerpTimer / 3;
                percentComplete *= percentComplete;
                back.fillAmount = Mathf.Lerp(fillBackBar, hFraction, percentComplete);
            }

            if (fillFrontBar < hFraction)
            {
                back.color = Color.green;
                back.fillAmount = hFraction;
                _lerpTimer += Time.deltaTime;
                float percentComplete = _lerpTimer / 3;
                percentComplete *= percentComplete;
                front.fillAmount = Mathf.Lerp(fillFrontBar, back.fillAmount, percentComplete);
            }
        }
    }
}
