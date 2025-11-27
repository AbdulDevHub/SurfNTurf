using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class EndScreenScript : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;    // Text field to display score
    [SerializeField] private Button quitButton;            // Quit game button
    [SerializeField] private Button mainMenuButton;        // Return to main menu button

    [Header("Button Sounds")]
    [SerializeField] private AudioClip clickSound;

    private AudioSource audioSource;

    // Call this method to display the final score
    public void ShowScore(float totalTime, int remainingScales, int remainingHealth, int totalScore)
    {
        if (scoreText != null)
            scoreText.text = 
                $"Total Time: {totalTime:F2} s\n" +
                $"Remaining Scales: {remainingScales}\n" +
                $"Remaining Health: {remainingHealth}\n" +
                $"Total Score: {totalScore}";
    }

    private void Start()
    {
        // Add button listeners
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);

        // Add audio source for button clicks
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
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
        SceneManager.LoadScene("MainMenu"); // Make sure your main menu scene is named "MainMenu"
    }
}
