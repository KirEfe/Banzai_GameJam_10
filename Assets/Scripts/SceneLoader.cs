using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadRoutine(sceneName));
    }

    public void LoadMainMenu() => LoadScene(GameScenes.MainMenu);
    public void LoadGame() => LoadScene(GameScenes.GameScene);
    public void ReloadCurrent() => LoadScene(SceneManager.GetActiveScene().name);

    private IEnumerator LoadRoutine(string sceneName)
    {
        // сюда потом вставишь показ LoadingScreen
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            // сюда передашь прогресс на LoadingScreen: operation.progress
            yield return null;
        }

        // сюда потом вставишь скрытие LoadingScreen
        operation.allowSceneActivation = true;
    }
}