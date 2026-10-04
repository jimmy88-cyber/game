using UnityEngine;

public class EnemyAttackRelay : MonoBehaviour
{
    private EnemyAI enemyAI;

    void Start()
    {
        enemyAI = GetComponentInParent<EnemyAI>();
    }

    // ?? เปลี่ยนชื่อฟังก์ชันให้ตรงกัน
    public void DealDamageToTarget()
    {
        if (enemyAI != null)
            enemyAI.DealDamageToTarget();
    }
}