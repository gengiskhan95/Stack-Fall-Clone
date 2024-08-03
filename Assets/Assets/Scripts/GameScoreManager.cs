using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameScoreManager : MonoBehaviour
{
    public static GameScoreManager Instance { get; private set; }

    [Header("UI Components")]
    [Tooltip("Text component to display the current score")]
    [SerializeField] private Text scoreText;

    [Header("Score Settings")]
    [Tooltip("The current score for the level being played")]
    [SerializeField] private int currentScore;

    private int sessionHighScore;
    private int sessionHighestConsecutiveCurrentScore;
    private int highScore;
    private int highestConsecutiveCurrentScore;

    private void Awake()
    {
        InitializeSingleton();
        LoadScores();
    }

    private void OnEnable()
    {
        RegisterSceneLoadedEvent();
    }

    private void OnDisable()
    {
        UnregisterSceneLoadedEvent();
    }

    private void Start()
    {
        InitializeScoreText();
        UpdateScoreUI();
    }

    #region Singleton Methods

    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    #endregion

    #region Scene Management Methods

    private void RegisterSceneLoadedEvent()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void UnregisterSceneLoadedEvent()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HandleSceneLoaded();
    }

    private void HandleSceneLoaded()
    {
        LocateScoreText();
        UpdateScoreUI();
    }

    #endregion

    #region UI Update Methods

    private void InitializeScoreText()
    {
        LocateScoreText();
        UpdateScoreUI();
    }

    private void LocateScoreText()
    {
        scoreText = GameObject.Find("ScoreText")?.GetComponent<Text>();
        if (scoreText == null)
        {
            Debug.LogError("ScoreText component not found in the scene. Please assign it in the Inspector or ensure it is named 'ScoreText'.");
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString();
        }
    }

    #endregion

    #region Score Management Methods

    public void AddScore(int value)
    {
        currentScore += value;
        if (currentScore > sessionHighestConsecutiveCurrentScore)
        {
            sessionHighestConsecutiveCurrentScore = currentScore;
        }
        UpdateScoreUI();
    }

    public void CompleteLevel()
    {
        sessionHighScore += currentScore;
        if (currentScore > highestConsecutiveCurrentScore)
        {
            highestConsecutiveCurrentScore = currentScore;
        }
        highScore += currentScore;
        ResetScore();
    }

    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreUI();
    }

    public void CheckAndUpdateScores()
    {
        if (sessionHighScore > PlayerPrefs.GetInt("HighScore", 0))
        {
            PlayerPrefs.SetInt("HighScore", sessionHighScore);
        }

        if (sessionHighestConsecutiveCurrentScore > PlayerPrefs.GetInt("HighestConsecutiveCurrentScore", 0))
        {
            PlayerPrefs.SetInt("HighestConsecutiveCurrentScore", sessionHighestConsecutiveCurrentScore);
        }
    }

    public void ResetSessionScores()
    {
        sessionHighScore = 0;
        sessionHighestConsecutiveCurrentScore = 0;
    }

    private void LoadScores()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        highestConsecutiveCurrentScore = PlayerPrefs.GetInt("HighestConsecutiveCurrentScore", 0);
    }

    #endregion

    #region Getter Methods

    public int GetCurrentScore()
    {
        return currentScore;
    }

    public int GetHighScore()
    {
        return PlayerPrefs.GetInt("HighScore", 0);
    }

    public int GetSessionHighScore()
    {
        return sessionHighScore;
    }

    public int GetHighestConsecutiveCurrentScore()
    {
        return PlayerPrefs.GetInt("HighestConsecutiveCurrentScore", 0);
    }

    public int GetSessionHighestConsecutiveCurrentScore()
    {
        return sessionHighestConsecutiveCurrentScore;
    }

    #endregion
}
