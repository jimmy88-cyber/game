using UnityEngine;
using TMPro;

public class EnemyCounterUI : MonoBehaviour
{
    public static EnemyCounterUI Instance;   // Singleton ���¡��ҡ����˹���� (����͹ AudioManager)

    [Header("UI Reference")]
    public TextMeshProUGUI counterText;

    private int aliveCount;
    private int totalCount;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // �Ѻ Enemy ����������� Tag "Enemy" 㹩ҡ�͹�������
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        totalCount = enemies.Length;
        aliveCount = totalCount;

        // �١ event �͹��¢ͧ���е��
        foreach (GameObject enemy in enemies)
        {
            HealthSystem hp = enemy.GetComponent<HealthSystem>();
            if (hp != null)
                hp.onDeath.AddListener(OnEnemyDied);
        }

        UpdateText();
    }

    void OnEnemyDied()
    {
        aliveCount = Mathf.Max(0, aliveCount - 1);
        UpdateText();

          // 🆕 ศัตรูหมดแล้ว แจ้ง WaveManager
        if (aliveCount == 0 && WaveManager.Instance != null)
        WaveManager.Instance.OnAllEnemiesDefeated();
    }

    // 🆕 เรียกตอนเริ่ม wave ใหม่
    public void ResetCount()
    {
    aliveCount = totalCount;
    UpdateText();
    }

    // ���¡�ҡ EnemyAI �͹ Respawn ���� ���͹Ѻ��ǹ���Ѻ���������
    public void OnEnemyRespawned()
    {
        aliveCount = Mathf.Min(totalCount, aliveCount + 1);
        UpdateText();
    }

    void UpdateText()
    {
        if (counterText != null)
            counterText.text = $"{aliveCount}/{totalCount}";
    }
}