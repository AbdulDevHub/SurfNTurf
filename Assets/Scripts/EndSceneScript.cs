using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class EndScreenScript : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI totalTime;       // Text field to display total time
    [SerializeField] private TextMeshProUGUI remainingScales; // Text field to display remaining scales
    [SerializeField] private TextMeshProUGUI remainingHealth; // Text field to display remaining health
    [SerializeField] private TextMeshProUGUI scoreText;       // Text field to display score
    [SerializeField] private Button quitButton;               // Quit game button
    [SerializeField] private Button mainMenuButton;           // Return to main menu button
    [SerializeField] private Button playAgainButton;

    [Header("Outcome UI")]
    [SerializeField] private TextMeshProUGUI outcomeText;
    [SerializeField] private Image outcomeImage;
    [SerializeField] private Sprite bearHappy;
    [SerializeField] private Sprite bearSad;

    [Header("Button Sounds")]
    [SerializeField] private AudioClip clickSound;

    private AudioSource audioSource;

    // Call this method to display the final score
    public void ShowScore(float totalTime, int remainingScales, int remainingHealth, int totalScore)
    {
        if (this.totalTime != null) {
            int minutes = Mathf.FloorToInt(totalTime / 60f);
            int seconds = Mathf.FloorToInt(totalTime % 60f);
            this.totalTime.text = $"Total Time: {minutes:00}:{seconds:00}";
        }
        if (this.remainingScales != null)
            this.remainingScales.text = $"Remaining Scales: {remainingScales}";
        if (this.remainingHealth != null)
            this.remainingHealth.text = $"Remaining Health: {remainingHealth}";
        if (scoreText != null)
            scoreText.text = $"Total Score: {totalScore}";
    }

    private void Start()
    {
        // Buttons
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        if (playAgainButton != null) playAgainButton.onClick.AddListener(PlayAgain);

        audioSource = gameObject.AddComponent<AudioSource>();

        UpdateOutcome();
    }

    private void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
            audioSource.PlayOneShot(clickSound);
    }

    private void QuitGame()
    {
        PlayClickSound();
        Application.Quit();
        Debug.Log("Quit Game");
    }

    private void ReturnToMainMenu()
    {
        PlayClickSound();
        SceneManager.LoadScene("MainMenu");
    }

    private void PlayAgain()
    {
        PlayClickSound();
        SceneManager.LoadScene("Game");
    }

    private void UpdateOutcome()
    {
        if (StatManager.Instance == null)
            return;

        bool playerWon = StatManager.Instance.remainingHealth > 0;

        if (outcomeText != null)
        {
            if (playerWon)
            {
                outcomeText.text = "You Won!";
                outcomeText.color = new Color32(0, 200, 0, 255); // Victory green
            }
            else
            {
                outcomeText.text = "You Failed!";
                outcomeText.color = new Color32(255, 0, 53, 255); // FF0035
            }
        }

        if (outcomeImage != null)
            outcomeImage.sprite = playerWon ? bearHappy : bearSad;
    }
}
