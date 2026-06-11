using System.Collections;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private float respawnDelay = 1.5f;

    private void Start()
    {
        playerHealth.onDeath.AddListener(OnPlayerDeath);
    }

    private void OnDestroy()
    {
        playerHealth.onDeath.RemoveListener(OnPlayerDeath);
    }

    private void OnPlayerDeath()
    {
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        // Отключаем управление пока игрок мёртв
        playerHealth.GetComponent<PlayerController>().enabled = false;

        yield return new WaitForSeconds(respawnDelay);

        // Возвращаем на точку старта
        playerHealth.transform.position = respawnPoint.position;
        playerHealth.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        // Восстанавливаем
        playerHealth.Revive();
        playerHealth.GetComponent<PlayerController>().enabled = true;
    }
}