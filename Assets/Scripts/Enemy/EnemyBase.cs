using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Здоровье")]
    [SerializeField] protected int maxHealth = 3;

    [Header("Урон игроку")]
    [SerializeField] protected int damageToPlayer = 1;

    [Header("Смерть")]
    [SerializeField] private float deathAnimationDuration = 1f;

    [Header("После получения удара")]
    [SerializeField] private float invincibilityDuration = 0.5f;
    [SerializeField] private float hitStunDuration = 0.4f;

    [Header("События для художника")]
    public UnityEvent onDamagedEvent;
    public UnityEvent onDeathEvent;
    public UnityEvent onStartMoving;
    public UnityEvent onStopMoving;

    protected int CurrentHealth;
    protected bool IsDead;
    protected bool IsStunned;
    protected Animator Animator;
    private bool _isMoving;
    private float _invincibilityTimer;

    protected virtual void Awake()
    {
        CurrentHealth = maxHealth;
        Animator = GetComponent<Animator>();
    }

    protected void SetMoving(bool moving)
    {
        if (_isMoving == moving) return;
        _isMoving = moving;

        if (Animator != null)
            Animator.SetBool("IsWalking", moving);

        if (moving)
            onStartMoving?.Invoke();
        else
            onStopMoving?.Invoke();
    }

    protected void ResetMovingState()
    {
        _isMoving = false;
        if (Animator != null)
            Animator.SetBool("IsWalking", false);
    }

    protected virtual void Update()
    {
        if (_invincibilityTimer > 0f)
            _invincibilityTimer -= Time.deltaTime;
    }

    public virtual void TakeDamage(int damage)
    {
        if (IsDead) return;
        if (_invincibilityTimer > 0f) return;

        CurrentHealth -= damage;
        _invincibilityTimer = invincibilityDuration;

        OnDamaged();

        if (CurrentHealth <= 0)
            Die();
        else
            StartCoroutine(HitStunRoutine());
    }

    private IEnumerator HitStunRoutine()
    {
        IsStunned = true;
        yield return new WaitForSeconds(hitStunDuration);
        IsStunned = false;
    }

    protected virtual void OnDamaged()
    {
        onDamagedEvent?.Invoke();
    }

    protected virtual void Die()
    {
        if (IsDead) return;
        IsDead = true;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        onDeathEvent?.Invoke();
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(deathAnimationDuration);
        Destroy(gameObject);
    }

    protected virtual void DealDamageToPlayer(Collider2D other)
    {
        other.GetComponent<PlayerHealth>()?.TakeDamage(damageToPlayer);
    }
}