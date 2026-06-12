using UnityEngine;
using UnityEngine.Events;

public class WingsPickup : MonoBehaviour
{
    [Header("События для художника")]
    public UnityEvent onPickup; // партикли, звук получения

    private bool _collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_collected) return;

        PlayerAbilities abilities = other.GetComponent<PlayerAbilities>();
        if (abilities == null) return;

        _collected = true;
        abilities.UnlockWings();
        onPickup?.Invoke();
        Destroy(gameObject, 0.5f); // небольшая задержка чтобы успел сыграть эффект
    }
}