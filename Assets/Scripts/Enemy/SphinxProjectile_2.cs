using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class SphinxProjectile_2 : MonoBehaviour
{
    [Header("Настройки физики качения")]
    [SerializeField] private float launchForceX = 4f;    
    [SerializeField] private float launchForceY = 1f;    
    [SerializeField] private float lifetime = 5f;        

    [Header("События для художника")]
    public UnityEvent onVanishEvent; 

    // СТРАХОВКА: Если Сфинкс забыл вызвать Setup(), урон все равно будет равен 1, а не 0
    private int _damage = 1; 
    
    private Rigidbody2D _rb;
    private bool _isVanishing;
    private Collider2D[] _colliders;
    private SpriteRenderer _renderer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _colliders = GetComponents<Collider2D>(); 
        _renderer = GetComponent<SpriteRenderer>();
    }

    public void Setup(float direction, int damage)
    {
        _damage = damage;

        if (_rb != null)
        {
            _rb.linearVelocity = new Vector2(direction * launchForceX, launchForceY);
            _rb.angularVelocity = -direction * 500f;
        }

        StartCoroutine(LifetimeRoutine());
    }

    // ВАРИАНТ 1: Если сработал коллайдер-триггер
    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDealDamage(other.gameObject);
    }

    // ВАРИАНТ 2: Если твердый коллайдер оттолкнулся от игрока раньше триггера
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_isVanishing) return;

        // Сначала проверяем, не врезались ли мы челом в Смертного бога
        if (TryDealDamage(collision.gameObject)) return;

        // Если это не игрок, проверяем стены гробницы
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (Mathf.Abs(contact.normal.x) > 0.7f) 
                {
                    Vanish();
                    break;
                }
            }
        }
    }

    // Универсальный метод проверки и нанесения урона
    private bool TryDealDamage(GameObject hitObject)
    {
        if (_isVanishing) return false;

        // Ищем скрипт здоровья (безопасный поиск GetComponentInParent)
        PlayerHealth player = hitObject.GetComponentInParent<PlayerHealth>();
        
        if (player != null)
        {
            player.TakeDamage(_damage);
            Vanish();
            return true; // Урон нанесен успешно
        }

        return false;
    }

    private IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(lifetime);
        if (!_isVanishing) Vanish();
    }

    private void Vanish()
    {
        _isVanishing = true;
        
        if (_rb != null) _rb.simulated = false;
        
        foreach (var col in _colliders)
        {
            if (col != null) col.enabled = false;
        }
        
        if (_renderer != null) _renderer.enabled = false;

        onVanishEvent?.Invoke();
        Destroy(gameObject, 1f);
    }
}