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

    [Header("События для художника")]
    public UnityEvent onDamagedEvent;
    public UnityEvent onDeathEvent;
    public UnityEvent onStartMoving;
    public UnityEvent onStopMoving;

    protected int CurrentHealth;
    protected bool IsDead;
    private bool _isMoving;

    protected virtual void Awake()
    {
        CurrentHealth = maxHealth;
    }

    // Вызывай в наследнике когда враг начал двигаться
    protected void SetMoving(bool moving)
    {
        if (_isMoving == moving) return; // состояние не изменилось — не спамим событие

        _isMoving = moving;
        if (_isMoving)
            onStartMoving?.Invoke();  // художник вешает Play("Walk")
        else
            onStopMoving?.Invoke();   // художник вешает Play("Idle")
    }

    public virtual void TakeDamage(int damage)
    {
        if (IsDead) return;

        CurrentHealth -= damage;
        OnDamaged();

        if (CurrentHealth <= 0)
            Die();
    }

    protected virtual void OnDamaged()
    {
        onDamagedEvent?.Invoke();
    }

    protected virtual void Die()
    {
        if (IsDead) return;
        IsDead = true;

        // Отключаем физику и коллайдер сразу
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        onDeathEvent?.Invoke(); // художник вешает Play("Death")
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