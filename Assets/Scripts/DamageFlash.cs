using UnityEngine;
using UnityEngine.UI;
public class DamageFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    public Image damageImage;
    public float flashDuration = 0.3f;
    public float maxAlpha = 0.4f;

    [Header("Vignette Settings")]           // ?? เพิ่มส่วนนี้
    public int textureSize = 512;           // ?? ความละเอียด texture (512 พอสำหรับ UI)
    [Range(0f, 1f)] public float innerRadius = 0.4f;  // ?? รัศมีจุดที่เริ่มโปร่งใส (ตรงกลาง)
    [Range(0f, 1f)] public float outerRadius = 1.0f;  // ?? รัศมีขอบนอกสุดที่สีเข้มที่สุด

    public float deathAlpha = 0.6f;

    private Coroutine flashCoroutine;

    void Awake()   // ?? เพิ่มฟังก์ชันนี้ทั้งหมด — สร้าง vignette sprite อัตโนมัติตอนเริ่มเกม
    {
        if (damageImage != null)
            damageImage.sprite = GenerateVignetteSprite();
    }

    Sprite GenerateVignetteSprite()
    {
        Texture2D tex = new Texture2D(textureSize, textureSize);
        Vector2 center = new Vector2(textureSize / 2f, textureSize / 2f);
        float maxDist = center.magnitude;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center) / maxDist;
                float alpha = Mathf.InverseLerp(innerRadius, outerRadius, dist);
                alpha = Mathf.Clamp01(alpha);
                tex.SetPixel(x, y, new Color(1f, 0f, 0f, alpha));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f));
    }

    public void Flash()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    System.Collections.IEnumerator FlashRoutine()
    {
        if (damageImage == null) yield break;
        Color c = damageImage.color;
        c.a = maxAlpha;
        damageImage.color = c;
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flashDuration;
            c.a = Mathf.Lerp(maxAlpha, 0f, t);
            damageImage.color = c;
            yield return null;
        }
        c.a = 0f;
        damageImage.color = c;
    }

    public void SetDeathRed()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        if (damageImage == null) return;

        Color c = damageImage.color;
        c.a = deathAlpha;
        damageImage.color = c;
    }
}