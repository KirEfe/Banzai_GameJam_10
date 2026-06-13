using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(BoxCollider2D))]
public class CinemachineBossCamera : MonoBehaviour
{
    [Header("Камера для этой зоны")]
    [SerializeField] private CinemachineCamera zoneCamera; 
    
    [Header("Настройки приоритета")]
    [SerializeField] private int activePriority = 20; // Когда мы внутри зоны (выше основной)
    [SerializeField] private int idlePriority = 0;     // Когда мы вышли / победили (ниже основной)

    [Header("Ссылка на босса")]
    [SerializeField] private ArcherBoss boss;

    private bool _battleEnded = false;

    private void Start()
    {
        // На старте игры принудительно сбрасываем камеру зоны в ноль
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
        // Включаем камеру только если босс еще жив
        if (!_battleEnded && other.CompareTag("Player") && zoneCamera != null)
        {
            zoneCamera.Priority = activePriority;
        }
    }

    private void Update()
    {     
        // Теперь проверяем через IsBossDead, к которому у нас есть полный доступ
        if (!_battleEnded && (boss == null || boss.IsBossDead))
        {
            _battleEnded = true; 
            
            if (zoneCamera != null)
            {
                zoneCamera.Priority = idlePriority; 
            }
            
            Debug.Log("Босс повержен! Камера Синемашины возвращена Смертному богу.");
        }
    }
}