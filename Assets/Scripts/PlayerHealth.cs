using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Здоровье")]
    [SerializeField] private int maxHealth = 3;

    [Header("Неуязвимость после удара")]
    [SerializeField] private float invincibilityDuration = 1.5f;

    [Header("События")]
    public UnityEvent onDamaged;
    public UnityEvent onDeath;

    private int _currentHealth;
    private float _invincibilityTimer;
    private bool _isDead;

    public int CurrentHealth => _currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    private void Update()
    {
        if (_invincibilityTimer > 0f)
            _invincibilityTimer -= Time.deltaTime;
    }

    public void TakeDamage(int damage)
    {
        if (_invincibilityTimer > 0f || _isDead) return;

        _currentHealth -= damage;
        _invincibilityTimer = invincibilityDuration;
        onDamaged?.Invoke();

        if (_currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        _isDead = true;
        onDeath?.Invoke();
    }

    // Вызывается при респауне
    public void Revive()
    {
        _currentHealth = maxHealth;
        _isDead = false;
        _invincibilityTimer = 0f;
    }
}