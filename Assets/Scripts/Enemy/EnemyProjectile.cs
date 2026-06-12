using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Настройки полета по дуге")]
    [SerializeField] private float launchForceX = 4f; // Скорость полета вперед (сделай меньше, чтобы летела медленно)
    [SerializeField] private float launchForceY = 6f; // Сила броска вверх (определяет высоту дуги)
    [SerializeField] private float lifetime = 5f;     // Навесная стрела может лететь чуть дольше

    private Rigidbody2D _rb;
    private int _damage;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void Setup(float direction, int damage)
    {
        _damage = damage;

        if (_rb != null)
        {
            // Задаем начальную скорость: толкаем стрелу вперед по горизонтали и подбрасываем вверх
            _rb.linearVelocity = new Vector2(direction * launchForceX, launchForceY);
        }

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Эффект вращения: стрела всегда смотрит туда, куда летит
        if (_rb != null && _rb.linearVelocity.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(_rb.linearVelocity.y, _rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(_damage);
            Destroy(gameObject); 
            return;
        }

        // Если стрела воткнулась в землю
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}