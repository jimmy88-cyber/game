using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("AI Settings")]
    public float detectionRange = 100f;
    public float attackRange = 1.5f;
    public int attackDamage = 20;
    public float attackCooldown = 2.0f;

    [Header("Attack Sound")]              // 🆕 เพิ่มส่วนนี้ทั้งหมด
    public AudioSource attackAudioSource;
    public AudioClip[] attackClips;
    [Range(0f, 1f)] public float attackVolume = 0.8f;

    [Header("Block Clash Sound")]           // 🆕 เพิ่มส่วนนี้ทั้งหมด
    public AudioClip[] blockClashClips;     // เสียงโล่/ดาบกระทบกัน
    [Range(0f, 1f)] public float blockClashVolume = 0.9f;

    [Header("Water Check")]
    public float navSampleDistance = 5f;

    [Header("Respawn Settings")]
    public float respawnDelay = 10f; // เวลารอฟื้นคืนชีพ (วินาที) 
    public bool autoRespawn = false;   // 🆕 ปิดไว้ เพราะใช้ระบบ wave แทน

    [Header("Knockback Settings")]              // 🆕 เพิ่มส่วนนี้
    public float knockbackGroundOffset = 0f;    // 🆕 ปรับใน Inspector ถ้ายังจม/ลอย

    private Vector3 spawnPosition;  // 🆕 ตำแหน่งเริ่มต้น
    private Quaternion spawnRotation;   // 🆕 มุมหันหน้าเริ่มต้น

    private Transform target;          // 🆕 เปลี่ยนจาก "player" เป็น "target" (เป้าหมายปัจจุบัน อาจเป็น Player หรือ Companion)
    private Transform aggroTarget; // 🆕 เป้าหมายที่ถูกยั่วยุ (ใครตีมาล่าสุด)
    private float aggroTimer = 0f;  // 🆕 นับเวลาว่ายังโกรธอยู่ไหม
    public float aggroDuration = 6f; // 🆕 ระยะเวลาที่ยังคงเป้าหมาย aggro (ปรับได้)
    private NavMeshAgent agent;
    private HealthSystem health;
    private Animator animator;
    private float attackTimer = 0f;

    private bool isStunned = false;
    private bool isDead = false;
    private float stunTimer = 0f;

    private bool attackLocked = false;
    public float attackLockDuration = 1f;

    public float groundOffset = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<HealthSystem>();
        animator = GetComponentInChildren<Animator>();
        // ไม่ต้องหา Player ตอน Start แล้ว จะหาใหม่ทุกเฟรมแทน

        // 🆕 เก็บตำแหน่งเริ่มต้นไว้สำหรับ respawn
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
    }

    void Update()
    {
        if (isDead) return;

        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            agent.isStopped = true;
            agent.velocity = Vector3.zero;

            if (animator != null)
                animator.SetFloat("Speed", 0f);

            if (stunTimer <= 0f)
                isStunned = false;

            return;
        }

        // 🆕 หาเป้าหมายที่ใกล้ที่สุดทุกเฟรม (ระหว่าง Player กับ Companion)
        target = FindNearestTarget();


        // if (target == null) return;   // ไม่มีเป้าหมายเลย ไม่ทำอะไร

        // 🆕 เช็คว่ายังมี Aggro Target ที่ยังไม่หมดเวลา และยังไม่ตายอยู่ไหม
        if (aggroTarget != null && aggroTimer > 0f)
        {
            HealthSystem aggroHealth = aggroTarget.GetComponent<HealthSystem>();
            bool aggroAlive = (aggroHealth == null || !aggroHealth.IsDead);

            if (aggroAlive)
            {
                target = aggroTarget;
                aggroTimer -= Time.deltaTime;
            }
            else
            {
                // เป้าหมาย aggro ตายไปแล้ว เลิกยึดติด กลับไปหาใกล้สุดแทน
                aggroTarget = null;
                aggroTimer = 0f;
                target = FindNearestTarget();
            }
        }
        else
        {
            // ไม่มี aggro หรือหมดเวลาแล้ว → หาเป้าหมายใกล้สุดตามปกติ
            target = FindNearestTarget();
        }

        if (target == null) return;

        Vector3 flatEnemyPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 flatTargetPos = new Vector3(target.position.x, 0, target.position.z);
        float dist = Vector3.Distance(flatEnemyPos, flatTargetPos);

        if (dist <= attackRange)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            HandleAttack();
        }
        else if (dist <= detectionRange)
        {
            agent.isStopped = false;
            // 🆕 เพิ่มส่วนนี้ — สุ่มตำแหน่งรอบๆ target แทนจุดเป๊ะเดียวกัน
            Vector3 randomOffset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            Vector3 desiredPos = target.position + randomOffset;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(target.position, out hit, navSampleDistance, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
        else
        {
            agent.isStopped = true;
        }

        if (animator != null)
        {
            float speed = agent.isStopped ? 0f : agent.velocity.magnitude;
            animator.SetFloat("Speed", speed);
        }

        attackTimer -= Time.deltaTime;
    }

    // 🆕 หาเป้าหมายที่ใกล้ที่สุดระหว่าง Player กับ Companion
    Transform FindNearestTarget()
    {
        Transform nearest = null;
        float minDist = detectionRange;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {       // 🆕 เช็คว่า Player ตายแล้วหรือยัง
            HealthSystem playerHealth = playerObj.GetComponent<HealthSystem>();
            bool playerAlive = (playerHealth == null || !playerHealth.IsDead);
            if (playerAlive)
            {
                float dist = Vector3.Distance(transform.position, playerObj.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = playerObj.transform;
                }
            }
        }

        GameObject[] companions = GameObject.FindGameObjectsWithTag("Companion");
        foreach (GameObject comp in companions)
        {
            // 🆕 ข้าม Companion ที่ตายแล้ว (กำลังรอ respawn)
            HealthSystem compHealth = comp.GetComponent<HealthSystem>();
            if (compHealth != null && compHealth.IsDead)
                continue;

            float dist = Vector3.Distance(transform.position, comp.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = comp.transform;
            }
        }

        return nearest;
    }

    void LateUpdate()
    {
        if (attackLocked && animator != null)
        {
            animator.ResetTrigger("Attack");
        }
    }

    void HandleAttack()
    {
        if (attackLocked) return;

        if (attackTimer <= 0f)
        {
            attackTimer = attackCooldown;
            Debug.Log("👹 Enemy เริ่มท่าโจมตี!");
            if (animator != null) animator.SetTrigger("Attack");
        }
    }

    // 🆕 เปลี่ยนชื่อจาก DealDamageToPlayer เป็น DealDamageToTarget (ทำดาเมจให้เป้าหมายปัจจุบัน ไม่ว่าจะเป็นใคร)
    public void DealDamageToTarget()
    {
        if (target == null || isDead || isStunned || attackLocked) return;

        // 🆕 เช็คว่าเป้าหมายตายไปแล้วหรือยัง (กันกรณี Animation Event ยิงมาช้ากว่าจังหวะที่ตาย)
        HealthSystem checkHealth = target.GetComponent<HealthSystem>();
        if (checkHealth != null && checkHealth.IsDead)
        {
            Debug.Log("⚠️ เป้าหมายตายไปแล้วก่อน Animation Event จะยิง ไม่ทำดาเมจ");
            return;
        }
        Vector3 flatEnemyPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 flatTargetPos = new Vector3(target.position.x, 0, target.position.z);
        float dist = Vector3.Distance(flatEnemyPos, flatTargetPos);

        if (dist > attackRange + 0.5f)
        {
            Debug.Log("⚠️ Animation Event ยิงมาแต่เป้าหมายอยู่นอกระยะแล้ว ไม่ทำดาเมจ");
            return;
        }

        // 🆕 เช็คว่าเป้าหมายอยู่ด้านหน้า Enemy ไหม (ไม่ใช่ด้านข้าง/หลัง)
        Vector3 dirToTarget = (flatTargetPos - flatEnemyPos).normalized;
        Vector3 enemyForward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        float angleToTarget = Vector3.Angle(enemyForward, dirToTarget);

        float attackAngle = 180f;   // มุมที่ยอมให้ตีโดน (ปรับได้ตามต้องการ)

        if (angleToTarget > attackAngle)
        {
            Debug.Log($"⚠️ เป้าหมายอยู่นอกมุมโจมตี (angle={angleToTarget}) ไม่ทำดาเมจ");
            return;
        }

        // เช็ค Block เฉพาะกรณีเป้าหมายเป็น Player (Companion ไม่มีระบบ Block)
        PlayerController playerController = target.GetComponent<PlayerController>();
        CompanionAI companionAI = target.GetComponent<CompanionAI>();   // 🆕 เพิ่มบรรทัดนี้
        //if (playerController != null)
        //{
            bool targetIsBlocking = false;// เช็คว่าเป้าหมายกำลัง Block อยู่ไหม (ทั้ง Player และ Companion)
            bool isInFrontOfTarget = false;
            Vector3 dirToEnemy = (flatEnemyPos - flatTargetPos).normalized;
            Vector3 targetForward = new Vector3(target.forward.x, 0, target.forward.z).normalized;
            float angle = Vector3.Angle(targetForward, dirToEnemy);
            isInFrontOfTarget = angle <= 90f;

            

            if (playerController != null && playerController.IsBlocking)
                targetIsBlocking = true;
            else if (companionAI != null && companionAI.IsBlocking)   // 🆕 เพิ่มเงื่อนไขนี้
                targetIsBlocking = true;

            if (targetIsBlocking && isInFrontOfTarget)
            {
                Debug.Log($"🛡️ {target.name} Player Block สำเร็จ! Enemy โดนสะท้อนกลับ");
                if (animator != null)
                    animator.SetTrigger("Hit");
                    PlayBlockClashSound();
                    StunFor(1.5f);
                return;
            }
           
        //}

        else
        {
            Debug.Log($"⚠️ Block ไม่สำเร็จ! targetIsBlocking={targetIsBlocking}, isInFrontOfTarget={isInFrontOfTarget}, angle={angle}");
        }

        HealthSystem targetHealth = target.GetComponent<HealthSystem>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(attackDamage);
            PlayAttackSound();   // 🆕 เพิ่มบรรทัดนี้ — เล่นเสียงตอนตีโดนเป้าหมายจริงเท่านั้น
            Debug.Log($"👹 Enemy ตีโดน {target.name}! ลดเลือดไป {attackDamage}");
        }
    }

    // 🆕 เพิ่มฟังก์ชันนี้ทั้งหมด
    void PlayAttackSound()
    {
        if (attackClips.Length == 0 || attackAudioSource == null) return;
        AudioClip clip = attackClips[Random.Range(0, attackClips.Length)];
        attackAudioSource.PlayOneShot(clip, attackVolume);
    }

    // 🆕 เพิ่มฟังก์ชันนี้ทั้งหมด
    void PlayBlockClashSound()
    {
        if (blockClashClips.Length == 0 || attackAudioSource == null) return;
        AudioClip clip = blockClashClips[Random.Range(0, blockClashClips.Length)];
        attackAudioSource.PlayOneShot(clip, blockClashVolume);
    }

    public void StunFor(float duration)
    {
        isStunned = true;
        stunTimer = duration;
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        if (animator != null)
            animator.ResetTrigger("Attack");

        attackLocked = true;
        Invoke(nameof(UnlockAttack), duration + attackLockDuration);

        attackTimer = attackCooldown;
    }

    // 🆕 เรียกจาก HealthSystem ตอนโดนโจมตี — สลับเป้าหมายไปหาคนที่ตีมา
    public void SetAggroTarget(Transform attacker)
    {
        if (isDead) return;

        aggroTarget = attacker;
        aggroTimer = aggroDuration;
        Debug.Log($"😡 Enemy ถูกยั่วยุ! หันไปหา {attacker.name}");
    }

    // 🆕 เรียกจากภายนอก (เช่น PlayerController ตอนเตะโดน) เพื่อดัน Enemy ถอยหลัง
    public void ApplyKnockback(Vector3 direction, float distance)
    {
        if (isDead) return;
        StartCoroutine(KnockbackRoutine(direction, distance));
    }

    // 🆕 Coroutine ค่อยๆ ดันตำแหน่งออกไปตามทิศทาง (ต้องทำแบบนี้เพราะ NavMeshAgent ใช้ AddForce ไม่ได้)
    System.Collections.IEnumerator KnockbackRoutine(Vector3 direction, float distance)
    {
        if (agent == null) yield break;

        Vector3 flatDir = new Vector3(direction.x, 0, direction.z).normalized;
        Vector3 targetPos = transform.position + flatDir * distance;
        float originalY = transform.position.y;   // 🆕 เก็บความสูงเดิมไว้ก่อนเริ่ม

        // เช็คว่าตำแหน่งปลายทางยังอยู่บน NavMesh ไหม (กันดันทะลุกำแพง/ตกขอบ)
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, distance + 1f, NavMesh.AllAreas))
        {
            targetPos = hit.position;
            targetPos.y += knockbackGroundOffset;   // 🆕 ชดเชย offset
        }

        bool wasStopped = agent.isStopped;
        agent.isStopped = true;

        float duration = 0.2f;   // ระยะเวลาที่ใช้ดันถอย (ปรับได้ ยิ่งน้อยยิ่งกระตุกเร็ว)
        float elapsed = 0f;
        Vector3 startPos = transform.position;

        while (elapsed < duration)
        {
            if (isDead) yield break;   // ถ้าตายกลางคัน หยุดทันที
                                       //transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            float t = elapsed / duration;
            Vector3 lerpPos = Vector3.Lerp(startPos, targetPos, t);

            // 🆕 Sample หาความสูงพื้นจริงตรงจุดนี้ แทนที่จะใช้ค่า Y จาก Lerp ตรงๆ
            NavMeshHit groundHit;
            if (NavMesh.SamplePosition(lerpPos, out groundHit, 5f, NavMesh.AllAreas))
            {
                lerpPos.y = groundHit.position.y + knockbackGroundOffset;  // 🆕 ชดเชย offset ตรงนี้ด้วย
            }
            transform.position = lerpPos;
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;

        if (agent.enabled)
            agent.Warp(targetPos);   // sync ตำแหน่งให้ NavMeshAgent รู้ตัวว่าอยู่ตรงไหนจริง

        agent.isStopped = wasStopped;
    }

    void UnlockAttack()
    {
        attackLocked = false;
    }

    public void SetDead()
    {
        isDead = true;
        isStunned = false;
        attackLocked = false;
        aggroTarget = null;      // 🆕 ล้าง aggro
        aggroTimer = 0f;         // 🆕
        target = null;           // 🆕 ล้างเป้าหมาย

        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.enabled = false;   // 🆕 ปิด NavMeshAgent ไปเลย (กันปัญหาลอย/จมตามที่เคยเจอ)
        }

        if (animator != null)
            animator.ResetTrigger("Attack");

        
        // 🆕 เพิ่มบรรทัดนี้ — เริ่มนับเวลาฟื้นคืนชีพ
        if (autoRespawn) 
        Invoke(nameof(Respawn), respawnDelay);
    }

    // 🆕 เพิ่มฟังก์ชันใหม่นี้ทั้งหมด
    void Respawn()
    {
        Debug.Log("✨ Enemy ฟื้นคืนชีพ!");

        isDead = false;

        transform.position = spawnPosition;
        transform.rotation = spawnRotation;

        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
        }

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        if (health != null)
        {
            health.ResetHealth();
        }

        if (EnemyCounterUI.Instance != null)          // 🆕 เพิ่มส่วนนี้
            EnemyCounterUI.Instance.OnEnemyRespawned();
    }

    // 🆕 เรียกจาก WaveManager ตอนเริ่ม wave ใหม่
    public void ResetForWave()
    {
    CancelInvoke();
    StopAllCoroutines();          // หยุด knockback ที่อาจค้างอยู่
    isStunned = false;
    attackLocked = false;
    aggroTarget = null;
    aggroTimer = 0f;
    attackTimer = 0f;

    Respawn();

    if (agent != null && agent.enabled)
        agent.Warp(spawnPosition);   // บอก NavMeshAgent ว่าอยู่ตำแหน่งใหม่แล้ว
    }

    // 🆕 เพิ่มฟังก์ชันนี้ด้วย(ป้องกัน error ถ้า object ถูกลบระหว่างรอ)
    void OnDestroy()
    {
        CancelInvoke();
    }
}