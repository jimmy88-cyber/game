using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;
    public UnityEvent onDeath;
    public UnityEvent<int> onTakeDamage;
    [Header("Hit Reaction")]
    public string hitTriggerName = "Hit";   // ?? ��駪��� trigger ��ҡ Inspector
    [Header("Death Settings")]
    public bool destroyOnDeath = true;   // ?? ����������͡��� � Enemy = true, Player = false
    private Animator animator;
    private EnemyAI enemyAI;  // ?? ��������ù��
    private PlayerController playerController;   // ?? ��������ù��
    private CompanionAI companionAI;
    private bool isDead = false;
    public bool IsDead => isDead;   // ?? ���� property ���ʤ�Ի���������

    [Header("Damage Feedback (Player Only)")]
    public DamageFlash damageFlash;      // ?? �ҡ DamageFlash object ���ç��� (੾�� Player)
    public CameraFollow cameraShake;     // ?? �ҡ Main Camera ���ç��� (੾�� Player)
    public GameObject hpBarUI;  // ?? ������÷Ѵ��� � �ҡ PlayerHPbar ��� (੾�� Player)
    public AudioSource hurtAudioSource;      // ?? ������÷Ѵ���
    public AudioClip[] hurtClips;            // ?? ������÷Ѵ��� � ���§��ͧ������Ẻ
    [Range(0f, 1f)] public float hurtVolume = 0.9f;   // ?? ������÷Ѵ���
    public DeathScreenUI deathScreenUI;   // 🆕 เพิ่มบรรทัดนี้ — ลาก DeathScreen object ใส่

    [Header("Damage Feedback (Enemy Only)")]        // ?? ������ǹ��������
    public AudioSource enemyHurtAudioSource;
    public AudioClip[] enemyHurtClips;
    [Range(0f, 1f)] public float enemyHurtVolume = 0.8f;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponentInChildren<Animator>();
        enemyAI = GetComponent<EnemyAI>();  // ?? ������÷Ѵ���
        playerController = GetComponent<PlayerController>();   // ?? ������÷Ѵ���
        companionAI = GetComponent<CompanionAI>();
    }

    public void TakeDamage(int amount , Transform attacker = null)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        onTakeDamage?.Invoke(currentHealth);
        Debug.Log($"{gameObject.name} HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // ?? ����� Player ��С��ѧ Block ���� ���ѧ�Ѻ��ԡ Block ��͹ (��ͧ�ѹ Trigger ��ҧ)
            PlayerController playerController = GetComponent<PlayerController>();
           
            if (playerController != null)
                playerController.ForceStopBlocking();
           
            if (companionAI != null)
                companionAI.ForceStopBlocking();

            if (animator != null)
                animator.SetTrigger(hitTriggerName);

            if (enemyAI != null && attacker != null)
                enemyAI.SetAggroTarget(attacker);

            if (enemyAI != null)
                enemyAI.StunFor(2f);

            // ?? ������ǹ��� � �Ϳ࿡��˹�Ҩ� (�зӧҹ��͹����駤����� �� ੾�� Player)
            if (damageFlash != null)
                damageFlash.Flash();

            if (cameraShake != null)
                cameraShake.Shake();

            PlayHurtSound();   // ?? ������÷Ѵ���
            PlayEnemyHurtSound();
        }
    }

    // ?? �����ѧ��ѹ�������� (�ҧ�����ѧ TakeDamage() ����)
    void PlayHurtSound()
    {
        if (hurtClips.Length == 0 || hurtAudioSource == null) return;
        AudioClip clip = hurtClips[Random.Range(0, hurtClips.Length)];
        hurtAudioSource.PlayOneShot(clip, hurtVolume);
    }

    // ?? �����ѧ��ѹ��������
    void PlayEnemyHurtSound()
    {
        if (enemyHurtClips.Length == 0 || enemyHurtAudioSource == null) return;
        AudioClip clip = enemyHurtClips[Random.Range(0, enemyHurtClips.Length)];
        enemyHurtAudioSource.PlayOneShot(clip, enemyHurtVolume);
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log($"{gameObject.name} died!");

        if (animator != null)
            animator.SetTrigger("Death");

        // ?? ������÷Ѵ��� � ������ Enemy ��ش��ʹ���
        if (enemyAI != null)
            enemyAI.SetDead();

        // ?? ������ǹ��� � ����� Player ���Դʤ�Ի��Ǻ����ѹ��
        if (playerController != null)
            playerController.SetDead();

        if (companionAI != null)
            companionAI.SetDead();

        if (damageFlash != null)          // ?? ������÷Ѵ��� � ��ҧ��ᴧ���͹���
            damageFlash.SetDeathRed();

       if ( hpBarUI != null)       // ?? ������ǹ��� � ��͹ HP bar �͹���
            hpBarUI.SetActive(false);


        if (deathScreenUI != null)             // 🆕 เพิ่มส่วนนี้
        deathScreenUI.ShowDeathScreen();

     onDeath?.Invoke();
        if (destroyOnDeath)
        Destroy(gameObject, 3f);
    }

    // ?? �����ѧ��ѹ������� HealthSystem.cs (�ҧ�����ѧ Die() ����)
    public void ResetHealth()
    {
        isDead = false;
        currentHealth = maxHealth;
        onTakeDamage?.Invoke(currentHealth);
        Debug.Log($"{gameObject.name} ��鹤׹�վ HP: {currentHealth}/{maxHealth}");
    }


}