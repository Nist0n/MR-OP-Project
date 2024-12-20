using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARPlaneManager))]
public class DebugModeMR : MonoBehaviour
{
    [SerializeField] private Canvas debugLog;
    
    [SerializeField]
    private InputActionReference toggleSurfaceRenderingAction; // Действие ввода для переключения визуализации

    [SerializeField] private bool isVisualiseOnStart; // Начальное состояние визуализации

    private bool _isVisualise; // Текущее состояние визуализации
    private ARPlaneManager _planeManager; // Компонент для управления AR-плоскостями

    private void Awake()
    {
        // Инициализация компонентов
        _planeManager = GetComponent<ARPlaneManager>();
        PlaneUpdateVisualisation();
    }

    public void OnEnable()
    {
        
    }

    public void OnDisable()
    {

    }

    // Метод-обработчик для изменения состояния AR-плоскостей
    private void OnPlanesChanged(ARPlanesChangedEventArgs arPlanesChangedEventArgs) => PlaneUpdateVisualisation();

    // Метод-обработчик для переключения визуализации при срабатывании действия ввода
    private void OnToggleSurfaceRendering(InputAction.CallbackContext obj)
    {
        _isVisualise = !_isVisualise; // Переключение состояния визуализации

        debugLog.enabled = _isVisualise;
        
        PlaneUpdateVisualisation();
    }

    // Обновление визуализации AR-плоскостей
    private void PlaneUpdateVisualisation()
    {
        foreach (var arPlane in _planeManager.trackables)
        {
            if (arPlane.TryGetComponent(out ARPlaneColorizer arPlaneColorizer))
            {
                arPlaneColorizer.isVisualise = _isVisualise; // Устанавливаем состояние визуализации
            }
        }
    }
}
