using GameProcess;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GameUI : MonoBehaviour
    {
        [SerializeField] private Button resetRoomButton;
        [SerializeField] private Button startButton;
        [SerializeField] private Button exitButton;
    
        private void Awake()
        {
            resetRoomButton.onClick.AddListener(ResetRoom);
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
    
        private void ResetRoom()
        {
            var arSession = FindAnyObjectByType<UnityEngine.XR.ARFoundation.ARSession>();
            var success = (arSession.subsystem as UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRSessionSubsystem)?.TryRequestSceneCapture() ?? false;
            Debug.Log($"Запрос на захват сцены Meta OpenXR завершен с результатом: {success}");
        }

        private void StartGame()
        {
            GameManager.Instance.StartGame();
        }
    }
}
