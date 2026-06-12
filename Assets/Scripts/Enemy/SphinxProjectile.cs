using UnityEngine;
using UnityEngine.Events;

public class SphinxProjectile : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private LayerMask groundLayer;

    [Header("События для художника")]
    public UnityEvent onHitPlayer;   // попал в игрока
    public UnityEvent onHitGround;   // попал в землю или стену

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(damage);
            onHitPlayer?.Invoke(); // партикли попадания
            Destroy(gameObject);
            return;
        }

        if (((1 << other.gameObject.layer) & groundLayer) != 0)
        {
            onHitGround?.Invoke(); // партикли разбивания об землю
            Destroy(gameObject);
        }
    }
}