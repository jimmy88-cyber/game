using UnityEngine;
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Background Music")]
    public AudioSource bgmSource;
    public AudioClip normalBGM;
    public AudioClip combatBGM;

    [Header("Ambient")]
    public AudioSource ambientSource;

    void Awake()
    {
        // ไม่ใช้ DontDestroyOnLoad — ให้เพลงในเกมถูกทำลายไปพร้อมฉาก
        // ไม่งั้นกลับไปหน้า Main Menu แล้วเสียงเกมจะเล่นซ้อนกับเพลงเมนู
        Instance = this;
    }

    void Start()
    {
        if (bgmSource != null && normalBGM != null)
        {
            bgmSource.clip = normalBGM;
            bgmSource.Play();
        }
    }

    public void SwitchToCombatMusic()
    {
        if (bgmSource.clip == combatBGM) return;
        bgmSource.clip = combatBGM;
        bgmSource.Play();
    }

    public void SwitchToNormalMusic()
    {
        if (bgmSource.clip == normalBGM) return;
        bgmSource.clip = normalBGM;
        bgmSource.Play();
    }

    public void FadeOutBGM(float duration)
    {
        StartCoroutine(FadeRoutine(bgmSource, 0f, duration));
    }

    System.Collections.IEnumerator FadeRoutine(AudioSource source, float targetVolume, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }
        source.volume = targetVolume;
    }
}