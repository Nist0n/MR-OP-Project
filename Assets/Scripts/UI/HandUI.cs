using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandUI : MonoBehaviour
{
    [SerializeField] private Button resetRoomButton;
    [SerializeField] private Button spawnSphereButton;
    [SerializeField] private Button exitButton;
    
    private void Awake()
    {
        resetRoomButton.onClick.AddListener(ResetRoom);
        spawnSphereButton.onClick.AddListener(SpawnSphere);
        exitButton.onClick.AddListener(Exit);
    }
    
    private void Exit()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
            UnityEngine.Application.Quit();
        #endif
    }
    
    private void SpawnSphere()
    {
        var sphere = Resources.Load<GameObject>("Prefabs/Sphere");    
        Instantiate(sphere, transform.position, Quaternion.identity);
    }
    
    private void ResetRoom()
    {
        var arSession = FindAnyObjectByType<UnityEngine.XR.ARFoundation.ARSession>();
        var success = (arSession.subsystem as UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRSessionSubsystem)?.TryRequestSceneCapture() ?? false;
        Debug.Log($"Запрос на захват сцены Meta OpenXR завершен с результатом: {success}");
    }
}
