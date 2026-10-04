using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [Header("References")]
    public GameObject pausePanel;
    public GameObject controlsPanel;    // ลาก ControlsPanel (ในหน้า Pause) ใส่
    public TMP_Text waveText;           // ลาก Text "WAVE" ในหน้า Pause ใส่
    public string menuSceneName = "MenuScene";

    void Start()
    {
        IsPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);

        // กันข้อความ "WAVE 10" ถูกตัดขึ้นบรรทัดใหม่เมื่อกล่อง Text แคบ
        if (waveText != null) waveText.textWrappingMode = TextWrappingModes.NoWrap;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            // ถ้าอยู่หน้า Controls ให้ย้อนกลับไปหน้า Pause ก่อน
            if (controlsPanel != null && controlsPanel.activeSelf) CloseControls();
            else if (IsPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        IsPaused = true;
        pausePanel.SetActive(true);
        if (waveText != null && WaveManager.Instance != null)
            waveText.text = "WAVE " + WaveManager.Instance.CurrentWave;
        Time.timeScale = 0f;            // หยุดทุกอย่างที่ใช้เวลา (Enemy, animation, physics)
        AudioListener.pause = true;     // หยุดเสียงทั้งหมด
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        pausePanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        StartCoroutine(UnpauseNextFrame());
    }

    // ปลดสถานะ pause ในเฟรมถัดไป กันการคลิกปุ่ม RESUME ไปกลายเป็นการฟันดาบ
    IEnumerator UnpauseNextFrame()
    {
        yield return null;
        IsPaused = false;
    }

    // ผูกกับปุ่ม CONTROLS ในหน้า Pause
    public void OpenControls()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(true);
    }

    // ผูกกับปุ่ม BACK ในหน้า Controls
    public void CloseControls()
    {
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;            // สำคัญ! ไม่งั้นเกมจะค้างตอนกลับมาเล่นใหม่
        AudioListener.pause = false;
        IsPaused = false;
        SceneManager.LoadScene(menuSceneName);
    }
}