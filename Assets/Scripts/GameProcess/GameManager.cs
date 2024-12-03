using UnityEngine;

namespace GameProcess
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;

        private int _minutesInGame;

        private readonly float _timeFactor = 0.0506f;

        public float GameDifficulty;

        private float _gameTime;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
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
    }
}
