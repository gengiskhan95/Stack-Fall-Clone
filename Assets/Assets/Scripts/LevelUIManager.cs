using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelUIManager : MonoBehaviour
{
    [Header("UI Elements - Level Progress")]
    [SerializeField] private Image levelProgressImage;
    [SerializeField] private Image currentLevelIndicatorImage;
    [SerializeField] private Text currentLevelNumberText;
    [SerializeField] private Image nextLevelIndicatorImage;
    [SerializeField] private Text nextLevelNumberText;

    [Header("UI Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject soundSettingsPanel;
    [SerializeField] private GameObject deletePrefsPanel;

    [Header("Sound Control Buttons")]
    [SerializeField] private Button enableSoundButton;
    [SerializeField] private Button disableSoundButton;

    [Header("UI Screens")]
    [SerializeField] private GameObject homeScreenUI;
    [SerializeField] private GameObject gameScreenUI;

    [Header("Action Buttons")]
    [Tooltip("Button to delete references and restart game.")]
    [SerializeField] private Button deleteReferencesButton;

    [Header("Text Elements")]
    [Tooltip("Text component for level completion message.")]
    [SerializeField] private Text levelCompletedText;
    [SerializeField] private Text gameOverText;

    private PlayerController playerController;
    private Material playerMaterial;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        UpdateUIColors();
    }

    private void Update()
    {
        HandleUserInput();
        UpdateSoundButtons();
    }

    #region Initialization Methods

    private void InitializeComponents()
    {
        InitializePlayerController();
        InitializePlayerMaterial();
        InitializeSoundControlButtons();
        InitializeActionButtons();
    }

    private void InitializePlayerController()
    {
        playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerController not found in the scene.");
        }
    }

    private void InitializePlayerMaterial()
    {
        if (playerController != null)
        {
            var playerMeshRenderer = playerController.transform.GetChild(0).GetComponent<MeshRenderer>();
            if (playerMeshRenderer != null)
            {
                playerMaterial = playerMeshRenderer.material;
            }
            else
            {
                Debug.LogError("Player's MeshRenderer component is missing.");
            }
        }
    }

    private void InitializeSoundControlButtons()
    {
        if (enableSoundButton != null)
        {
            enableSoundButton.onClick.AddListener(ToggleSound);
        }
        else
        {
            Debug.LogError("Enable Sound Button is not assigned.");
        }

        if (disableSoundButton != null)
        {
            disableSoundButton.onClick.AddListener(ToggleSound);
        }
        else
        {
            Debug.LogError("Disable Sound Button is not assigned.");
        }
    }

    private void InitializeActionButtons()
    {
        if (deleteReferencesButton != null)
        {
            deleteReferencesButton.onClick.AddListener(OnDeleteReferencesButtonClicked);
        }
        else
        {
            Debug.LogError("Delete References Button is not assigned.");
        }
    }

    #endregion

    #region UI Update Methods

    private void ToggleSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ToggleSound();
        }
        else
        {
            Debug.LogError("SoundManager instance is not found.");
        }
    }

    private void UpdateUIColors()
    {
        if (playerMaterial == null)
        {
            Debug.LogError("Player material is not initialized.");
            return;
        }

        var adjustedColor = playerMaterial.color + Color.gray;
        UpdateElementColor(levelProgressImage.transform.parent.GetComponent<Image>(), adjustedColor);
        UpdateElementColor(levelProgressImage, playerMaterial.color);
        UpdateElementColor(currentLevelIndicatorImage, playerMaterial.color);
        UpdateElementColor(nextLevelIndicatorImage, playerMaterial.color);
        UpdateElementColor(levelCompletedText, playerMaterial.color);
    }

    private void UpdateElementColor(Graphic uiElement, Color color)
    {
        if (uiElement != null)
        {
            uiElement.color = color;
        }
        else
        {
            Debug.LogError($"{uiElement} is missing.");
        }
    }

    public void SetLevelProgressImageFill(float fillAmount)
    {
        if (levelProgressImage != null)
        {
            levelProgressImage.fillAmount = fillAmount;
        }
        else
        {
            Debug.LogError("Level Progress Image component is missing.");
        }
    }

    public void ToggleSoundSettingsPanel()
    {
        if (soundSettingsPanel != null)
        {
            soundSettingsPanel.SetActive(!soundSettingsPanel.activeSelf);
        }
        else
        {
            Debug.LogError("Sound Settings Panel is not assigned.");
        }

        if (deletePrefsPanel != null)
        {
            deletePrefsPanel.SetActive(!deletePrefsPanel.activeSelf);
        }
        else
        {
            Debug.LogError("Delete Prefs Panel is not assigned.");
        }
    }

    private void UpdateSoundButtons()
    {
        if (SoundManager.Instance != null)
        {
            bool soundEnabled = SoundManager.Instance.soundEnabled;
            enableSoundButton.gameObject.SetActive(!soundEnabled);
            disableSoundButton.gameObject.SetActive(soundEnabled);
        }
        else
        {
            Debug.LogError("SoundManager instance is not found.");
        }
    }

    #endregion

    #region Input Handling Methods

    private bool IsPointerOverUI()
    {
        var pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);
        raycastResults.RemoveAll(result => result.gameObject.GetComponent<IgnoreGameUI>() != null);

        return raycastResults.Count > 0;
    }

    private void HandleUserInput()
    {
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI() && playerController?.playerState == PlayerController.PlayerState.Ready)
        {
            playerController.playerState = PlayerController.PlayerState.Playing;
            homeScreenUI.SetActive(false);
            gameScreenUI.SetActive(true);
        }
    }

    #endregion

    #region Custom Methods

    private void OnDeleteReferencesButtonClicked()
    {
        DeleteAllReferences();
        RestartGame();
    }

    private void DeleteAllReferences()
    {
        PlayerPrefs.DeleteAll();
        GameScoreManager.Instance.ResetScore();
        Debug.Log("All PlayerPrefs references deleted.");
    }

    private void RestartGame()
    {
        PlayerPrefs.SetInt("Level", 1);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void DisplayGameOverText()
    {
        int currentScore = GameScoreManager.Instance.GetCurrentScore();
        int highScore = GameScoreManager.Instance.GetHighScore();
        int highestConsecutiveCurrentScore = GameScoreManager.Instance.GetHighestConsecutiveCurrentScore();
        int sessionHighScore = GameScoreManager.Instance.GetSessionHighScore();
        int sessionHighestConsecutiveCurrentScore = GameScoreManager.Instance.GetSessionHighestConsecutiveCurrentScore();

        gameOverText.text = $"Game Over!\n" +
                            $"Current Score: {currentScore}\n" +
                            $"High Score: {highScore}\n" +
                            $"Session High Score: {sessionHighScore}\n" +
                            $"Highest Consecutive Score: {highestConsecutiveCurrentScore}\n" +
                            $"Session Highest Consecutive Score: {sessionHighestConsecutiveCurrentScore}";
    }

    public void CompleteLevel()
    {
        GameScoreManager.Instance.CompleteLevel();
        DisplayGameOverText();
    }

    public void FailLevel()
    {
        GameScoreManager.Instance.ResetScore();
        DisplayGameOverText();
    }

    #endregion
}
