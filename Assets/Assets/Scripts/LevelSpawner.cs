using CartoonFX;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSpawner : MonoBehaviour
{
    [Header("Obstacle Settings")]
    [Tooltip("Array of available obstacle prefabs.")]
    [SerializeField] private GameObject[] obstaclePrefabs;
    [Tooltip("Number of different obstacles to use per level.")]
    [SerializeField] private int obstaclesPerLevel = 4;

    [Header("Level Settings")]
    [SerializeField] private int initialLevel = 1;
    [Tooltip("Amount to add to the level for obstacle spawning.")]
    [SerializeField] private int levelAddition = 7;
    [Tooltip("Threshold for first difficulty level.")]
    [SerializeField] private int difficultyThreshold1 = 20;
    [Tooltip("Threshold for second difficulty level.")]
    [SerializeField] private int difficultyThreshold2 = 50;
    [Tooltip("Threshold for third difficulty level.")]
    [SerializeField] private int difficultyThreshold3 = 100;

    [Header("Win Object Settings")]
    [SerializeField] private GameObject winObjectPrefab;

    [Header("UI Settings")]
    [SerializeField] private Text currentLevelText;
    [SerializeField] private Text nextLevelText;

    [Header("Player Settings")]
    [SerializeField] private Material playerPlateMaterial;
    [SerializeField] private Material playerBaseMaterial;
    [SerializeField] private MeshRenderer playerMeshRenderer;

    [Header("Effect Settings")]
    [SerializeField] private TrailRenderer playerTrailRenderer;
    [Tooltip("Array of particle systems to match the color.")]
    [SerializeField] private ParticleSystem[] playerParticleSystems;
    [Tooltip("Array of CFXR_Effect components to match the color gradient.")]
    [SerializeField] private CFXR_Effect[] playerCFXREffects;

    private List<GameObject> currentObstacles = new List<GameObject>();
    private float obstacleYPosition;
    private int currentLevel;
    private int totalInstantiatedObjects = 0;

    private void Awake()
    {
        InitializeLevel();
        InitializeMaterials();
        LoadObstacles();
    }

    private void Start()
    {
        EnsureObstaclesGenerated();
        SpawnObstacles();
        SpawnWinObject();
        ApplyEffectColors();
        LogTotalInstantiatedObjects();
    }

    #region Level Initialization

    private void InitializeLevel()
    {
        currentLevel = PlayerPrefs.GetInt("Level", initialLevel);
        UpdateLevelUI();
    }

    private void UpdateLevelUI()
    {
        currentLevelText.text = currentLevel.ToString();
        nextLevelText.text = (currentLevel + 1).ToString();
    }

    #endregion

    #region Material Initialization

    private void InitializeMaterials()
    {
        int savedLevel = PlayerPrefs.GetInt("SavedLevel", -1);
        if (savedLevel != currentLevel)
        {
            ResetMaterialColors();
            PlayerPrefs.SetInt("SavedLevel", currentLevel);
        }
        else if (PlayerPrefs.HasKey("PlayerPlateMaterialColor") && PlayerPrefs.HasKey("PlayerBaseMaterialColor"))
        {
            LoadMaterialColors();
        }
        else
        {
            ResetMaterialColors();
        }
    }

    private void ResetMaterialColors()
    {
        Color plateColor = GenerateRandomColor();
        Color baseColor = plateColor + Color.gray;
        ApplyMaterialColors(plateColor, baseColor);
        SaveMaterialColors(plateColor, baseColor);
    }

    private Color GenerateRandomColor()
    {
        return Random.ColorHSV(0, 1, 0.5f, 1, 1, 1);
    }

    private void ApplyMaterialColors(Color plateColor, Color baseColor)
    {
        playerPlateMaterial.color = plateColor;
        playerBaseMaterial.color = baseColor;
        playerMeshRenderer.material.color = baseColor;
    }

    private void SaveMaterialColors(Color plateColor, Color baseColor)
    {
        PlayerPrefs.SetString("PlayerPlateMaterialColor", JsonUtility.ToJson(plateColor));
        PlayerPrefs.SetString("PlayerBaseMaterialColor", JsonUtility.ToJson(baseColor));
    }

    private void LoadMaterialColors()
    {
        Color plateColor = JsonUtility.FromJson<Color>(PlayerPrefs.GetString("PlayerPlateMaterialColor"));
        Color baseColor = JsonUtility.FromJson<Color>(PlayerPrefs.GetString("PlayerBaseMaterialColor"));
        ApplyMaterialColors(plateColor, baseColor);
    }

    #endregion

    #region Obstacle Management

    private void LoadObstacles()
    {
        string savedObstacles = PlayerPrefs.GetString("CurrentObstacles", "");
        if (!string.IsNullOrEmpty(savedObstacles))
        {
            ParseSavedObstacles(savedObstacles);
        }
    }

    private void ParseSavedObstacles(string savedObstacles)
    {
        string[] obstacleIndices = savedObstacles.Split(',');
        currentObstacles.Clear();
        foreach (string index in obstacleIndices)
        {
            if (int.TryParse(index, out int obstacleIndex) && IsValidObstacleIndex(obstacleIndex))
            {
                currentObstacles.Add(obstaclePrefabs[obstacleIndex]);
            }
        }
    }

    private bool IsValidObstacleIndex(int index)
    {
        return index >= 0 && index < obstaclePrefabs.Length;
    }

    private void SaveObstacles(List<int> obstacleIndices)
    {
        PlayerPrefs.SetString("CurrentObstacles", string.Join(",", obstacleIndices));
    }

    private void EnsureObstaclesGenerated()
    {
        if (currentObstacles.Count == 0)
        {
            GenerateRandomObstacles();
        }
    }

    private void GenerateRandomObstacles()
    {
        List<int> obstacleIndices = new List<int>();
        int randomSet = Random.Range(0, obstaclePrefabs.Length / obstaclesPerLevel);
        int startIndex = randomSet * obstaclesPerLevel;

        currentObstacles.Clear();
        for (int i = 0; i < obstaclesPerLevel; i++)
        {
            int obstacleIndex = startIndex + i;
            currentObstacles.Add(obstaclePrefabs[obstacleIndex]);
            obstacleIndices.Add(obstacleIndex);
        }

        SaveObstacles(obstacleIndices);
    }

    private void SpawnObstacles()
    {
        float randomFactor = Random.value;
        for (obstacleYPosition = 0; obstacleYPosition > -currentLevel - levelAddition; obstacleYPosition -= 0.5f)
        {
            GameObject obstacle = InstantiateObstacle();
            if (obstacle != null)
            {
                totalInstantiatedObjects++;
                SetObstacleTransform(obstacle, randomFactor);
                obstacle.transform.parent = FindObjectOfType<RotateManager>().transform;
            }
        }
    }

    private GameObject InstantiateObstacle()
    {
        int index = GetObstacleIndex();
        return Instantiate(currentObstacles[index]);
    }

    private int GetObstacleIndex()
    {
        if (currentLevel < difficultyThreshold1)
            return Random.Range(0, 2);
        if (currentLevel < difficultyThreshold2)
            return Random.Range(1, 3);
        if (currentLevel < difficultyThreshold3)
            return Random.Range(2, 4);
        return Random.Range(3, 4);
    }

    private void SetObstacleTransform(GameObject obstacle, float randomFactor)
    {
        obstacle.transform.position = new Vector3(0, obstacleYPosition - 0.01f, 0);
        obstacle.transform.eulerAngles = new Vector3(0, obstacleYPosition * 8, 0);

        AdjustObstacleRotation(obstacle, randomFactor);
    }

    private void AdjustObstacleRotation(GameObject obstacle, float randomFactor)
    {
        if (IsWithinMidLevelRange())
        {
            obstacle.transform.eulerAngles += Vector3.up * 180;
        }
        else if (IsNearEndLevelRange() && randomFactor > 0.75f)
        {
            obstacle.transform.eulerAngles += Vector3.up * 90;
        }
    }

    private bool IsWithinMidLevelRange()
    {
        return Mathf.Abs(obstacleYPosition) >= currentLevel * 0.3f && Mathf.Abs(obstacleYPosition) <= currentLevel * 0.6f;
    }

    private bool IsNearEndLevelRange()
    {
        return Mathf.Abs(obstacleYPosition) >= currentLevel * 0.8f;
    }

    private void SpawnWinObject()
    {
        GameObject winObject = Instantiate(winObjectPrefab);
        winObject.transform.position = new Vector3(0, obstacleYPosition - 0.01f, 0);
        totalInstantiatedObjects++;
    }

    #endregion

    #region Effect Management

    private void ApplyEffectColors()
    {
        if (playerMeshRenderer != null)
        {
            Color playerColor = playerMeshRenderer.material.color;
            ApplyTrailRendererColor(playerColor);
            ApplyParticleSystemColors(playerColor);
            ApplyCFXREffectColors(playerColor);
        }
    }

    private void ApplyTrailRendererColor(Color playerColor)
    {
        playerTrailRenderer.material.color = playerColor;
    }

    private void ApplyParticleSystemColors(Color playerColor)
    {
        foreach (ParticleSystem ps in playerParticleSystems)
        {
            var main = ps.main;
            main.startColor = playerColor;
        }
    }

    private void ApplyCFXREffectColors(Color playerColor)
    {
        foreach (CFXR_Effect effect in playerCFXREffects)
        {
            Gradient gradient = CreateGradient(playerColor);
            effect.animatedLights[0].colorGradient = gradient;
        }
    }

    private Gradient CreateGradient(Color color)
    {
        return new Gradient
        {
            colorKeys = new GradientColorKey[] { new GradientColorKey(color, 0.0f), new GradientColorKey(color, 1.0f) },
            alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
        };
    }

    #endregion

    #region Utility

    private void LogTotalInstantiatedObjects()
    {
        Debug.Log($"Total objects instantiated: {totalInstantiatedObjects}");
    }

    public void LoadNextLevel()
    {
        PlayerPrefs.SetInt("Level", PlayerPrefs.GetInt("Level") + 1);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    #endregion
}
