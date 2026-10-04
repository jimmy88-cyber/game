using UnityEngine;
using UnityEngine.AI;

public class CompanionAI : MonoBehaviour
{
    [Header("Combat Settings")]
    public float detectionRange = 15f;
    public float attackRange = 1.5f;
    public int attackDamage = 5;
    public float attackCooldown = 1.5f;

    [Header("Block Settings")]
    public float blockChance = 0.3f;      // ?? โอกาส Block เมื่ออยู่ใกล้ Enemy (0-1)
    public float blockDuration = 10.5f;    // ?? ระยะเวลา Block แต่ละครั้ง         1.5
    public float blockCheckInterval = 2f; // ?? ทุกๆ กี่วิ ถึงจะสุ่มเช็คว่าจะ Block ไหม

    private NavMeshAgent agent;
    private Animator animator;
    private HealthSystem health;
    private Transform currentTarget;
    private float attackTimer = 0f;
    private bool isDead = false;

    private bool isBlocking = false;         // ??
    public bool IsBlocking => isBlocking;    // ?? ให้ EnemyAI เช็คได้
    private float blockTimer = 0f;           // ??
    private float blockCheckTimer = 0f;      // ??

    [Header("Respawn Settings")]
    public float respawnDelay = 10f;   // ?? เวลารอฟื้นคืนชีพ (วินาที)

    private Vector3 spawnPosition;      // ?? ตำแหน่งเริ่มต้น
    private Quaternion spawnRotation;   // ?? มุมหันหน้าเริ่มต้น

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        health = GetComponent<HealthSystem>();

        // ?? เก็บตำแหน่งเริ่มต้นไว้สำหรับ respawn
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
    }

    void Update()
    {
        if (isDead) return;

        // ?? จัดการสถานะ Block ก่อน
        HandleBlockLogic();

        // ?? หมุนตัวเข้าหาเป้าหมายเสมอ ถ้ามีเป้าหมายอยู่ (ไม่ว่าจะกำลัง Block, โจมตี หรือเดินเข้าใกล้)
        if (currentTarget != null)
        {
            Vector3 dirToTarget = (currentTarget.position - transform.position);
            dirToTarget.y = 0;
            if (dirToTarget.magnitude > 0.1f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dirToTarget.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 20f * Time.deltaTime);
            }
        }


        if (isBlocking) return;   // ?? ถ้ากำลัง Block อยู่ ไม่ทำอย่างอื่น (ไม่เดิน ไม่โจมตี)

        currentTarget = FindNearestEnemy();

        if (currentTarget != null)
        {
            float distToEnemy = Vector3.Distance(transform.position, currentTarget.position);

            if (distToEnemy <= attackRange)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                HandleAttack();
            }
            else
            {
                agent.isStopped = false;
                agent.SetDestination(currentTarget.position);
            }
        }
        else
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        attackTimer -= Time.deltaTime;

        if (animator != null)
        {
            float speed = agent.isStopped ? 0f : agent.velocity.magnitude;
            animator.SetFloat("Speed", speed);
        }
    }

    // ?? สุ่มเข้า/ออกสถานะ Block เป็นระยะ ตอนอยู่ใกล้ Enemy
    void HandleBlockLogic()
    {
        if (isBlocking)
        {
            blockTimer -= Time.deltaTime;
            if (blockTimer <= 0f)
            {
                isBlocking = false;
                if (animator != null)
                    animator.SetBool("Block", false);
            }
            return;
        }

        blockCheckTimer -= Time.deltaTime;
        if (blockCheckTimer <= 0f)
        {
            blockCheckTimer = blockCheckInterval;

            // เช็คว่ามี Enemy อยู่ใกล้ในระยะโจมตีไหม ถึงจะเริ่มสุ่ม Block
            if (currentTarget != null)
            {
                float dist = Vector3.Distance(transform.position, currentTarget.position);
                if (dist <= attackRange + 1f)
                {
                    float roll = Random.value;
                    if (roll <= blockChance)
                    {
                        isBlocking = true;
                        blockTimer = blockDuration;
                        agent.isStopped = true;
                        agent.velocity = Vector3.zero;
                        if (animator != null)
                            animator.SetBool("Block", true);
                        Debug.Log("??? Companion เข้า Block!");
                    }
                }
            }
        }
    }

    // ?? ให้ EnemyAI เรียกยกเลิก Block ได้ (เหมือนกับ Player)
    public void ForceStopBlocking()
    {
        isBlocking = false;
        if (animator != null)
            animator.SetBool("Block", false);
    }

    Transform FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearest = null;
        float minDist = detectionRange;

        foreach (GameObject enemy in enemies)
        {

            HealthSystem enemyHealth = enemy.GetComponent<HealthSystem>();
            if (enemyHealth != null && enemyHealth.IsDead) 
            {
                continue;
            }
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = enemy.transform;
            }
        }
        return nearest;
    }

    void HandleAttack()
    {
        if (attackTimer <= 0f && currentTarget != null)
        {
            attackTimer = attackCooldown;
            if (animator != null)
                animator.SetTrigger("Attack");
            Debug.Log("??? Companion เริ่มโจมตี!");
        }
    }

    public void DealDamage()
    {
        if (currentTarget == null)
        {
            Debug.Log("currentTarget เป็น null ตอนพยายามทำดาเมจ!");
            return;
        }
        HealthSystem enemyHealth = currentTarget.GetComponentInParent<HealthSystem>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(attackDamage, transform);
            Debug.Log($" Companion โจมตี Enemy สำเร็จ! ลดเลือดไป {attackDamage}");
        }
        else
        {
            Debug.Log("เจอ currentTarget แต่ไม่มี HealthSystem companent!");
        }
    }

    public void SetDead()
    {
        isDead = true;
        isBlocking = false;
        currentTarget = null;
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
        // ?? เพิ่มบรรทัดนี้ — เริ่มนับเวลาฟื้นคืนชีพ
        Invoke(nameof(Respawn), respawnDelay);
    }

    // ?? เพิ่มฟังก์ชันใหม่นี้ทั้งหมด (วางต่อจาก SetDead())
    void Respawn()
    {
        Debug.Log("? Companion ฟื้นคืนชีพ!");

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
    }

    // ?? เพิ่มฟังก์ชันนี้ด้วย (ป้องกัน error ถ้า object ถูกลบระหว่างรอ)
    void OnDestroy()
    {
        CancelInvoke();
    }

}