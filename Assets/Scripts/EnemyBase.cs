using UnityEngine;
using UnityEngine.Events;

public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Здоровье")]
    [SerializeField] protected int maxHealth = 3;

    [Header("Урон игроку")]
    [SerializeField] protected int damageToPlayer = 1;

    [Header("События для художника")]
    public UnityEvent onDamagedEvent;
    public UnityEvent onDeathEvent;

    protected int CurrentHealth;

    protected virtual void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public virtual void TakeDamage(int damage)
    {
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
        onDeathEvent?.Invoke();
        Destroy(gameObject);
    }

    protected virtual void DealDamageToPlayer(Collider2D other)
    {
        other.GetComponent<PlayerHealth>()?.TakeDamage(damageToPlayer);
    }
}