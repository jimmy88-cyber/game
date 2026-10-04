using UnityEngine;

public class AttackEventRelay : MonoBehaviour
{
    private PlayerController playerController;

    [Header("Attack Sound Settings")]
    public AudioSource audioSource;
    public AudioClip[] attackClips;
    [Range(0f, 1f)] public float volume = 0.8f;

    void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
    }

    // เรียกจาก Animation Event แทน
    public void DealDamage()
    {
        if (playerController != null)
            playerController.DealDamage();
        PlayAttackSound();
    }

    // ?? เพิ่มฟังก์ชันนี้ — ส่งต่อคำสั่งไปยัง DealKickDamage ของ PlayerController
    public void DealKickDamage()
    {
        if (playerController != null)
            playerController.DealKickDamage();
        //PlayAttackSound();
    }
    void PlayAttackSound()
    {
        if (attackClips.Length == 0 || audioSource == null) return;
        audioSource.Stop();
        AudioClip clip = attackClips[Random.Range(0, attackClips.Length)];
        audioSource.PlayOneShot(clip, volume);
    }
}