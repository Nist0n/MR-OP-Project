using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameProcess
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private PlayerConfig player;

        [SerializeField] private InputActionReference cheatInput;
        
        
        public static GameManager Instance;

        private int _minutesInGame;

        private readonly float _timeFactor = 0.2506f;

        public float GameDifficulty;

        public List<GameObject> Enemies;

        private float _gameTime;

        public int EnemyLevel;

        private float _timerUpgrade;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            AudioManager.instance.PlayMusic("BG_Music");
        }

        private void Update()
        {
            Timer();
            
            UpdateTimeScale();
            
            IncreaseDifficulty();
            
            Cheats();
        }
        
        private void Timer()
        {
            _gameTime += Time.deltaTime;
            _minutesInGame = (int) Mathf.Floor(_gameTime / 60);
        }

        private void UpdateTimeScale()
        {
            GameDifficulty = (1 + _minutesInGame * _timeFactor) * 1.15f;
        }

        private void IncreaseDifficulty()
        {
            switch (_gameTime)
            {
                case > 120 and < 121:
                case > 240 and < 241:
                case > 320 and < 321:
                case > 400 and < 401:
                case > 440 and < 441:
                    EnemyLevel = Mathf.FloorToInt(1 + (GameDifficulty - 1) / 0.33f);
                    break;
                case > 441:
                    _timerUpgrade += Time.deltaTime;
                    if (_timerUpgrade > 20)
                    {
                        EnemyLevel = Mathf.FloorToInt(1 + (GameDifficulty - 1) / 0.33f);
                        _timerUpgrade -= 20;
                    }
                    break;
            }
        }

        private void Cheats()
        {
            if (cheatInput.action.triggered)
            {
                Debug.Log("HM");
                player.GetRewards(10, 10);
            }
        }
    }
}
