using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    [Header("References")]
    public WaveIntroUI waveUI;          // ลาก WaveIntro ใส่
    public PlayerController player;     // ลาก Player ใส่

    [Header("Timing (seconds)")]
    public float delayBeforeWin = 2f;   // รอหลังฆ่าตัวสุดท้าย
    public float winFadeIn = 1f;        // ความเร็วที่จอมืดลง
    public float winHold = 2f;          // YOU WIN ค้างกี่วิ

    [Header("Text")]
    public string winMessage = "YOU WIN";
    public Color winColor = new Color(1f, 0.82f, 0.3f);   // เหลืองทอง
    public Color waveColor = Color.white;

    public int CurrentWave { get; private set; } = 1;
    private bool isTransitioning = false;
    private EnemyAI[] enemies;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // เก็บรายชื่อ Enemy ทั้งหมดไว้ตั้งแต่เริ่ม
        enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
    }

    // EnemyCounterUI จะเรียกฟังก์ชันนี้ตอนศัตรูเหลือ 0
    public void OnAllEnemiesDefeated()
    {
        if (isTransitioning) return;
        StartCoroutine(NextWaveRoutine());
    }

    IEnumerator NextWaveRoutine()
    {
        isTransitioning = true;

        // 1) รอ 2 วิ
        yield return new WaitForSeconds(delayBeforeWin);

        // 2) จอค่อยๆ มืดพร้อมข้อความ YOU WIN
        waveUI.StopAllCoroutines();
        waveUI.waveText.text = winMessage;
        waveUI.waveText.color = winColor;
        yield return FadeBoth(0f, 1f, winFadeIn);

        // 3) ค้างไว้ 2 วิ
        yield return new WaitForSeconds(winHold);

        // 4) ย้ายทุกอย่างกลับที่เดิมตอนที่จอยังดำอยู่ (ผู้เล่นมองไม่เห็นการวาร์ป)
        CurrentWave++;
        waveUI.waveText.color = waveColor;
        ResetBattlefield();

        // 5) โชว์ WAVE 2 บนจอดำ แล้วค่อยๆ สว่าง (ใช้ระบบเดิมใน WaveIntroUI)
        waveUI.ShowWave(CurrentWave, true);

        isTransitioning = false;
    }

    void ResetBattlefield()
    {
        foreach (EnemyAI e in enemies)
            if (e != null) e.ResetForWave();

        if (player != null)
            player.ResetForWave();

        if (EnemyCounterUI.Instance != null)
            EnemyCounterUI.Instance.ResetCount();
    }

    IEnumerator FadeBoth(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / duration);
            waveUI.blackOverlay.alpha = a;
            waveUI.waveGroup.alpha = a;
            yield return null;
        }
        waveUI.blackOverlay.alpha = to;
        waveUI.waveGroup.alpha = to;
    }
}