using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour
{
    [Header("Button Sounds")]
    [SerializeField] private AudioClip clickSound;   // For Play, Quit, Instructions, Back buttons

    [Header("Menu Music")]
    [SerializeField] private AudioClip menuMusic;
    private AudioSource musicSource;

    [Header("UI Panels")]
    [SerializeField] private GameObject instructionsPanel;  // Assign the panel in Inspector

    [Header("UI Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button instructionsButton;
    [SerializeField] private Button backButton;  // Back button inside the instructions panel

    void Start()
    {
        // Play menu music
        if (menuMusic != null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.clip = menuMusic;
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.Play();
        }

        // Add button listeners
        if (playButton != null)
            playButton.onClick.AddListener(PlayGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        if (instructionsButton != null)
            instructionsButton.onClick.AddListener(OpenInstructions);

        if (backButton != null)
            backButton.onClick.AddListener(CloseInstructions);

        // Make sure instructions panel starts closed
        if (instructionsPanel != null)
            instructionsPanel.SetActive(false);
    }

    public void PlayGame()
    {
        if (clickSound != null)
            AudioSource.PlayClipAtPoint(clickSound, Vector3.zero);

        if (musicSource != null)
            musicSource.Stop();

        SceneManager.LoadScene("Game");
    }

    public void QuitGame()
    {
        if (clickSound != null)
            AudioSource.PlayClipAtPoint(clickSound, Vector3.zero);

        Application.Quit();
        Debug.Log("Quit Game");
    }

    private void OpenInstructions()
    {
        if (clickSound != null)
            AudioSource.PlayClipAtPoint(clickSound, Vector3.zero);

        if (instructionsPanel != null)
            instructionsPanel.SetActive(true);
    }

    private void CloseInstructions()
    {
        if (clickSound != null)
            AudioSource.PlayClipAtPoint(clickSound, Vector3.zero);

        if (instructionsPanel != null)
            instructionsPanel.SetActive(false);
    }
}
