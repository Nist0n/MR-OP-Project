using System;
using UnityEngine;

namespace Player
{
    public class PlayerConfig : MonoBehaviour
    {
        private float _health;

        private float _maxHealth = 100;

        private void Start()
        {
            _health = _maxHealth;
        }

        private void Update()
        {
            _health = Mathf.Clamp(_health, 0, _maxHealth);

            if (_health <= 0)
            {
                Debug.Log("IsDead");
            }
        }

        public void ReceiveDamage(float damage)
        {
            _health -= damage;
        }
    }
}
