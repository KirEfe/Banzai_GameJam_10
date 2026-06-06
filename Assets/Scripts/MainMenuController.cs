using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public void OnPlayButton()
    {
        SceneLoader.Instance.LoadGame();
    }

    public void OnExitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}