using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        other.GetComponent<PlayerHealth>()?.TakeDamage(999);
    }
}