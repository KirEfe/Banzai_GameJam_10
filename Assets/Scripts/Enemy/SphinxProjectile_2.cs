using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class SphinxProjectile_2 : MonoBehaviour
{
    [Header("Настройки физики качения")]
    [SerializeField] private float launchForceX = 4f;    // Сила плевка вперед
    [SerializeField] private float launchForceY = 1f;    // Легкий подброс вверх при спавне
    [SerializeField] private float lifetime = 5f;        // Сколько секунд катится до саморазрушения

    [Header("События для художника")]
    public UnityEvent onVanishEvent; // Срабатывает при взрыве/исчезновении клубка (для эффектов ниток/пыли/звука)

    private Rigidbody2D _rb;
    private Collider2D _collider;
    private SpriteRenderer _renderer;
    private int _damage;
    private bool _isVanishing;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<CircleCollider2D>();
        _renderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Инициализация шара Сфинксом в момент спавна
    /// </summary>
    public void Setup(float direction, int damage)
    {
        _damage = damage;

        if (_rb != null)
        {
            // Задаем начальную скорость: толкаем вперед и чуть-чуть приподнимаем
            _rb.linearVelocity = new Vector2(direction * launchForceX, launchForceY);
            
            // ЛАЙФХАК: Принудительно закручиваем шарик по оси, чтобы он сразу начал сочно вращаться
            _rb.angularVelocity = -direction * 500f;
        }

        // Запускаем таймер уничтожения по времени
        StartCoroutine(LifetimeRoutine());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_isVanishing) return;

        // 1. Проверяем попадание в Смертного бога
        PlayerHealth player = collision.gameObject.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(_damage);
            Vanish();
            return;
        }

        // 2. Проверяем столкновение со стенами гробницы
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // Проверяем точки контакта: если шар ударился во что-то боком (нормаль по X), значит это стена.
            // Если он просто катится по полу (нормаль по Y), мы его не трогаем.
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (Mathf.Abs(contact.normal.x) > 0.7f) // Сильный боковой удар в стену/препятствие
                {
                    Vanish();
                    break;
                }
            }
        }
    }

    private IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(lifetime);
        if (!_isVanishing) Vanish();
    }

    private void Vanish()
    {
        _isVanishing = true;
        
        // Отключаем физику и графику, чтобы шар мгновенно исчез для игрока
        _rb.simulated = false;
        _collider.enabled = false;
        if (_renderer != null) _renderer.enabled = false;

        // Пингуем художника — в этот момент сработают его партиклы и звуки
        onVanishEvent?.Invoke();

        // Уничтожаем объект окончательно через 1 секунду. 
        // Задержка нужна, чтобы AudioSource или Particle System внутри объекта успели доиграть до конца!
        Destroy(gameObject, 1f);
    }
}