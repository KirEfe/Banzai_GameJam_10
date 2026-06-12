using UnityEngine;
using UnityEngine.Events;

public class PlayerAbilities : MonoBehaviour
{
    public bool HasWings { get; private set; }
    public bool HasEye { get; private set; }
    public bool HasScepter { get; private set; }

    [Header("События для художника")]
    public UnityEvent onWingsUnlocked;
    public UnityEvent onEyeUnlocked;
    public UnityEvent onScepterUnlocked;

    private void Start()
    {
        // Восстанавливаем способности из прогрессии при загрузке сцены
        if (GameProgression.HasWings) UnlockWings(silent: true);
        if (GameProgression.HasEye) UnlockEye(silent: true);
        if (GameProgression.HasScepter) UnlockScepter(silent: true);
    }

    public void UnlockWings(bool silent = false)
    {
        HasWings = true;
        GameProgression.HasWings = true;
        HUDManager.Instance?.UnlockSymbol(HUDManager.SymbolType.Wings);
        if (!silent) onWingsUnlocked?.Invoke();
    }

    public void UnlockEye(bool silent = false)
    {
        HasEye = true;
        GameProgression.HasEye = true;
        HUDManager.Instance?.UnlockSymbol(HUDManager.SymbolType.Eye);
        if (!silent) onEyeUnlocked?.Invoke();
    }

    public void UnlockScepter(bool silent = false)
    {
        HasScepter = true;
        GameProgression.HasScepter = true;
        HUDManager.Instance?.UnlockSymbol(HUDManager.SymbolType.Scepter);
        if (!silent) onScepterUnlocked?.Invoke();
    }
}