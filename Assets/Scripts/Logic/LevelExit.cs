using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // ОБЯЗАТЕЛЬНО для смены сцен
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class LevelExit : MonoBehaviour
{
    [Header("Настройки перехода")]
    [SerializeField] private float delayBeforeLoad = 0.5f;   // Небольшая задержка, чтобы анимация/звук успели отыграть
    [SerializeField] private string customNextSceneName = ""; // Опционально: если нужно прыгнуть на конкретную сцену в обход очереди

    [Header("События для художника")]
    public UnityEvent onExitTriggered; // Сюда вешаем музыку победы или анимацию фейда экрана

    private bool _isTransitioning = false;

    private void Start()
    {
        // Авто-настройка триггера, чтобы не забыть в редакторе
        var col = GetComponent<BoxCollider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Если в дверь/портал вошел игрок и переход еще не начался
        if (other.CompareTag("Player") && !_isTransitioning)
        {
            StartCoroutine(LoadNextLevelRoutine());
        }
    }

    private IEnumerator LoadNextLevelRoutine()
    {
        _isTransitioning = true;

        // Отключаем управление игроку, чтобы он не бегал во время перехода
        var controller = FindFirstObjectByType<PlayerController>();
        if (controller != null) controller.enabled = false;

        // Пингуем художника
        onExitTriggered?.Invoke();

        // Ждем пока доиграют эффекты
        yield return new WaitForSeconds(delayBeforeLoad);

        // 1. Проверяем, задано ли имя сцены вручную
        if (!string.IsNullOrEmpty(customNextSceneName))
        {
            SceneManager.LoadScene(customNextSceneName);
            yield break;
        }

        // 2. Иначе автоматически вычисляем индекс следующей сцены
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        // Если в списке сцен есть следующий уровень — загружаем его
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            // Страховка для джема: если это был финальный уровень, возвращаем в главное меню (индекс 0)
            Debug.LogWarning("Следующий уровень не найден в Build Settings! Возвращаюсь на экран 0.");
            SceneManager.LoadScene(0);
        }
    }
}