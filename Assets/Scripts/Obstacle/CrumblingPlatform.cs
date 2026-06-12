using System.Collections;
using UnityEngine;
using UnityEngine.Events; // ОБЯЗАТЕЛЬНО: добавляем для работы с ивентами

public class CrumblingPlatform : MonoBehaviour
{
    [SerializeField] private float shakeDelay = 0.5f;    // время до начала тряски
    [SerializeField] private float crumbleDelay = 0.8f;  // время до разрушения
    [SerializeField] private float respawnDelay = 3f;    // время до появления обратно

    [Header("События для художника")]
    public UnityEvent onCrumbleStart; // Срабатывает в момент наступления на платформу
    public UnityEvent onRespawn;      // Срабатывает, когда платформа восстанавливается

    private Vector3 _startPosition;
    private Collider2D _collider;
    private SpriteRenderer _renderer;
    private Animator _animator;       // Ссылка на компонент аниматора
    private bool _isCrumbling;

    private void Awake()
    {
        _startPosition = transform.position;
        _collider = GetComponent<Collider2D>();
        _renderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>(); // Кешируем аниматор
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player") && !_isCrumbling)
            StartCoroutine(CrumbleRoutine());
    }

    private IEnumerator CrumbleRoutine()
    {
        _isCrumbling = true;

        // Включаем анимацию разрушения и дёргаем ивент (для звуков/эффектов)
        if (_animator != null)
        {
            _animator.SetTrigger("Crumble"); // Запускаем триггер разрушения
        }
        onCrumbleStart?.Invoke();

        // Тряска
        yield return StartCoroutine(ShakeRoutine());

        // Разрушение — отключаем коллайдер и спрайт
        _collider.enabled = false;
        _renderer.enabled = false;

        // Ждём и восстанавливаем
        yield return new WaitForSeconds(respawnDelay);

        transform.position = _startPosition;
        _collider.enabled = true;
        _renderer.enabled = true;

        // Возвращаем аниматор в дефолтное состояние (Idle)
        if (_animator != null)
        {
            _animator.SetTrigger("Reset"); // Возвращаем платформу в целое состояние
        }
        onRespawn?.Invoke();

        _isCrumbling = false;
    }

    private IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;
        float shakeMagnitude = 0.12f;

        while (elapsed < shakeDelay)
        {
            float offsetY = Random.Range(-shakeMagnitude, shakeMagnitude);
            transform.position = _startPosition + new Vector3(0f, offsetY, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = _startPosition;
        yield return new WaitForSeconds(crumbleDelay - shakeDelay);
    }
}