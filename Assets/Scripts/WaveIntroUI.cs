using UnityEngine;
using TMPro;
using System.Collections;

public class WaveIntroUI : MonoBehaviour
{
    public static WaveIntroUI Instance;   // เรียกใช้จากสคริปต์อื่นได้

    [Header("References")]
    public CanvasGroup blackOverlay;      // Canvas Group ของ BlackOverlay
    public CanvasGroup waveGroup;         // Canvas Group ของ WaveGroup
    public TextMeshProUGUI waveText;

    [Header("Timing (seconds)")]
    public float holdBlack = 1.0f;        // จอดำค้างพร้อมข้อความ
    public float fadeBlack = 1.5f;        // ความมืดค่อยๆ จาง
    public float holdText = 1.0f;         // ข้อความค้างบนฉาก
    public float fadeText = 1.0f;         // ข้อความค่อยๆ หาย

    [Header("Start")]
    public bool playOnStart = true;
    public int startWave = 1;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (playOnStart)
            ShowWave(startWave, true);
        else
            SetAlpha(0f, 0f);
    }

    // เรียกตอนเริ่ม wave ใหม่ — fromBlack = true คือเริ่มจากจอดำ (ใช้ตอนเริ่มเกม)
    public void ShowWave(int waveNumber, bool fromBlack = false)
    {
        StopAllCoroutines();
        if (waveText != null)
            waveText.text = "WAVE " + waveNumber;
        StartCoroutine(IntroRoutine(fromBlack));
    }

    IEnumerator IntroRoutine(bool fromBlack)
    {
        // จังหวะที่ 1: จอดำ + ข้อความ
        SetAlpha(fromBlack ? 1f : 0f, 1f);
        yield return new WaitForSeconds(holdBlack);

        // จังหวะที่ 2: ความมืดจางลง ข้อความยังอยู่
        if (fromBlack)
            yield return Fade(blackOverlay, 1f, 0f, fadeBlack);
        yield return new WaitForSeconds(holdText);

        // จังหวะที่ 3: ข้อความค่อยๆ หาย
        yield return Fade(waveGroup, 1f, 0f, fadeText);
    }

    IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null) yield break;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        group.alpha = to;
    }

    void SetAlpha(float black, float text)
    {
        if (blackOverlay != null) blackOverlay.alpha = black;
        if (waveGroup != null) waveGroup.alpha = text;
    }
}