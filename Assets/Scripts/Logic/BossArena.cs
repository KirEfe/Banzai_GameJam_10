using UnityEngine;
using UnityEngine.Events;

public class BossArena : MonoBehaviour
{
    [SerializeField] private ArcherBoss boss;

    [Header("События для художника")]
    public UnityEvent onBattleStart; // музыка, эффекты

    private bool _battleStarted;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_battleStarted) return;
        if (!other.CompareTag("Player")) return;

        _battleStarted = true;
        onBattleStart?.Invoke();
        boss.StartBattle();
    }
}