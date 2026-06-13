using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PoundStone : MonoBehaviour
{
    [Header("Настройки камня")]
    [SerializeField] private float initialUpwardForce = 8f;
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private float destroyAnimationDuration = 0.4f; // длина анимации разрушения

    [Header("Урон")]
    [SerializeField] private int damage = 2; // камень бьёт сильнее обычной атаки

    [SerializeField] private LayerMask enemyLayer;

    [Header("События для художника")]
    public UnityEvent onSpawn;
    public UnityEvent onLaunch;
    public UnityEvent onDestroyed; // триггер анимации разрушения

    private Rigidbody2D _rb;
    private BoxCollider2D _collider;
    private bool _isDestroying;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, initialUpwardForce);
        onSpawn?.Invoke();
        StartCoroutine(LifetimeRoutine());
    }

    public void Launch(Vector2 direction, float force)
    {
        if (_isDestroying) return;
        if (_rb != null)
        {
            _rb.linearVelocity = direction * force;
            onLaunch?.Invoke();
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (_isDestroying) return;

        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            other.gameObject.GetComponent<IDamageable>()?.TakeDamage(damage);
            StartCoroutine(DestroyRoutine());
        }
    }

    private IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(lifetime);
        StartCoroutine(DestroyRoutine());
    }

    public void DestroyStone()
    {
        if (_isDestroying) return;
        StartCoroutine(DestroyRoutine());
    }

    private IEnumerator DestroyRoutine()
    {
        if (_isDestroying) yield break;
        _isDestroying = true;

        // Отключаем коллайдер и физику
        _collider.enabled = false;
        _rb.simulated = false;

        // Запускаем анимацию разрушения
        onDestroyed?.Invoke(); // художник вешает Play("Break")

        yield return new WaitForSeconds(destroyAnimationDuration);

        Destroy(gameObject);
    }
}