using UnityEngine;

public class Spikes : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        other.GetComponent<PlayerHealth>()?.TakeDamage(damage);
    }
}