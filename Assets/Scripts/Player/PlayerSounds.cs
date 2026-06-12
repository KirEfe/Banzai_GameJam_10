using UnityEngine;
using UnityEngine.Events;

public class PlayerSounds : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    [Header("Шаги")]
    [SerializeField] private AudioClip[] footstepClips;

    [Header("Прыжок и приземление")]
    [SerializeField] private AudioClip[] jumpClips;
    [SerializeField] private AudioClip[] landClips;

    [Header("Атака")]
    [SerializeField] private AudioClip[] attackClips;

    [Header("Получение урона")]
    [SerializeField] private AudioClip[] hurtClips;

    [Header("Дополнительные эффекты")]
    public UnityEvent onFootstepFX;
    public UnityEvent onJumpFX;
    public UnityEvent onLandFX;
    public UnityEvent onAttackFX;
    public UnityEvent onHurtFX;

    private int _lastFootstepIndex = -1;
    private int _lastJumpIndex = -1;
    private int _lastLandIndex = -1;
    private int _lastAttackIndex = -1;
    private int _lastHurtIndex = -1;

    private float _footstepCooldown;
    private const float FootstepCooldownTime = 0.15f;

    // Вызывается через Animation Event
    public void PlayFootstep()
    {
        if (Time.time < _footstepCooldown) return;
        _footstepCooldown = Time.time + FootstepCooldownTime;

        onFootstepFX?.Invoke();
        PlayRandom(footstepClips, ref _lastFootstepIndex);
    }

    // Вызывается из PlayerController
    public void PlayJump()
    {
        onJumpFX?.Invoke();
        PlayRandom(jumpClips, ref _lastJumpIndex);
    }

    public void PlayLand()
    {
        onLandFX?.Invoke();
        PlayRandom(landClips, ref _lastLandIndex);
    }

    // Вызывается из PlayerController или Animation Event
    public void PlayAttack()
    {
        onAttackFX?.Invoke();
        PlayRandom(attackClips, ref _lastAttackIndex);
    }

    public void PlayHurt()
    {
        onHurtFX?.Invoke();
        PlayRandom(hurtClips, ref _lastHurtIndex);
    }

    private void PlayRandom(AudioClip[] clips, ref int lastIndex)
    {
        if (clips == null || clips.Length == 0 || audioSource == null) return;

        int index;
        do { index = Random.Range(0, clips.Length); }
        while (clips.Length > 1 && index == lastIndex);

        lastIndex = index;
        audioSource.PlayOneShot(clips[index]);
    }
}