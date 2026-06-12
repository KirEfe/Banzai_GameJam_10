using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Sphinx : EnemyBase
{
    [Header("Снаряды")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float shootInterval = 2f;
    [SerializeField] private float projectileSpeed = 5f;
    [SerializeField] private float projectileLifetime = 5f;

    [Header("Направление стрельбы")]
    [SerializeField] private bool shootLeft = true;

    [Header("События для художника")]
    public UnityEvent onShoot;

    protected override void Awake()
    {
        base.Awake();
        StartCoroutine(ShootRoutine());
    }

    private IEnumerator ShootRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(shootInterval);
            Shoot();
        }
    }

    private void Shoot()
    {
        if (projectilePrefab == null || shootPoint == null) return;

        onShoot?.Invoke();

        GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            Vector2 direction = shootLeft ? Vector2.left : Vector2.right;
            rb.linearVelocity = direction * projectileSpeed;
        }

        Destroy(projectile, projectileLifetime);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
            other.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(damageToPlayer);
    }

    private void OnDrawGizmosSelected()
    {
        if (shootPoint == null) return;

        Gizmos.color = Color.red;
        Vector3 direction = shootLeft ? Vector3.left : Vector3.right;
        Gizmos.DrawLine(shootPoint.position, shootPoint.position + direction * 3f);
        Gizmos.DrawSphere(shootPoint.position, 0.1f);
    }
}