using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Player Components")]
    [SerializeField] private Rigidbody playerRigidbody;

    [Header("Invincibility Settings")]
    [SerializeField] private GameObject invincibilityShieldObject;
    [Tooltip("Duration of the invincibility effect in seconds.")]
    [SerializeField] private float invincibilityDuration = 1.0f;
    [Tooltip("Rate at which the invincibility shield timer decreases.")]
    [SerializeField] private float shieldDeactivationRate = 0.35f;

    [Header("Movement Settings")]
    [Tooltip("Speed at which the player falls.")]
    [SerializeField] private float fallSpeed = -700f;
    [Tooltip("Speed at which the player rises.")]
    [SerializeField] private float riseSpeed = 250f;
    [Tooltip("Rate at which the fall timer increments when falling.")]
    [SerializeField] private float fallTimerIncrementRate = 0.8f;
    [Tooltip("Rate at which the fall timer decrements when not falling.")]
    [SerializeField] private float fallTimerDecrementRate = 0.5f;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip winAudioClip;
    [SerializeField] private AudioClip deathAudioClip;
    [SerializeField] private AudioClip invincibleDestroyAudioClip;
    [SerializeField] private AudioClip destroyAudioClip;
    [SerializeField] private AudioClip bounceAudioClip;

    [Header("UI Elements")]
    [SerializeField] private Image invincibilityDurationSlider;
    [SerializeField] private GameObject invincibilityUIObject;

    [Header("Game UI Elements")]
    [Tooltip("UI element shown when the game is over.")]
    [SerializeField] private GameObject gameOverUIObject;
    [Tooltip("UI element shown when the level is finished.")]
    [SerializeField] private GameObject levelCompleteUIObject;

    [Header("Power UI Elements")]
    [Tooltip("UI element shown when power is ON.")]
    [SerializeField] private GameObject powerOnUIObject;
    [Tooltip("UI element shown when power is OFF.")]
    [SerializeField] private GameObject powerOffUIObject;

    [Header("Level Manager")]
    [SerializeField] private LevelUIManager levelUIManager;

    private bool isFalling;
    private bool isInvincible;
    private float invincibilityTimer;
    private bool hasTouchedPassable;
    private float failCollisionDuration;
    private bool hasFailedCollision;

    private int currentObstacleCount;
    private int totalObstacleCount;

    public enum PlayerState
    {
        Ready,
        Playing,
        Dead,
        Finished
    }

    [Header("Player State")]
    [Tooltip("Current state of the player.")]
    [SerializeField] public PlayerState playerState = PlayerState.Ready;

    private void Awake()
    {
        InitializePlayer();
    }

    private void Start()
    {
        InitializeGame();
    }

    private void Update()
    {
        ProcessPlayerState();
        CheckEndGameConditions();
    }

    private void FixedUpdate()
    {
        ProcessFixedUpdate();
    }

    #region Initialization Methods

    private void InitializePlayer()
    {
        InitializePlayerComponents();
        ResetPlayerStatus();
    }

    private void InitializePlayerComponents()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        invincibilityShieldObject = transform.Find("InvincibleShield")?.gameObject;

        if (invincibilityShieldObject == null)
        {
            Debug.LogError("Invincibility Shield is not assigned in the Inspector.");
        }
    }

    private void InitializeGame()
    {
        totalObstacleCount = FindObjectsOfType<ObstacleManager>().Length;
        UpdateLevelProgress();
    }

    private void ResetPlayerStatus()
    {
        isFalling = false;
        isInvincible = false;
        invincibilityTimer = 0;
        hasTouchedPassable = false;
        failCollisionDuration = 0;
        hasFailedCollision = false;
    }

    #endregion

    #region State Handling Methods

    private void ProcessPlayerState()
    {
        switch (playerState)
        {
            case PlayerState.Ready:
                HandleReadyState();
                break;
            case PlayerState.Playing:
                HandlePlayingState();
                break;
            case PlayerState.Finished:
                HandleFinishedState();
                break;
            case PlayerState.Dead:
                HandleDeadState();
                break;
        }
    }

    private void HandleReadyState()
    {
        // Handle actions for Ready state if needed
    }

    private void HandlePlayingState()
    {
        HandlePlayerInput();
        UpdateInvincibilityStatus();
        UpdateInvincibilityShield();
    }

    private void HandleFinishedState()
    {
        invincibilityUIObject.SetActive(false);
        if (Input.GetMouseButtonDown(0))
        {
            LoadNextLevel();
        }
    }

    private void HandleDeadState()
    {
        invincibilityUIObject.SetActive(false);
        if (Input.GetMouseButtonDown(0))
        {
            RestartGame();
        }
    }

    #endregion

    #region Player Input Methods

    private void HandlePlayerInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartFalling();
        }
        if (Input.GetMouseButtonUp(0))
        {
            ResetPlayerMovement();
        }
    }

    private void StartFalling()
    {
        isFalling = true;
    }

    private void ResetPlayerMovement()
    {
        isFalling = false;
        hasTouchedPassable = false;
        failCollisionDuration = 0;
        hasFailedCollision = false;
    }

    #endregion

    #region Invincibility Methods

    private void UpdateInvincibilityStatus()
    {
        invincibilityDurationSlider.fillAmount = invincibilityTimer / invincibilityDuration;
        invincibilityUIObject.SetActive(invincibilityTimer >= 0.15f || invincibilityDurationSlider.color == Color.red);

        if (isInvincible)
        {
            DecreaseInvincibilityTimer();
        }
        else
        {
            UpdateInvincibilityTimer();
        }
    }

    private void DecreaseInvincibilityTimer()
    {
        invincibilityTimer -= Time.deltaTime * shieldDeactivationRate;
        if (invincibilityTimer <= 0)
        {
            DisableInvincibility();
        }
    }

    private void UpdateInvincibilityTimer()
    {
        if (isFalling && !isInvincible && !hasFailedCollision)
        {
            invincibilityTimer += Time.deltaTime * fallTimerIncrementRate;
        }
        else
        {
            invincibilityTimer -= Time.deltaTime * fallTimerDecrementRate;
        }
        invincibilityTimer = Mathf.Clamp(invincibilityTimer, 0, invincibilityDuration);
        if (invincibilityTimer >= invincibilityDuration)
        {
            EnableInvincibility();
        }
    }

    private void EnableInvincibility()
    {
        invincibilityTimer = invincibilityDuration;
        isInvincible = true;
        powerOnUIObject.SetActive(true);
        powerOffUIObject.SetActive(false);
        invincibilityDurationSlider.color = Color.red;
    }

    private void DisableInvincibility()
    {
        invincibilityTimer = 0;
        isInvincible = false;
        powerOnUIObject.SetActive(false);
        powerOffUIObject.SetActive(true);
        invincibilityDurationSlider.color = Color.white;
    }

    private void UpdateInvincibilityShield()
    {
        invincibilityShieldObject?.SetActive(isInvincible);
    }

    #endregion

    #region Movement Methods

    private void ProcessFixedUpdate()
    {
        if (playerState == PlayerState.Playing)
        {
            ApplyFallingVelocity();
        }
    }

    private void ApplyFallingVelocity()
    {
        if (isFalling)
        {
            playerRigidbody.velocity = Vector3.up * fallSpeed * Time.fixedDeltaTime;
        }
    }

    private void ApplyRisingVelocity()
    {
        playerRigidbody.velocity = Vector3.up * riseSpeed * Time.deltaTime;
        SoundManager.Instance.PlaySoundFX(bounceAudioClip, 0.5f);
    }

    #endregion

    #region Collision Handling Methods

    private void OnCollisionEnter(Collision collision)
    {
        if (isFalling)
        {
            HandleCollision(collision);
        }
        else
        {
            ApplyRisingVelocity();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!isFalling || collision.gameObject.CompareTag("Finish"))
        {
            ApplyRisingVelocity();
        }
        else
        {
            HandleCollisionStay(collision);
        }
    }

    private void HandleCollision(Collision collision)
    {
        if (isInvincible)
        {
            HandleInvincibleCollision(collision);
        }
        else
        {
            HandleNormalCollision(collision);
        }

        if (collision.gameObject.CompareTag("Finish") && playerState == PlayerState.Playing)
        {
            CompleteLevel();
        }
    }

    private void HandleInvincibleCollision(Collision collision)
    {
        if (collision.gameObject.CompareTag("Passable") || collision.gameObject.CompareTag("Fail"))
        {
            ShatterObstacle(collision);
            SoundManager.Instance.PlaySoundFX(invincibleDestroyAudioClip, 0.5f);
            currentObstacleCount++;
            UpdateLevelProgress();
        }
    }

    private void HandleNormalCollision(Collision collision)
    {
        if (collision.gameObject.CompareTag("Passable"))
        {
            ShatterObstacle(collision);
            SoundManager.Instance.PlaySoundFX(destroyAudioClip, 0.5f);
            currentObstacleCount++;
            UpdateLevelProgress();
        }
        else if (collision.gameObject.CompareTag("Fail"))
        {
            HandleFailCollision();
        }
    }

    private void HandleFailCollision()
    {
        if (isFalling && !isInvincible)
        {
            invincibilityTimer = 0;
            invincibilityDurationSlider.fillAmount = 0;
            hasFailedCollision = true;
        }
        GameOver();
    }

    private void HandleCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Fail"))
        {
            failCollisionDuration += Time.deltaTime;
            if (hasTouchedPassable && failCollisionDuration >= 0.1f)
            {
                HandleFailCollision();
            }
        }
        else if (collision.gameObject.CompareTag("Passable"))
        {
            hasTouchedPassable = true;
            failCollisionDuration = 0;
        }
        else if (collision.gameObject.CompareTag("Finish") && playerState == PlayerState.Playing)
        {
            CompleteLevel();
        }
    }

    #endregion

    #region Game State Methods

    private void GameOver()
    {
        levelUIManager.DisplayGameOverText();
        gameOverUIObject.SetActive(true);
        playerState = PlayerState.Dead;
        playerRigidbody.isKinematic = true;
        GameScoreManager.Instance.CheckAndUpdateScores();
        GameScoreManager.Instance.ResetScore();
        SoundManager.Instance.PlaySoundFX(deathAudioClip, 0.5f);
    }

    private void RestartGame()
    {
        GameScoreManager.Instance.CheckAndUpdateScores();
        GameScoreManager.Instance.ResetSessionScores(); // Reset session scores before restarting
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void LoadNextLevel()
    {
        PlayerPrefs.SetInt("Level", PlayerPrefs.GetInt("Level") + 1);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void CheckEndGameConditions()
    {
        if (playerState == PlayerState.Playing)
        {
            var remainingObstacles = FindObjectsOfType<ObstacleManager>();
            var rotateManager = GameObject.Find("Rotate Manager");

            if (remainingObstacles.Length == 0 && rotateManager != null && rotateManager.transform.childCount == 0)
            {
                CompleteLevel();
            }
        }
    }

    private void CompleteLevel()
    {
        playerState = PlayerState.Finished;
        SoundManager.Instance.PlaySoundFX(winAudioClip, 0.5f);
        DisplayLevelCompleteUI();
        levelUIManager.CompleteLevel();
    }

    #endregion

    #region UI Update Methods

    private void UpdateLevelProgress()
    {
        levelUIManager.SetLevelProgressImageFill(currentObstacleCount / (float)totalObstacleCount);
    }

    private void ShatterObstacle(Collision collision)
    {
        collision.transform.parent.GetComponent<ObstacleManager>().ShatterAllObstacles();
        int scoreToAdd = isInvincible ? 1 : 2;
        GameScoreManager.Instance.AddScore(scoreToAdd);
    }

    private void DisplayLevelCompleteUI()
    {
        levelCompleteUIObject.SetActive(true);
        levelCompleteUIObject.transform.GetChild(0).GetComponent<Text>().text = "Level " + PlayerPrefs.GetInt("Level") + "\nCompleted";
    }

    #endregion
}
