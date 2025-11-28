using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class InstructionPageController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject instructionPage;      
    public Button gearButton;               
    public Button backButton;               
    public Button quitButton;               // Quit game
    public Button mainMenuButton;           // Load MainMenu scene

    [Header("Animation Settings")]
    public CanvasGroup canvasGroup;         // For fade animation
    public float fadeDuration = 0.25f;

    private bool isOpen = false;

    private void Start()
    {
        // Ensure the panel starts hidden
        if (instructionPage != null)
            instructionPage.SetActive(false);

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        // Listeners
        if (gearButton != null)
            gearButton.onClick.AddListener(ShowInstructionPage);

        if (backButton != null)
            backButton.onClick.AddListener(HideInstructionPage);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(LoadMainMenu);
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
    // QUIT GAME
    // ----------------------------
    private void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    // ----------------------------
    // LOAD MAIN MENU
    // ----------------------------
    private void LoadMainMenu()
    {
        Time.timeScale = 1f;  // Ensure game is unpaused
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
