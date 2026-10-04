using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DeathScreenUI : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup canvasGroup;
    public float fadeInDuration = 2f;
    public float delayBeforeFade = 0.5f;   // รอสักครู่ก่อนเริ่ม fade (ให้ animation ตายเล่นก่อน)

    [Header("Click To Menu")]
    public TMP_Text menuPromptText;        // ลาก Text "CLICK TO MAIN MENU" ใส่
    public string menuSceneName = "MenuScene";
    public float blinkSpeed = 1.5f;        // ยิ่งน้อยยิ่งกระพริบช้า
    public float minAlpha = 0.15f;         // ความจางสุดตอนกระพริบ

    private bool canGoToMenu = false;

    void Awake()
    {
        gameObject.SetActive(false);
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    public void ShowDeathScreen()
    {
        gameObject.SetActive(true);
        canGoToMenu = false;
        if (menuPromptText != null) menuPromptText.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(FadeInRoutine());
    }

    System.Collections.IEnumerator FadeInRoutine()
    {
        yield return new WaitForSeconds(delayBeforeFade);

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // fade เสร็จแล้วค่อยโชว์ปุ่ม และเริ่มรับการกด
        if (menuPromptText != null) menuPromptText.gameObject.SetActive(true);
        canGoToMenu = true;
    }

    void Update()
    {
        if (!canGoToMenu) return;

        // กระพริบช้าๆ ด้วยการปรับความโปร่งใส
        if (menuPromptText != null)
        {
            float t = (Mathf.Sin(Time.unscaledTime * blinkSpeed * Mathf.PI) + 1f) * 0.5f;
            Color c = menuPromptText.color;
            c.a = Mathf.Lerp(minAlpha, 1f, t);
            menuPromptText.color = c;
        }

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            GoToMenu();
    }

    // ผูกกับ OnClick ของปุ่มได้ด้วย (ถ้าทำเป็น Button)
    public void GoToMenu()
    {
        if (!canGoToMenu) return;
        canGoToMenu = false;

        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(menuSceneName);
    }
}
