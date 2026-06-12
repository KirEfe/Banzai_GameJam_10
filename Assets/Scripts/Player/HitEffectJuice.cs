using System.Collections;
using UnityEngine;

public class HitEffectJuice : MonoBehaviour
{
    [Header("Настройки вспышки")]
    [SerializeField] private SpriteRenderer enemySprite;
    [SerializeField] private Color flashColor = new Color(1f, 0.3f, 0.3f, 1f); // Ярко-красный хит
    [SerializeField] private float flashDuration = 0.1f;

    [Header("Эффект тяжести (Hit Stop)")]
    [SerializeField] private float hitStopDuration = 0.05f; // На сколько замирает мир при ударе

    private Color _originalColor;
    private Coroutine _flashCoroutine;
    private static bool _isHitStopping = false; // Общая переменная, чтобы стоп-кадры не наслаивались

    private void Start()
    {
        if (enemySprite == null) 
            enemySprite = GetComponentInChildren<SpriteRenderer>();
            
        if (enemySprite != null) 
            _originalColor = enemySprite.color;
    }

    /// <summary>
    /// Главный метод импакта. Вешаем его на событие OnDamaged у противников
    /// </summary>
    public void PlayHitJuice()
    {
        // 1. Запускаем вспышку цвета
        if (enemySprite != null)
        {
            if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
            _flashCoroutine = StartCoroutine(FlashRoutine());
        }

        // 2. Запускаем стоп-кадр мира (если он уже не идет от другого удара)
        if (!_isHitStopping && hitStopDuration > 0f)
        {
            StartCoroutine(HitStopRoutine());
        }
    }

    private IEnumerator FlashRoutine()
    {
        enemySprite.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        enemySprite.color = _originalColor;
    }

    private IEnumerator HitStopRoutine()
    {
        _isHitStopping = true;
        
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f; // Полностью замораживаем физику и анимации в игре

        // КРИТИЧНО: Используем Realtime, так как обычное время стоит на месте!
        yield return new WaitForSecondsRealtime(hitStopDuration); 

        Time.timeScale = originalTimeScale;
        _isHitStopping = false;
    }
}