using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Sphinx : EnemyBase
{
    [Header("Снаряды")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float projectileSpeed = 5f;
    [SerializeField] private float projectileLifetime = 5f;

    [Header("Стрельба")]
    [SerializeField] private float shootInterval = 2f;
    [SerializeField] private float shootDelay = 0.3f; // задержка перед спавном

    [Header("Обнаружение игрока")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private bool shootLeft = true;

    [Header("События для художника")]
    public UnityEvent onShoot;

    private bool _playerInRange;
    private Coroutine _shootCoroutine;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        base.Update();
        if (IsDead) return;

        bool wasInRange = _playerInRange;
        _playerInRange = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer) != null;

        // Игрок вошёл в зону — начинаем стрелять
        if (_playerInRange && !wasInRange)
        {
            if (_shootCoroutine != null) StopCoroutine(_shootCoroutine);
            _shootCoroutine = StartCoroutine(ShootRoutine());
        }

        // Игрок вышел из зоны — прекращаем
        if (!_playerInRange && wasInRange)
        {
            if (_shootCoroutine != null)
            {
                StopCoroutine(_shootCoroutine);
                _shootCoroutine = null;
            }
        }
    }

    private IEnumerator ShootRoutine()
    {
        while (_playerInRange && !IsDead)
        {
            yield return new WaitForSeconds(shootInterval);

            if (!_playerInRange || IsDead) yield break;

            onShoot?.Invoke(); // анимация плевка

            yield return new WaitForSeconds(shootDelay); // задержка перед спавном

            if (!_playerInRange || IsDead) yield break;

            SpawnProjectile();
        }
    }

    private void SpawnProjectile()
    {
        if (projectilePrefab == null || shootPoint == null) return;

        GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            Vector2 direction = shootLeft ? Vector2.left : Vector2.right;
            rb.linearVelocity = direction * projectileSpeed;
        }

        Destroy(projectile, projectileLifetime);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
            other.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(damageToPlayer);
    }

    private void OnDrawGizmosSelected()
    {
        // Зона обнаружения
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (shootPoint == null) return;

        // Направление стрельбы
        Gizmos.color = Color.red;
        Vector3 direction = shootLeft ? Vector3.left : Vector3.right;
        Gizmos.DrawLine(shootPoint.position, shootPoint.position + direction * 3f);
        Gizmos.DrawSphere(shootPoint.position, 0.1f);
    }
}