using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [System.Serializable]
    public class Sound
    {
        public string soundName;        // Name for referencing
        public AudioClip clip;          // The audio clip
        [Range(0f, 1f)] public float volume = 1f; // Per-sound volume
        public bool is3D;               // Toggle between 2D and 3D sound
    }

    public List<Sound> sounds = new List<Sound>();

    private Dictionary<string, Sound> soundDictionary;
    private AudioSource audioSource2D; // Handles all 2D sounds

    void Awake()
    {
        // Singleton
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        // Create dictionary for fast lookup
        soundDictionary = new Dictionary<string, Sound>();
        foreach (var s in sounds)
        {
            if (!soundDictionary.ContainsKey(s.soundName))
                soundDictionary.Add(s.soundName, s);
        }

        // Create 2D AudioSource
        audioSource2D = gameObject.AddComponent<AudioSource>();
        audioSource2D.spatialBlend = 0f; // 2D
    }

    // -------------------------------------------------------
    // PLAY SOUND
    // -------------------------------------------------------

    public void PlaySound(string name, Vector3 position = default)
    {
        if (!soundDictionary.ContainsKey(name))
        {
            Debug.LogWarning("Sound not found: " + name);
            return;
        }

        Sound s = soundDictionary[name];

        if (s.is3D)
        {
            // 3D sound played at a specific world position
            AudioSource.PlayClipAtPoint(s.clip, position, s.volume);
        }
        else
        {
            // 2D sound played by manager's audio source
            audioSource2D.PlayOneShot(s.clip, s.volume);
        }
    }

    // -------------------------------------------------------
    // VOLUME CONTROL
    // -------------------------------------------------------

    public void SetVolume(string name, float newVolume)
    {
        if (soundDictionary.ContainsKey(name))
        {
            soundDictionary[name].volume = Mathf.Clamp01(newVolume);
        }
    }

    public float GetVolume(string name)
    {
        return soundDictionary.ContainsKey(name)
            ? soundDictionary[name].volume
            : 0f;
    }
}
