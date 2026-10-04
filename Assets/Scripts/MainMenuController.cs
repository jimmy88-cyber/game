using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names")]
    public string gameSceneName = "SampleScene";

    [Header("Menu Music")]
    public AudioSource menuMusic;
    public float fadeOutTime = 1f;

    [Header("Panels")]                     // 🆕
    public GameObject mainMenuPanel;       // 🆕 ลาก MainMenuPanel ใส่
    public GameObject controlsPanel;       // 🆕 ลาก ControlsPanel ใส่
    //public GameObject controlsPanel;   // 🆕 ลาก ControlsPanel ใส่

    public GameObject exitButton;   // 🆕 ลากปุ่ม EXIT ใส่
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ShowMainMenu();                    // 🆕 เริ่มที่หน้าเมนูหลักเสมอ

        #if     UNITY_WEBGL
        if (exitButton != null) exitButton.SetActive(false);   // 🆕 ซ่อนปุ่ม Exit บนเว็บ
        #endif

    }

    void Update()
    {
        // 🆕 กด Esc ตอนอยู่หน้า Controls เพื่อกลับเมนู
        if (Input.GetKeyDown(KeyCode.Escape) && controlsPanel != null && controlsPanel.activeSelf)
            ShowMainMenu();
    }

    // 🆕 ผูกกับปุ่ม CONTROLS
    public void OpenControls()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(true);
    }

    //public void CloseControls()        // 🆕
    //{
    //    controlsPanel.SetActive(false);
    //}

    // 🆕 ผูกกับปุ่ม BACK ในหน้า Controls
    public void ShowMainMenu()
    {
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void StartGame()
    {
        StartCoroutine(FadeAndLoad());
    }

    System.Collections.IEnumerator FadeAndLoad()
    {
        if (menuMusic != null)
        {
            float startVol = menuMusic.volume;
            float t = 0f;
            while (t < fadeOutTime)
            {
                t += Time.deltaTime;
                menuMusic.volume = Mathf.Lerp(startVol, 0f, t / fadeOutTime);
                yield return null;
            }
        }
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenSettings()
    {
        Debug.Log("เปิดหน้า Settings");
    }

    public void QuitGame()
    {
        Application.Quit();
    }



}