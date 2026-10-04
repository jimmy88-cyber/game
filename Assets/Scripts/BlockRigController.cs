using UnityEngine;
using UnityEngine.Animations.Rigging;

public class BlockRigController : MonoBehaviour
{
    [Header("References")]
    public OverrideTransform blockConstraint;
    public Transform blockTarget;
    public Transform playerModel;

    [Header("Settings")]
    public float smoothSpeed = 10f;

    private float targetWeight = 0f;

    void Update()
    {
        // รับ Input คลิกขวา
        if (Input.GetMouseButtonDown(1))
            targetWeight = 1f;
        if (Input.GetMouseButtonUp(1))
            targetWeight = 0f;

        // Smooth transition
        blockConstraint.weight = Mathf.Lerp(
            blockConstraint.weight, targetWeight, smoothSpeed * Time.deltaTime);

        // ตั้ง BlockTarget ให้หันตรงตามกล้องตลอดเวลา
        if (Camera.main != null)
        {
            float camY = Camera.main.transform.eulerAngles.y;
            blockTarget.rotation = Quaternion.Euler(0, camY, 0);
        }
    }
}