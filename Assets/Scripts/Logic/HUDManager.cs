using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("Здоровье (Сердца)")]
    [SerializeField] private Image[] heartImages;
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;

    [Header("Символы Божественности")]
    [SerializeField] private Image wingsIcon;   // Двойной прыжок
    [SerializeField] private Image eyeIcon;     // Заморозка времени
    [SerializeField] private Image scepterIcon; // Щит

    public enum SymbolType { Wings, Eye, Scepter }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // На старте игры делаем все способности полупрозрачными (заблокированными)
        LockAllSymbols();

            // 2. Проверяем нашу вечную память и включаем то, что уже было подобрано
        if (GameProgression.HasWings) UnlockSymbol(SymbolType.Wings);
        if (GameProgression.HasEye) UnlockSymbol(SymbolType.Eye);
        if (GameProgression.HasScepter) UnlockSymbol(SymbolType.Scepter);
    }

    // --- ЛОГИКА ЗДОРОВЬЯ ---
    public void UpdateHealthUI(int currentHealth)
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < currentHealth)
            {
                heartImages[i].sprite = fullHeartSprite;
                heartImages[i].enabled = true;
            }
            else
            {
                // Если у тебя есть спрайт пустого сердца — меняем на него. 
                // Если нет — просто скрываем иконку сердца (emptyHeartSprite == null)
                if (emptyHeartSprite != null)
                    heartImages[i].sprite = emptyHeartSprite;
                else
                    heartImages[i].enabled = false; 
            }
        }
    }

    // --- ЛОГИКА СИМВОЛОВ ---
    public void UnlockSymbol(SymbolType symbol)
    {
        switch (symbol)
        {
            case SymbolType.Wings:
                SetImageAlpha(wingsIcon, 1f); // Полная яркость
                break;
            case SymbolType.Eye:
                SetImageAlpha(eyeIcon, 1f);
                break;
            case SymbolType.Scepter:
                SetImageAlpha(scepterIcon, 1f);
                break;
        }
    }

    private void LockAllSymbols()
    {
        SetImageAlpha(wingsIcon, 0.2f);   // 20% прозрачности (заблокировано)
        SetImageAlpha(eyeIcon, 0.2f);
        SetImageAlpha(scepterIcon, 0.2f);
    }

    private void SetImageAlpha(Image image, float alpha)
    {
        if (image == null) return;
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}