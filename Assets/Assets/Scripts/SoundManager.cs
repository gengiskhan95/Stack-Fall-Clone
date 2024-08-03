using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Sound Settings")]
    [Tooltip("Enable or disable all sounds")]
    [SerializeField] public bool soundEnabled = true;

    [Header("Audio Source")]
    [Tooltip("AudioSource component to play sound effects")]
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        EnsureSingleton();
        InitializeAudioSource();
    }

    #region Initialization Methods
    private void EnsureSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            Debug.LogWarning("Duplicate SoundManager instance found. Destroying the new one.");
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("SoundManager instance set and marked to not be destroyed on load.");
        }
    }

    private void InitializeAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("AudioSource component is not found on the GameObject.");
            }
            else
            {
                Debug.Log("AudioSource component is successfully assigned.");
            }
        }
    }
    #endregion

    #region Public Methods
    public void ToggleSound()
    {
        soundEnabled = !soundEnabled;
        Debug.Log($"Sound enabled set to: {soundEnabled}");
    }

    public void PlaySoundFX(AudioClip clip, float volume)
    {
        if (!soundEnabled)
        {
            Debug.Log("Sound is disabled. No sound will be played.");
            return;
        }

        if (clip == null)
        {
            Debug.LogWarning("Attempted to play a sound, but the AudioClip was null.");
            return;
        }

        audioSource.PlayOneShot(clip, volume);
        Debug.Log($"Playing sound: {clip.name} at volume: {volume}");
    }
    #endregion
}
