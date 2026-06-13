using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ArcherBoss : EnemyBase
{
    [Header("Платформы")]
    [SerializeField] private Transform[] platforms;

    [Header("Стрельба")]
    [SerializeField] private GameObject[] projectilePrefabs; // список снарядов по порядку
    [SerializeField] private Transform firePoint;
    [SerializeField] private int shotsPerBurst = 3;
    [SerializeField] private float timeBetweenShots = 0.3f;
    [SerializeField] private float timeBetweenBursts = 2f;
    [SerializeField] private float phase2BurstDelay = 1.5f;
    [SerializeField] private float phase3BurstDelay = 1f;
    [SerializeField] private float projectileSpeed = 8f;

    [Header("Прыжок")]
    [SerializeField] private int hitsBeforeJump = 3;
    [SerializeField] private float jumpPeakHeight = 3f;
    [SerializeField] private float jumpDuration = 0.6f;

    [Header("Патруль на платформе")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float patrolRange = 2f; // насколько далеко от центра платформы

    [Header("Награда")]
    [SerializeField] private GameObject rewardPrefab;

    [Header("События для художника")]
    public UnityEvent onBattleStart;
    public UnityEvent onShoot;
    public UnityEvent onJump;
    public UnityEvent onLand;
    public UnityEvent onPhaseChange;

    public bool IsBossDead => IsDead;

    private int _currentPlatformIndex;
    private int _hitsReceivedOnPlatform;
    private int _projectileIndex; // текущий индекс в списке снарядов
    private bool _isJumping;
    private bool _battleStarted;
    private Transform _player;
    private Coroutine _shootingCoroutine;
    private Vector3 _currentPlatformCenter;
    private bool _patrollingRight = true;


    protected override void Awake()
    {
        base.Awake();
    }

    public void StartBattle()
    {
        if (_battleStarted) return;
        _battleStarted = true;

        _player = GameObject.FindWithTag("Player")?.transform;

        if (platforms.Length > 0)
        {
            transform.position = platforms[0].position;
            _currentPlatformCenter = platforms[0].position; // запоминаем центр
        }

        onBattleStart?.Invoke();
        SetMoving(true);
        _shootingCoroutine = StartCoroutine(ShootingLoop());
    }

    public override void TakeDamage(int damage)
    {
        if (!_battleStarted || _isJumping) return;

        base.TakeDamage(damage);
        if (IsDead) return;

        _hitsReceivedOnPlatform++;
        if (_hitsReceivedOnPlatform >= hitsBeforeJump)
        {
            _hitsReceivedOnPlatform = 0;
            TryJumpToNextPlatform();
        }
    }

    private void TryJumpToNextPlatform()
    {
        int nextIndex = (_currentPlatformIndex + 1) % platforms.Length;
        StartCoroutine(JumpToPlatform(nextIndex));
    }

    private IEnumerator JumpToPlatform(int targetIndex)
    {
        _isJumping = true;

        if (_shootingCoroutine != null)
        {
            StopCoroutine(_shootingCoroutine);
            _shootingCoroutine = null;
        }

        if (Animator != null)
            Animator.SetTrigger("Jump");

        onJump?.Invoke();

        Transform target = platforms[targetIndex];
        Vector2 startPos = transform.position;
        Vector2 endPos = target.position;
        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / jumpDuration;

            float x = Mathf.Lerp(startPos.x, endPos.x, t);
            float y = Mathf.Lerp(startPos.y, endPos.y, t) + jumpPeakHeight * Mathf.Sin(t * Mathf.PI);
            transform.position = new Vector2(x, y);

            bool movingRight = endPos.x > startPos.x;
            transform.localScale = new Vector3(
                movingRight ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );

            yield return null;
        }

        transform.position = endPos;
        _currentPlatformCenter = endPos; // новый центр патруля
        _currentPlatformIndex = targetIndex;

        if (Animator != null)
            Animator.SetTrigger("Land");

        onLand?.Invoke();

        if (_currentPlatformIndex > 0)
            onPhaseChange?.Invoke();

        _isJumping = false;
        SetMoving(true);

        _shootingCoroutine = StartCoroutine(ShootingLoop());
    }

    private IEnumerator ShootingLoop()
    {
        while (!IsDead)
        {
            // Патрулируем пока ждём следующей очереди
            float delay = _currentPlatformIndex == 0 ? timeBetweenBursts :
                        _currentPlatformIndex == 1 ? phase2BurstDelay :
                        phase3BurstDelay;

            float patrolTimer = 0f;
            SetMoving(true);

            while (patrolTimer < delay)
            {
                Patrol();
                patrolTimer += Time.deltaTime;
                yield return null;
            }

            // Останавливаемся для прицеливания
            SetMoving(false);
            FacePlayer();

            if (Animator != null)
                Animator.SetTrigger("Aim");

            yield return new WaitForSeconds(0.3f);

            yield return StartCoroutine(ShootBurst());
        }
    }

// Новый метод патруля:
    private void Patrol()
    {
        float leftBound = _currentPlatformCenter.x - patrolRange;
        float rightBound = _currentPlatformCenter.x + patrolRange;

        float direction = _patrollingRight ? 1f : -1f;
        transform.position += Vector3.right * direction * patrolSpeed * Time.deltaTime;

        // Разворот у границ
        if (transform.position.x >= rightBound) _patrollingRight = false;
        if (transform.position.x <= leftBound) _patrollingRight = true;

        // Разворот спрайта
        transform.localScale = new Vector3(
            _patrollingRight ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z
        );
    }

    private IEnumerator ShootBurst()
    {
        for (int i = 0; i < shotsPerBurst; i++)
        {
            if (IsDead) yield break;

            Shoot();

            if (Animator != null)
                Animator.SetTrigger("Shoot");

            onShoot?.Invoke();
            yield return new WaitForSeconds(timeBetweenShots);
        }
    }

    private void Shoot()
{
    if (firePoint == null || _player == null) return;
    if (projectilePrefabs == null || projectilePrefabs.Length == 0) return;

    GameObject prefab = projectilePrefabs[_projectileIndex % projectilePrefabs.Length];
    _projectileIndex++;

    if (prefab == null) return;

    GameObject proj = Instantiate(prefab, firePoint.position, Quaternion.identity);

    // Используем Setup вместо прямого задания скорости
    EnemyProjectile projectile = proj.GetComponent<EnemyProjectile>();
    if (projectile != null)
    {
        float direction = _player.position.x > firePoint.position.x ? 1f : -1f;
        projectile.Setup(direction, damageToPlayer);
    }
}

    private void FacePlayer()
    {
        if (_player == null) return;
        bool playerRight = _player.position.x > transform.position.x;
        transform.localScale = new Vector3(
            playerRight ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z
        );
    }

    protected override void Die()
    {
        if (rewardPrefab != null)
            Instantiate(rewardPrefab, transform.position, Quaternion.identity);

        base.Die(); // вызываем смерть из EnemyBase
    }

    private void OnDrawGizmosSelected()
    {
        if (platforms == null) return;
        Gizmos.color = Color.magenta;
        for (int i = 0; i < platforms.Length; i++)
        {
            if (platforms[i] == null) continue;
            Gizmos.DrawWireSphere(platforms[i].position, 0.4f);
            if (i < platforms.Length - 1 && platforms[i + 1] != null)
                Gizmos.DrawLine(platforms[i].position, platforms[i + 1].position);
        }
    }
}