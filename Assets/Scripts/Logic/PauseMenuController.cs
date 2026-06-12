using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;

    private InputSystem_Actions _input;
    private bool _isPaused = false;

    private void Awake()
    {
        _input = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _input.UI.Cancel.performed += OnPausePressed;
        _input.UI.Enable();
    }

    private void OnDisable()
    {
        _input.UI.Cancel.performed -= OnPausePressed;
        _input.UI.Disable();
    }

    private void OnPausePressed(InputAction.CallbackContext ctx)
    {
        if (_isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        _isPaused = true;
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        _isPaused = false;
    }

    public void OnResumeButton() => Resume();

    public void OnMainMenuButton()
{
    Time.timeScale = 1f; // сначала сбрасываем время
    _isPaused = false;
    pauseMenuUI.SetActive(false);
    SceneLoader.Instance.LoadMainMenu(); // потом переходим
}
}