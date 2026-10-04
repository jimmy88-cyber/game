using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("Player HP")]
    public Slider playerHPBar;
    public TextMeshProUGUI playerHPText;

    private HealthSystem playerHealth;

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<HealthSystem>();
            if (playerHPBar != null)
                playerHPBar.maxValue = playerHealth.maxHealth;
        }
    }

    void Update()
    {
        if (playerHealth == null) return;

        if (playerHPBar != null)
            playerHPBar.value = playerHealth.currentHealth;

        if (playerHPText != null)
            playerHPText.text = $"HP: {playerHealth.currentHealth}/{playerHealth.maxHealth}";
    }
}