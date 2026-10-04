using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Settings")]
    public float distance = 6f;
    public float smoothSpeed = 5f;
    public float rotationSpeed = 3f;

    public float currentAngleX = 20f;
    public float currentAngleY = 0f;

    private bool isFirstPerson = false;
    private float tpDistance = 6f;
    private float tpAngleX = 20f;

    

    [Header("First Person Settings")]
    public float fpHeight = 1.6f;
    public float fpForward = 0.3f; // ����͹���ͧ仢�ҧ˹����硹���

    private SkinnedMeshRenderer[] playerMeshes;
    private int skipFrames = 2; //���� input �������á��ѧ�������

    // ?? ������ǹ��������
    [Header("Shake Settings")]
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.3f;

    private float shakeTimer = 0f;
    private Vector3 shakeOffset = Vector3.zero;

    // ���¡�ҡ��¹͡����ⴹ�����
    public void Shake()
    {
        shakeTimer = shakeDuration;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        tpDistance = distance;
        tpAngleX = currentAngleX;

        // �� Mesh �ͧ Player �������Ѻ��͹/�ʴ�
        playerMeshes = target.GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    void Update()
    {
        if (PauseMenu.IsPaused) return;   // 🆕

        if (Input.GetKeyDown(KeyCode.V))
        {
            isFirstPerson = !isFirstPerson;

            if (isFirstPerson)
            {
                currentAngleX = 0f;
                // ��͹����Фõ͹ First Person
                foreach (var mesh in playerMeshes)
                    mesh.enabled = false;
            }
            else
            {
                distance = tpDistance;
                currentAngleX = tpAngleX;
                // �ʴ�����Фõ͹ Third Person
                foreach (var mesh in playerMeshes)
                    mesh.enabled = true;
            }
        }
    }

    void LateUpdate()
    {   if (PauseMenu.IsPaused) return;   // 🆕
    
        if (target == null) return;

        // inputҨⴴԴ㹪ǧáѧ͡
        if (skipFrames > 0)
        {
            skipFrames--;
        }
        else
        {
            currentAngleY += Input.GetAxis("Mouse X") * rotationSpeed;
            currentAngleX -= Input.GetAxis("Mouse Y") * rotationSpeed;
        }

        if (isFirstPerson)
            currentAngleX = Mathf.Clamp(currentAngleX, -60f, 60f);
        else
            currentAngleX = Mathf.Clamp(currentAngleX, 5f, 60f);

        Quaternion rotation = Quaternion.Euler(currentAngleX, currentAngleY, 0);

        // �ӹǳ shake offset
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            shakeOffset = Random.insideUnitSphere * shakeMagnitude * (shakeTimer / shakeDuration);
        }
        else
        {
            shakeOffset = Vector3.zero;
        }

        if (isFirstPerson)
        {
            Vector3 forward = Quaternion.Euler(0, currentAngleY, 0) * Vector3.forward;
            Vector3 fpPos = target.position
                + Vector3.up * fpHeight
                + forward * fpForward;
            transform.position = fpPos + shakeOffset;
            transform.rotation = rotation;
        }
        else
        {
            Vector3 offset = rotation * new Vector3(0, 0, -distance);
            transform.position = Vector3.Lerp(
                transform.position,
                target.position + offset + shakeOffset,
                smoothSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                rotation,
                smoothSpeed * Time.deltaTime);
        }
    }
}