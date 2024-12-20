using System.Collections.Generic;
using Audio;
using GameProcess.Directors;
using Player;
using Saving;
using UI;
using UI.Inventory;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameProcess
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private PlayerConfig player;

        [SerializeField] private Inventory inventory;

        [SerializeField] private InputActionReference cheatInput;

        [SerializeField] private GameObject startingUI;
        
        [SerializeField] private GameObject loseUI;

        [SerializeField] private CombatDirector combatDirector;
        
        public static GameManager Instance;

        private int _minutesInGame;

        private readonly float _timeFactor = 0.2506f;

        public float GameDifficulty;

        public List<GameObject> Enemies;

        private float _gameTime;

        public int EnemyLevel;

        private float _timerUpgrade;

        public bool IsGameStarted = false;

        public bool IsGameOver;
        
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
        
        private void Start()
        {
            EventHandler.GameLost += OnGameLost;
            SaveSystem.Instance.Load();
            StartMenuShow();
            AudioManager.instance.PlayMusic("BG_Music");
        }

        public void StartMenuShow() => ShowUI.CreateUI(startingUI);

        private void Update()
        {
            if (!IsGameStarted)
            {
                return;
            }
            
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
                EventHandler.EnemyDied.Invoke(10, 10);
            }
        }

        public void StartGame()
        {
            inventory.InsertITems();
            player.HpBar.SetActive(true);
            combatDirector.enabled = true;
            IsGameStarted = true;
            IsGameOver = false;
            player.HealMax();
            AudioManager.instance.PlayMusic("BattleMusic");
        }

        private void OnGameLost()
        {
            player.HpBar.SetActive(false);
            SaveSystem.Instance.Credits += _minutesInGame * _minutesInGame * 8;
            IsGameOver = true;
            IsGameStarted = false;
            _gameTime = 0;
            _minutesInGame = 0;
            _timerUpgrade = 0;
            GameDifficulty = 0;
            EnemyLevel = 1;
            combatDirector.ResetGame();
            combatDirector.enabled = false;
            foreach (var enemy in Enemies)
            {
                Destroy(enemy);
            }
            Enemies.Clear();
            AudioManager.instance.PlayMusic("BG_Music");
            SaveSystem.Instance.Save();
            ShowUI.CreateUI(loseUI);
        }
    }
}
