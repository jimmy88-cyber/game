using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI label;
    public Color normalColor = new Color(0.63f, 0.63f, 0.63f);
    public Color hoverColor = Color.white;
    public float hoverScale = 1.08f;
    public float speed = 12f;

    private Vector3 targetScale = Vector3.one;
    private Color targetColor;

    void Start()
    {
        targetColor = normalColor;
        if (label != null) label.color = normalColor;
    }

    void Update()
    {
        // ค่อยๆ เปลี่ยนขนาดและสีให้นุ่มนวล (ใช้ unscaledDeltaTime เผื่อเกมหยุดเวลาไว้)
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, speed * Time.unscaledDeltaTime);
        if (label != null)
            label.color = Color.Lerp(label.color, targetColor, speed * Time.unscaledDeltaTime);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = Vector3.one * hoverScale;
        targetColor = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = Vector3.one;
        targetColor = normalColor;
    }
}