using UnityEngine;
using UnityEngine.Events;

public class MeleeEnemy : EnemyBase
{
    [Header("Патруль")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float patrolDistance = 3f;

    [Header("Атака")]
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float afterAttackDelay = 0.5f; // пауза после удара

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
    private float _afterAttackTimer;

    private enum State { Patrolling, Attacking, AfterAttack }
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
        base.Update();

        if (IsDead) return;

        // Стан после получения удара — стоим на месте
        if (IsStunned)
        {
            SetMoving(false);
            return;
        }

        _attackCooldownCounter -= Time.deltaTime;
        _afterAttackTimer -= Time.deltaTime;

        // Пауза после атаки — стоим на месте
        if (_afterAttackTimer > 0f)
        {
            SetMoving(false);
            return;
        }

        bool playerInRange = CheckPlayerInRange();

        if (playerInRange)
        {
            if (_currentState != State.Attacking)
            {
                _currentState = State.Attacking;
                SetMoving(false);
            }
            TryAttack();
        }
        else
        {
            if (_currentState != State.Patrolling)
            {
                _currentState = State.Patrolling;
                ResetMovingState();
                // SetMoving(true);
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
        transform.localScale = new Vector3(
            _movingRight ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z
        );
    }

        private bool CheckPlayerInRange()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackRange, playerLayer);
        if (hit == null) return false;

        // Проверяем что игрок спереди
        float directionToPlayer = hit.transform.position.x - transform.position.x;
        bool playerInFront = _movingRight ? directionToPlayer > 0 : directionToPlayer < 0;

        return playerInFront;
    }

        private void TryAttack()
    {
        if (_attackCooldownCounter > 0f) return;

        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackRange, playerLayer);
        if (hit == null) return;

        float directionToPlayer = hit.transform.position.x - transform.position.x;
        bool playerInFront = _movingRight ? directionToPlayer > 0 : directionToPlayer < 0;
        if (!playerInFront) return;

        _attackCooldownCounter = attackCooldown;
        _afterAttackTimer = afterAttackDelay;
        onAttack?.Invoke();

        hit.GetComponent<PlayerHealth>()?.TakeDamage(damageToPlayer);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 start = Application.isPlaying ? _startPosition : transform.position;
        Gizmos.DrawLine(start + Vector3.left * patrolDistance, start + Vector3.right * patrolDistance);
        Gizmos.DrawWireCube(start + Vector3.left * patrolDistance, Vector3.one * 0.2f);
        Gizmos.DrawWireCube(start + Vector3.right * patrolDistance, Vector3.one * 0.2f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
        if (wallCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(wallCheck.position, checkRadius); ///
        }
    }
}