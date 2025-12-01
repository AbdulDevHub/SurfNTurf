using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour
{
    [Header("Button Sounds")]
    [SerializeField] private AudioClip clickSound;

    private AudioSource uiAudioSource;               // NEW → Dedicated 2D AudioSource for UI clicks

    [Header("Menu Music")]
    [SerializeField] private AudioClip menuMusic;
    private AudioSource musicSource;

    [Header("UI Panels")]
    [SerializeField] private GameObject instructionsPanel;
    [SerializeField] private GameObject storyPanel;   // NEW

    [Header("UI Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button instructionsButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button storyButton;       // NEW
    [SerializeField] private Button storyBackButton;   // NEW

    void Start()
    {
        // -----------------------------------
        // Create a 2D AudioSource for UI clicks
        // -----------------------------------
        uiAudioSource = gameObject.AddComponent<AudioSource>();
        uiAudioSource.spatialBlend = 0f;  // Force 2D sound
        uiAudioSource.playOnAwake = false;

        // -----------------------------------
        // Play menu music
        // -----------------------------------
        if (menuMusic != null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.clip = menuMusic;
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f; // Also 2D
            musicSource.Play();
        }

        // -----------------------------------
        // Button listeners
        // -----------------------------------
        if (playButton != null)
            playButton.onClick.AddListener(PlayGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        if (instructionsButton != null)
            instructionsButton.onClick.AddListener(OpenInstructions);

        if (backButton != null)
            backButton.onClick.AddListener(CloseInstructions);

        if (storyButton != null)
            storyButton.onClick.AddListener(OpenStory);      // NEW

        if (storyBackButton != null)
            storyBackButton.onClick.AddListener(CloseStory); // NEW

        // Ensure panels start hidden
        if (instructionsPanel != null)
            instructionsPanel.SetActive(false);

        if (storyPanel != null)
            storyPanel.SetActive(false);   // NEW
    }

    // -----------------------------------
    // UI Button Sound
    // -----------------------------------
    private void PlayClickSound()
    {
        if (clickSound != null)
            uiAudioSource.PlayOneShot(clickSound); // 2D sound → consistent volume!
    }

    // -----------------------------------
    // Button functions
    // -----------------------------------

    public void PlayGame()
    {
        PlayClickSound();

        if (musicSource != null)
            musicSource.Stop();

        SceneManager.LoadScene("Game");
    }

    public void QuitGame()
    {
        PlayClickSound();
        Application.Quit();
        Debug.Log("Quit Game");
    }

    private void OpenInstructions()
    {
        PlayClickSound();

        if (instructionsPanel != null)
            instructionsPanel.SetActive(true);
    }

    private void CloseInstructions()
    {
        PlayClickSound();

        if (instructionsPanel != null)
            instructionsPanel.SetActive(false);
    }

    // ---------- NEW: STORY PANEL ----------
    private void OpenStory()
    {
        if (clickSound != null)
            AudioSource.PlayClipAtPoint(clickSound, Vector3.zero);

        if (storyPanel != null)
            storyPanel.SetActive(true);
    }

    private void CloseStory()
    {
        if (clickSound != null)
            AudioSource.PlayClipAtPoint(clickSound, Vector3.zero);

        if (storyPanel != null)
            storyPanel.SetActive(false);
    }
}
