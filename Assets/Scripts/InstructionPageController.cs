using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class InstructionPageController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject instructionPage;      
    public Button gearButton;               
    public Button backButton;               
    public Button cheatButton;              // Cheat toggle (formerly quit)
    public Button mainMenuButton;           // Load MainMenu scene

    [Header("Animation Settings")]
    public CanvasGroup canvasGroup;         // For fade animation
    public float fadeDuration = 0.25f;

    private bool isOpen = false;
    private bool isCheatActive = false;

    private void Start()
    {
        // Ensure the panel starts hidden
        if (instructionPage != null)
            instructionPage.SetActive(false);

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        // Listeners + sound
        if (gearButton != null)
            gearButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySound("Button Click");
                ShowInstructionPage();
            });

        if (backButton != null)
            backButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySound("Button Click");
                HideInstructionPage();
            });

        if (cheatButton != null)
        {
            cheatButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySound("Button Click");
                ToggleCheat();
            });
            
            // Update button appearance initially
            UpdateCheatButtonAppearance();
        }

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySound("Button Click");
                LoadMainMenu();
            });
    }

    private void Update()
    {
        // ESC Key → Toggle Instruction Page (No Sound)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isOpen)
                ShowInstructionPage();
            else
                HideInstructionPage();
        }
    }

    // ----------------------------
    // CHEAT TOGGLE
    // ----------------------------
    private void ToggleCheat()
    {
        isCheatActive = !isCheatActive;
        
        // Update PlayerHealth to use cheat mode
        PlayerHealth player = Object.FindFirstObjectByType<PlayerHealth>();
        if (player != null)
            player.SetInvincibility(isCheatActive);
        
        UpdateCheatButtonAppearance();
        
        Debug.Log($"Cheat Mode: {(isCheatActive ? "ACTIVE" : "INACTIVE")}");
    }

    private void UpdateCheatButtonAppearance()
    {
        if (cheatButton == null) return;

        // Update button text if it has a TMP_Text component
        TMP_Text buttonText = cheatButton.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
            buttonText.text = isCheatActive ? "Cheat: ON" : "Cheat: OFF";
    }

    // ----------------------------
    // SHOW PAGE + PAUSE GAME
    // ----------------------------
    public void ShowInstructionPage()
    {
        if (isOpen) return;

        instructionPage.SetActive(true);
        StartCoroutine(FadeCanvas(0f, 1f));

        Time.timeScale = 0f;   // Pause game
        isOpen = true;
    }

    // ----------------------------
    // HIDE PAGE + RESUME GAME
    // ----------------------------
    public void HideInstructionPage()
    {
        if (!isOpen) return;

        StartCoroutine(FadeCanvas(1f, 0f, () =>
        {
            instructionPage.SetActive(false);
        }));

        Time.timeScale = 1f;   // Resume game
        isOpen = false;
    }

    // ----------------------------
    // LOAD MAIN MENU
    // ----------------------------
    private void LoadMainMenu()
    {
        Time.timeScale = 1f;  // Ensure game is unpaused
        
        // Reset cheat when returning to main menu
        isCheatActive = false;
        PlayerHealth player = Object.FindFirstObjectByType<PlayerHealth>();
        if (player != null)
            player.SetInvincibility(false);
        
        SceneManager.LoadScene("MainMenu");
    }

    // ----------------------------
    // FADE ANIMATION
    // ----------------------------
    private IEnumerator FadeCanvas(float start, float end, System.Action onComplete = null)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime; // Very important because game is paused!
            float t = elapsed / fadeDuration;

            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(start, end, t);

            yield return null;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = end;

        onComplete?.Invoke();
    }
}