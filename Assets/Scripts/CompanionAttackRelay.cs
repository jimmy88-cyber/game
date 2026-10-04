using UnityEngine;

public class CompanionAttackRelay : MonoBehaviour
{
    private CompanionAI companionAI;

    void Start()
    {
        companionAI = GetComponentInParent<CompanionAI>();
    }

    public void DealDamage()
    {
        if (companionAI != null)
            companionAI.DealDamage();
        else Debug.Log(" companionAI เป็น null!");
    }
}