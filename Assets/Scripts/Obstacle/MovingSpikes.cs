using UnityEngine;

public class MovingSpikes : MonoBehaviour
{
    [SerializeField] private float moveDistance = 1f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stayUpDuration = 1.5f;   // сколько торчат наружу
    [SerializeField] private float stayDownDuration = 2f;   // сколько прячутся
    [SerializeField] private int damage = 1;

    private Vector3 _downPosition;
    private Vector3 _upPosition;
    private float _timer;
    private bool _isUp;
    private bool _isMoving;

    private void Start()
    {
        _downPosition = transform.position;
        _upPosition = _downPosition + Vector3.up * moveDistance;
        _timer = stayDownDuration; // начинаем со скрытого состояния
    }

    private void Update()
    {
        if (_isMoving)
        {
            MoveSpikes();
        }
        else
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _isMoving = true;
                _isUp = !_isUp;
            }
        }
    }

    private void MoveSpikes()
    {
        Vector3 target = _isUp ? _upPosition : _downPosition;
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            transform.position = target;
            _isMoving = false;
            _timer = _isUp ? stayUpDuration : stayDownDuration;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        other.GetComponent<PlayerHealth>()?.TakeDamage(damage);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 down = transform.position;
        Vector3 up = transform.position + Vector3.up * moveDistance;

        // Нижняя позиция — синяя
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(down, Vector3.one * 0.3f);

        // Верхняя позиция — красная
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(up, Vector3.one * 0.3f);

        // Линия пути
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(down, up);
    }
}