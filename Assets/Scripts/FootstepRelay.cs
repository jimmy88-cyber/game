using UnityEngine;
public class FootstepRelay : MonoBehaviour
{
    [Header("Footstep Settings")]
    public AudioSource audioSource;
    public AudioClip[] footstepClips;
    [Range(0f, 1f)] public float volume = 0.7f;

    public PlayerController playerController;

    public void PlayFootstep()
    {
        if (footstepClips.Length == 0 || audioSource == null) return;

        // ?? เพิ่มเงื่อนไขนี้ — ถ้ากำลัง Block อยู่ ไม่ต้องเล่นเสียงเดิน
        if (playerController != null && playerController.IsBlocking)
            return;

        audioSource.Stop();
        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        audioSource.PlayOneShot(clip, volume);
    }

    // ?? เพิ่มฟังก์ชันนี้ทั้งหมด — เรียกตอนหยุดเดิน
    public void StopFootstep()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }
}