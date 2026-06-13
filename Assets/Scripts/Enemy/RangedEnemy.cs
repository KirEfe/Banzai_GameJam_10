using UnityEngine;
using UnityEngine.Events;

public class RangedEnemy : EnemyBase
{
    [Header("Патруль")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float patrolDistance = 4f;
    [SerializeField] private bool canTurn = true; // сними галочку для Сфинкса

    [Header("Дистанционная Атака")]
    [SerializeField] private float attackRange = 5f; // Большая дистанция атаки
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float shootDelay = 0.3f;
    [SerializeField] private GameObject projectilePrefab; // Префаб стрелы/копья
    [SerializeField] private Transform firePoint;         // Точка, откуда вылетает стрела

    [SerializeField] private LayerMask playerLayer;

    [Header("Проверка земли и стен")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float checkRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("События для художника")]
    public UnityEvent onAttack;

    private Vector3 _startPosition;
    private bool _movingRight = true;
    private float _attackCooldownCounter;

    private enum State { Patrolling, Attacking }
    private State _currentState;

    protected override void Awake()
    {
        base.Awake();
        _startPosition = transform.position;
        _currentState = State.Patrolling;
        SetMoving(true);
    }

    protected override void Update()
    {
        base.Update(); // Запускаем таймер неуязвимости из базы

        if (IsDead) return;

        // Если в стане — стоим
        if (IsStunned)
        {
            SetMoving(false);
            return;
        }

        if (_attackCooldownCounter > 0f)
            _attackCooldownCounter -= Time.deltaTime;

        bool playerInRange = CheckPlayerInRange();

        if (playerInRange)
        {
            if (_currentState != State.Attacking)
            {
                _currentState = State.Attacking;
                SetMoving(false); // Останавливаемся для стрельбы
            }
            
            // Разворачиваем лучника лицом к игроку перед выстрелом
            AimAtPlayer();
            TryAttack();
        }
        else
        {
            if (_currentState != State.Patrolling)
            {
                _currentState = State.Patrolling;
            }
            
            SetMoving(true);
            Patrol();
        }
    }

    private void Patrol()
    {
        float distanceFromStart = transform.position.x - _startPosition.x;
        if (distanceFromStart >= patrolDistance) _movingRight = false;
        if (distanceFromStart <= -patrolDistance) _movingRight = true;

        bool groundAhead = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        bool wallAhead = Physics2D.OverlapCircle(wallCheck.position, checkRadius, groundLayer);

        if (!groundAhead || wallAhead)
            _movingRight = !_movingRight;

        float direction = _movingRight ? 1f : -1f;
        transform.position += Vector3.right * direction * moveSpeed * Time.deltaTime;
        
        UpdateScale();
    }

    private bool CheckPlayerInRange()
    {
        // Ищем игрока в радиусе атаки вокруг лучника
        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackRange, playerLayer);
        return hit != null;
    }

    private void AimAtPlayer()
    {
        if (!canTurn) return; // Сфинкс не разворачивается
        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackRange, playerLayer);
        if (hit == null) return;

        // Определяем, с какой стороны игрок, и поворачиваемся к нему
        float directionToPlayer = hit.transform.position.x - transform.position.x;
        _movingRight = directionToPlayer > 0;
        UpdateScale();
    }

    private void UpdateScale()
    {
        transform.localScale = new Vector3(
            _movingRight ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z
        );
    }

    private void TryAttack()
    {
        if (_attackCooldownCounter > 0f) return;

        _attackCooldownCounter = attackCooldown;
        
        // 1. Запускаем анимацию атаки в Аниматоре
        if (Animator != null)
        {
            Animator.SetTrigger("Attack"); // Убедись, что в Animator создан параметр-Trigger с именем Attack
        }

        onAttack?.Invoke();

        // 2. Вместо мгновенного спавна запускаем задержку
        StartCoroutine(SpawnProjectileWithDelay());
    }

    private System.Collections.IEnumerator SpawnProjectileWithDelay()
    {
        // Ждем, пока проиграется фаза замаха/натяжения лука
        yield return new WaitForSeconds(shootDelay);

        // Критически важно для джема: проверяем, не убил ли игрок лучника, пока тот замахивался
        if (IsDead || IsStunned) yield break; // Если мертв или в стане — стрелу не спавним 

        // Спавним стрелу
        if (projectilePrefab != null && firePoint != null)
        {
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            EnemyProjectile projectileScript = proj.GetComponent<EnemyProjectile>();
            
            if (projectileScript != null)
            {
                float shootDirection = _movingRight ? 1f : -1f;
                projectileScript.Setup(shootDirection, damageToPlayer); // Передаем урон из базы 
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 start = Application.isPlaying ? _startPosition : transform.position;
        Gizmos.DrawLine(start + Vector3.left * patrolDistance, start + Vector3.right * patrolDistance);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}