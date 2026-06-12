using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnManager : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private PlayerHealth playerHealth;
    
    [Header("Тайминги Анимаций")]
    [SerializeField] private float deathAnimationDuration = 1.0f;   // Сколько секунд длится анимация смерти
    [SerializeField] private float respawnAnimationDuration = 0.8f; // Сколько секунд длится анимация появления

    private static Vector3? _savedSpawnPosition = null;

    private void Start()
    {
        if (playerHealth != null)
            playerHealth.onDeath.AddListener(OnPlayerDeath); // Слушаем смерть игрока

        // Если мы только что перезагрузили сцену после смерти:
        if (_savedSpawnPosition.HasValue)
        {
            playerHealth.transform.position = _savedSpawnPosition.Value; // Смещаем на точку респауна
            _savedSpawnPosition = null; 

            // Запускаем корутину анимации появления
            StartCoroutine(RespawnAnimationRoutine());
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.onDeath.RemoveListener(OnPlayerDeath);
        }
    }

    private void OnPlayerDeath()
    {
        StartCoroutine(DeathRoutine()); // Запускаем логику смерти
    }

    // --- ЛОГИКА СМЕРТИ ---
    private IEnumerator DeathRoutine()
    {
        // 1. Сразу же отключаем управление игроком
        var controller = playerHealth.GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        // 2. Включаем триггер анимации смерти
        var animator = playerHealth.GetComponent<Animator>();
        if (animator != null) animator.SetTrigger("Die"); // Имя триггера в Аниматоре

        // 3. Ждём, пока Смертный бог красиво падает на землю
        yield return new WaitForSeconds(deathAnimationDuration);

        // 4. Запоминаем точку респауна перед перезапуском
        if (respawnPoint != null)
        {
            _savedSpawnPosition = respawnPoint.position;
        }

        // 5. Перезапускаем сцену
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (currentSceneIndex == -1)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            SceneManager.LoadScene(currentSceneIndex);
        }
    }

    // --- ЛОГИКА ПОЯВЛЕНИЯ (РЕСПАУНА) ---
    private IEnumerator RespawnAnimationRoutine()
    {
        // 1. Сразу блокируем управление на новой сцене, чтобы игрок не бегал во время анимации появления
        var controller = playerHealth.GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        // 2. Включаем триггер анимации появления (например, восстает из праха или падает с неба)
        var animator = playerHealth.GetComponent<Animator>();
        if (animator != null) animator.SetTrigger("Respawn"); // Имя триггера в Аниматоре

        // 3. Ждём заданное время, пока анимация проигрывается
        yield return new WaitForSeconds(respawnAnimationDuration);

        // 4. Возвращаем игроку управление! Наш Animator сам перейдет в Idle, если так настроено транзишном
        if (controller != null) controller.enabled = true;
    }
}