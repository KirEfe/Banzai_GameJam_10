using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // Добавляем для работы с перезапуском сцен

public class RespawnManager : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private float respawnDelay = 0.5f;

    // Внутренняя память, которая не сотрётся при перезапуске сцены.
    // Нужна только для того, чтобы передать координаты в следующую секунду.
    private static Vector3? _savedSpawnPosition = null;

    private void Start()
    {
        playerHealth.onDeath.AddListener(OnPlayerDeath); // Подписываемся на здоровье как раньше

        // ЕСЛИ мы только что перезапустили сцену и у нас есть сохранённая точка:
        if (_savedSpawnPosition.HasValue)
        {
            playerHealth.transform.position = _savedSpawnPosition.Value; // Перемещаем на RespawnPoint
            _savedSpawnPosition = null; // Очищаем память до следующей смерти
        }
    }

    private void OnDestroy()
    {
        // Небольшая страховка: проверяем на null, так как при перезагрузке сцены 
        // игрок может уничтожиться чуть раньше менеджера
        if (playerHealth != null)
        {
            playerHealth.onDeath.RemoveListener(OnPlayerDeath); // Отписываемся
        }
    }

    private void OnPlayerDeath()
    {
        StartCoroutine(RespawnRoutine()); // Запускаем корутину смерти
    }

    private IEnumerator RespawnRoutine()
    {
        // Отключаем управление пока игрок мёртв[cite: 7]
        var controller = playerHealth.GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        yield return new WaitForSeconds(respawnDelay); // Ждём задержку[cite: 7]

        // Запоминаем позицию нашего RespawnPoint перед тем, как перезагрузить мир[cite: 7]
        if (respawnPoint != null)
        {
            _savedSpawnPosition = respawnPoint.position;
        }

        // ПЕРЕЗАПУСК СЦЕНЫ
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (currentSceneIndex == -1)
        {
            // Если запускаешь из папки, а не из билда
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            SceneManager.LoadScene(currentSceneIndex);
        }
    }
}