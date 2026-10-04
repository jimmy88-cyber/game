using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [Header("Movement")]
    public float moveSpeed = 10f;
    public float rotateSpeed = 10f;
    public float waterMoveSpeed = 1f;
    private bool isInWater = false;

    [Header("Jump")]
    public float jumpForce = 5f;
    private bool isGrounded = true;

    [Header("Combat")]
    public float attackRange = 1.5f;
    public int attackDamage = 35;
    public float attackCooldown = 0.8f;
    public FootstepRelay footstepRelay;   // 🆕 เพิ่มบรรทัดนี้ — ลาก PlayerModel (ตัวที่มี FootstepRelay) ใส่

    [Header("Action Sounds")]              // 🆕 เพิ่มส่วนนี้ทั้งหมด
    public AudioSource actionAudioSource;
    public AudioClip[] kickClips;
    public AudioClip[] rollClips;
    public AudioClip[] dodgeBackClips;
    public AudioClip[] jumpClips;
    [Range(0f, 1f)] public float actionVolume = 0.8f;

    [Header("Combo Settings")]
    public int comboThreshold = 3;
    public float comboResetTime = 1.5f;
    public float comboCooldown = 0.5f;  //4

    [Header("Fall Death")]
    public float fallDeathY = -10f;

    [Header("Kick Settings")]
    public float kickKnockback = 1f;   // 🆕 ระยะที่ดัน Enemy ถอยหลัง

    private Rigidbody rb;
    private float attackTimer = 0f;
    private float comboCooldownTimer = 0f;
    private bool isInCombo = false;
    private HealthSystem health;
    private Animator animator;
    private CameraFollow cameraFollow;
    private bool isDodging = false;
    private bool isBlocking = false;
    public bool IsBlocking => isBlocking; ////
    private bool isRolling = false;
    private Quaternion blockRotation;
    private bool isDead = false;
    private int clickCount = 0;
    private float comboTimer = 0f;

    private Vector3 spawnPosition;       // 🆕
    private Quaternion spawnRotation;    // 🆕

    // เรียกจาก Animation Event
    public void DealDamage()
    {
        PerformAttack(attackDamage);
        Debug.Log("💥 Animation Hit!");
    }

    // 🆕 เรียกจาก Animation Event เฉพาะคลิป Kick (แยกจาก DealDamage เดิม)
    public void DealKickDamage()
    {
        PerformKickAttack(attackDamage, kickKnockback);
        Debug.Log("🦵 Kick Hit! Enemy ถูกดันถอยหลัง");
    }

    // 🆕 เพิ่มฟังก์ชันนี้ — บังคับยกเลิก Block ทันที (ใช้ตอนโดนตีจากด้านหลัง/ข้าง)
    public void ForceStopBlocking()
    {
        isBlocking = false;
        if (animator != null)
            animator.SetBool("Block", false);
    }
    // เพิ่มฟังก์ชันนี้ (วางไว้ใกล้ๆ ForceStopBlocking ที่เพิ่มไปก่อนหน้า)
    public void SetDead()
    {
        isDead = true;

        // หยุดการเคลื่อนไหวทันที
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;   // ล็อกไม่ให้ physics ขยับต่อ
        }

        // ยกเลิกสถานะต่างๆ ที่ค้างอยู่ (กันไม่ให้ animation อื่นแทรกทับท่า Death)
        isBlocking = false;
        isDodging = false;
        isRolling = false;
    }

    // 🆕 เรียกจาก WaveManager ตอนเริ่ม wave ใหม่
    public void ResetForWave()
    {
    StopAllCoroutines();   // หยุด Dodge/Roll ที่อาจค้างอยู่
    isDodging = false;
    isRolling = false;
    isBlocking = false;
    isDead = false;

    if (animator != null)
    {
        animator.SetBool("Block", false);
        animator.SetFloat("Speed", 0f);
    }

    rb.isKinematic = false;
    rb.linearVelocity = Vector3.zero;
    rb.angularVelocity = Vector3.zero;
    rb.position = spawnPosition;
    transform.position = spawnPosition;
    transform.rotation = spawnRotation;

    if (health != null)
        health.ResetHealth();   // เลือดเต็มตอนเริ่ม wave ใหม่
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        health = GetComponent<HealthSystem>();
        animator = GetComponentInChildren<Animator>();
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
        spawnPosition = transform.position;   // 🆕
        spawnRotation = transform.rotation;   // 🆕
    }

    void Update()
    {
        if (isDead) return;  // 🆕 เพิ่มบรรทัดนี้ — ถ้าตายแล้ว ไม่ทำอะไรเลยทั้งสิ้น
        if (PauseMenu.IsPaused) return;   // 🆕 เพิ่มบรรทัดนี้
        HandleMovement();
        HandleJump();
        HandleAttack();
        CheckFallDeath();
        HandleComboTimer();
    }

    void HandleComboTimer()
    {
        if (clickCount > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
            {
                clickCount = 0;
                comboTimer = 0f;
            }
        }

        if (isInCombo)
        {
            comboCooldownTimer -= Time.deltaTime;
            if (comboCooldownTimer <= 0f)
                isInCombo = false;
        }
    }

    void HandleMovement()
    {
        if (isDodging) return;
        if (isBlocking) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float cameraAngle = cameraFollow != null ? cameraFollow.currentAngleY : 0f;
        Quaternion camRotation = Quaternion.Euler(0, cameraAngle, 0);
        Vector3 dir = camRotation * new Vector3(h, 0, v).normalized;

        if (animator != null)
            animator.SetFloat("Speed", dir.magnitude);

        if (dir.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
            float currentSpeed = isInWater ? waterMoveSpeed : moveSpeed;
            Vector3 moveVelocity = dir * currentSpeed;
            rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            if (footstepRelay != null)
                footstepRelay.StopFootstep();
        }
    }

    void HandleJump()
    {
        if (isRolling) return;
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            // 🆕 เพิ่มส่วนนี้ — ยกเลิก Block ก่อนกระโดด (กันปัญหาเดียวกัน)
            if (isBlocking)
            {
                isBlocking = false;
                if (animator != null)
                    animator.SetBool("Block", false);
            }

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
            if (animator != null)
                animator.SetTrigger("Jump");
            PlayActionSound(jumpClips);
        }
    }

    void CheckFallDeath()
    {
        if (transform.position.y < fallDeathY)
            health.TakeDamage(health.maxHealth);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            isInWater = true;
            Debug.Log("Water Zone");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            isInWater = false;
            Debug.Log("Water out");
        }
    }

    void HandleAttack()
    {
        attackTimer -= Time.deltaTime;
        if (isRolling) return;

        // คลิกซ้าย → Attack / Combo
        if (Input.GetMouseButtonDown(0) && attackTimer <= 0f && !isInCombo)
        {
             // 🆕 ถ้ากำลัง Block อยู่ ให้ยกเลิกก่อน เพื่อไม่ให้ Trigger Attack ค้างรอ Bool Block
            if (isBlocking)
         {
             isBlocking = false;
             if (animator != null)
            animator.SetBool("Block", false);
         }
            
            attackTimer = attackCooldown;
            clickCount++;
            comboTimer = comboResetTime;

            if (clickCount >= comboThreshold)
            {
                clickCount = 0;
                comboTimer = 0f;
                isInCombo = true;
                comboCooldownTimer = comboCooldown;
                if (animator != null)
                    animator.SetTrigger("Combo");
                Debug.Log("💥 COMBO!");
            }
            else
            {
                if (animator != null)
                    animator.SetTrigger("Attack");
                Debug.Log($"⚔️ Attack! ({clickCount}/{comboThreshold})");
            }
            
        }

        // กด F → Kick
        if (Input.GetKeyDown(KeyCode.F) && attackTimer <= 0f && !isInCombo)
        {
            attackTimer = attackCooldown;
            if (animator != null)
                animator.SetTrigger("Kick");
            PlayActionSound(kickClips);
           
            Debug.Log("🦵 Kick!");
        }

        // กด C → Roll
        if (Input.GetKeyDown(KeyCode.C) && !isDodging && !isRolling)
        {
            // 🆕 ยกเลิก Block ทันที ก่อนเริ่ม Roll (กันสถานะค้าง)
            if (isBlocking)
            {
                isBlocking = false;
                if (animator != null)
                    animator.SetBool("Block", false);
            }

            float cameraAngle = cameraFollow != null ? cameraFollow.currentAngleY : 0f;
            Quaternion camRotation = Quaternion.Euler(0, cameraAngle, 0);
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            Vector3 inputDir = new Vector3(h, 0, v).normalized;
            Vector3 dodgeDir = inputDir.magnitude > 0.1f
                ? camRotation * inputDir
                : camRotation * Vector3.forward;
            transform.rotation = Quaternion.LookRotation(dodgeDir);
            if (animator != null)
                animator.SetTrigger("Roll");
            PlayActionSound(rollClips);
            StartCoroutine(DodgeRoutine(dodgeDir, 10f));
        }

        // กด E → Dodge หลัง
        if (Input.GetKeyDown(KeyCode.E) && !isDodging && !isRolling)
        {

            // 🆕 เพิ่มส่วนนี้ — ยกเลิก Block ทันที ก่อนเริ่ม DodgeBack (เหมือนกับที่ทำใน Roll)
            if (isBlocking)
            {
                isBlocking = false;
                if (animator != null)
                    animator.SetBool("Block", false);
                if (footstepRelay != null)
                    footstepRelay.StopFootstep();
            }

            if (animator != null)
                animator.SetTrigger("DodgeBack");
            PlayActionSound(dodgeBackClips);
            StartCoroutine(DodgeRoutine(-transform.forward, 8f));
        }

        // คลิกขวา → Block
        if (Input.GetMouseButtonDown(1) && !isInCombo)
        {   if (!isBlocking)
            {
            isBlocking = true;
            if (animator != null)
                animator.SetBool("Block", true);
            if (footstepRelay != null)              // 🆕 เพิ่มบรรทัดนี้
                 footstepRelay.StopFootstep();
                float cameraAngle = cameraFollow != null ? cameraFollow.currentAngleY : 0f;
            blockRotation = Quaternion.Euler(0, cameraAngle, 0);
            }        
        }
        
        else if (Input.GetMouseButtonUp(1))
        {
            isBlocking = false;
            if (animator != null)
                animator.SetBool("Block", false);
        }
        if (isBlocking)
            transform.rotation = blockRotation;
    }

    System.Collections.IEnumerator DodgeRoutine(Vector3 direction, float force)
    {
        isDodging = true;
        isRolling = true;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(direction * force, ForceMode.Impulse);

        yield return new WaitForSeconds(0.5f);
        isDodging = false;
        rb.linearVelocity = Vector3.zero;

        yield return new WaitForSeconds(0.5f); ///1.5
        isRolling = false;
    }

    void PerformAttack(int damage)
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position + transform.forward * attackRange, 1.5f);
        Debug.Log($"🔍 เจอ Collider ทั้งหมด {hits.Length} ชิ้น");  // เพิ่ม log นี้
        foreach (Collider hit in hits)
        {
            Debug.Log($"   - เจอ: {hit.gameObject.name}, Tag: {hit.tag}");  // เพิ่ม log นี้
            if (hit.CompareTag("Enemy"))
            {
                HealthSystem enemyHealsth = hit.GetComponentInParent<HealthSystem>();
                if (enemyHealsth != null)
                    enemyHealsth.TakeDamage(attackDamage, transform);
                Debug.Log($"⚔️ โจมตี Enemy สำเร็จ! ลดเลือดไป {damage}");
            }
           else
            {
                Debug.Log("⚠️ เจอ Tag Enemy แต่ไม่มี HealthSystem component!");
            }
        }
    }

    void PerformKickAttack(int damage, float knockback)
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position + transform.forward * attackRange, 1.5f);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                HealthSystem enemyHealth = hit.GetComponentInParent<HealthSystem>();
                if (enemyHealth != null)
                    enemyHealth.TakeDamage(damage, transform);

                // 🆕 ดัน Enemy ถอยหลังตามทิศทางที่ Player หันหน้า
                EnemyAI enemyAI = hit.GetComponentInParent<EnemyAI>();
                if (enemyAI != null)
                    enemyAI.ApplyKnockback(transform.forward, knockback);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position + transform.forward * attackRange, 0.8f);
    }

    // 🆕 เพิ่มฟังก์ชันนี้ทั้งหมด — ใช้ร่วมกันทุก action sound
    void PlayActionSound(AudioClip[] clips)
    {
        if (clips.Length == 0 || actionAudioSource == null) return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        actionAudioSource.PlayOneShot(clip, actionVolume);
    }
}