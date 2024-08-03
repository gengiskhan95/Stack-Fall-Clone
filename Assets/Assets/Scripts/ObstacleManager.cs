using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    [Header("Obstacle Settings")]
    [SerializeField] private Obstacle[] obstacles;

    private void Awake()
    {
        InitializeObstacles();
    }

    private void Start()
    {
        SetupInitialSettings();
    }

    #region Initialization

    private void SetupInitialSettings()
    {
        // Burada başlangıç ayarlarını yapabilirsiniz
    }

    private void InitializeObstacles()
    {
        List<Obstacle> obstacleList = new List<Obstacle>();

        foreach (Transform child in transform)
        {
            Obstacle obstacle = child.GetComponent<Obstacle>();
            if (obstacle != null)
            {
                obstacleList.Add(obstacle);
            }
        }

        obstacles = obstacleList.ToArray();
    }

    #endregion

    #region Obstacle Management

    public void ShatterAllObstacles()
    {
        DetachFromParent();
        ShatterObstacles();
        StartCoroutine(RemoveAllShatteredParts());
    }

    private void DetachFromParent()
    {
        if (transform.parent != null)
        {
            transform.parent = null;
        }
    }

    private void ShatterObstacles()
    {
        foreach (Obstacle obstacle in obstacles)
        {
            obstacle.Shatter();
        }
    }

    private IEnumerator RemoveAllShatteredParts()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    #endregion
}
