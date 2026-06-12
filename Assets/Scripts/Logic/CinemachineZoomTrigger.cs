using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(BoxCollider2D))]
public class CinemachineZoomTrigger : MonoBehaviour
{
    [Header("Камера для этой зоны")]
    [SerializeField] private CinemachineCamera zoneCamera; 
    
    [Header("Настройки приоритета")]
    [SerializeField] private int activePriority = 20; // Когда мы внутри зоны (выше основной)
    [SerializeField] private int idlePriority = 0;     // Когда мы вышли (ниже основной)

    private void Start()
    {
        // На старте игры принудительно сбрасываем камеру зоны в ноль, 
        // чтобы в начале уровня работала только основная камера
        if (zoneCamera != null)
        {
            zoneCamera.Priority = idlePriority;
        }

        // Авто-настройка триггера
        var col = GetComponent<BoxCollider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Игрок вошел -> ставим 20 (Зона побеждает основную камеру)
        if (other.CompareTag("Player") && zoneCamera != null)
        {
            zoneCamera.Priority = activePriority;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Игрок вышел -> роняем приоритет в 0 (Основная камера с приоритетом 10 побеждает)
        if (other.CompareTag("Player") && zoneCamera != null)
        {
            zoneCamera.Priority = idlePriority;
        }
    }
}