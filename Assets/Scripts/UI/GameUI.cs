using GameProcess;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GameUI : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button exitButton;
    
        private void Awake()
        {
            exitButton.onClick.AddListener(Exit);
            startButton.onClick.AddListener(StartGame);
        }
    
        private void Exit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }
        
        private void StartGame()
        {
            GameManager.Instance.StartGame();
        }
    }
}
