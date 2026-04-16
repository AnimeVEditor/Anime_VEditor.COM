using UnityEngine;

/// <summary>
/// Obstacle Spawner - Generates obstacles, coins, and power-ups in three lanes
/// Creates infinite running environment with increasing difficulty
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    [Header("Singleton")]
    public static ObstacleSpawner Instance;

    [Header("Spawn Settings")]
    public float spawnDistance = 50f; // Distance ahead of player to spawn
    public float despawnDistance = -10f; // Distance behind player to despawn
    public float minSpawnInterval = 1f;
    public float maxSpawnInterval = 2.5f;
    private float nextSpawnTime = 0f;
    private float currentSpawnInterval = 2f;

    [Header("Lane Configuration")]
    public int numberOfLanes = 3;
    public float laneWidth = 3f;

    [Header("Obstacle Prefabs")]
    public GameObject[] lowObstacles; // Need to jump over
    public GameObject[] highObstacles; // Need to slide under
    public GameObject[] fullObstacles; // Need to dodge (full height)
    public GameObject[] movingObstacles; // Trains, cars

    [Header("Coin Prefabs")]
    public GameObject coinPrefab;
    public GameObject coinLinePrefab;
    public float coinSpawnChance = 0.7f;
    public int minCoinsPerLine = 3;
    public int maxCoinsPerLine = 8;

    [Header("Power-up Prefabs")]
    public GameObject[] powerUpPrefabs;
    public float powerUpSpawnChance = 0.15f;

    [Header("Environment")]
    public GameObject[] environmentPrefabs;
    public float environmentSpawnDistance = 100f;
    private float lastEnvironmentSpawnZ = 0f;

    [Header("Object Pooling")]
    public int poolSize = 50;
    private Queue<GameObject>[] obstaclePools;
    private List<GameObject>[] activeObstacles;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializePools();
        nextSpawnTime = Time.time + 1f;
    }

    void InitializePools()
    {
        obstaclePools = new Queue<GameObject>[6]; // Different obstacle types
        activeObstacles = new List<GameObject>[6];

        for (int i = 0; i < 6; i++)
        {
            obstaclePools[i] = new Queue<GameObject>();
            activeObstacles[i] = new List<GameObject>();

            // Create pool objects
            for (int j = 0; j < poolSize / 6; j++)
            {
                GameObject obj = new GameObject("PooledObstacle_" + i);
                obj.SetActive(false);
                obstaclePools[i].Enqueue(obj);
            }
        }
    }

    void Update()
    {
        if (!GameManager.Instance.isGameRunning || GameManager.Instance.isGameOver) return;

        // Spawn obstacles
        if (Time.time >= nextSpawnTime)
        {
            SpawnObstacleSet();
            
            // Decrease spawn interval as speed increases
            float speedMultiplier = GameManager.Instance.GetCurrentSpeed() / GameManager.Instance.initialSpeed;
            currentSpawnInterval = Mathf.Lerp(maxSpawnInterval, minSpawnInterval, 
                Mathf.Clamp01((speedMultiplier - 1f) / 2f));
            
            nextSpawnTime = Time.time + currentSpawnInterval;
        }

        // Spawn environment
        SpawnEnvironment();

        // Clean up objects behind player
        CleanupObjects();
    }

    void SpawnObstacleSet()
    {
        // Get player position
        float playerZ = PlayerController.Instance.transform.position.z;
        float spawnZ = playerZ + spawnDistance;

        // Determine number of lanes to fill (1-3)
        int lanesToFill = Random.Range(1, 4);
        
        // Select which lanes to use
        List<int> availableLanes = new List<int> { 0, 1, 2 };
        List<int> selectedLanes = new List<int>();

        for (int i = 0; i < lanesToFill; i++)
        {
            int randomIndex = Random.Range(0, availableLanes.Count);
            selectedLanes.Add(availableLanes[randomIndex]);
            availableLanes.RemoveAt(randomIndex);
        }

        // Spawn obstacles in selected lanes
        foreach (int lane in selectedLanes)
        {
            SpawnObstacleInLane(lane, spawnZ);
        }

        // Spawn coins in empty lanes or above/between obstacles
        foreach (int lane in selectedLanes)
        {
            if (Random.value < coinSpawnChance)
            {
                SpawnCoins(lane, spawnZ);
            }
        }

        // Chance to spawn power-up
        if (Random.value < powerUpSpawnChance)
        {
            int powerUpLane = Random.Range(0, 3);
            SpawnPowerUp(powerUpLane, spawnZ);
        }
    }

    void SpawnObstacleInLane(int lane, float zPosition)
    {
        float xPos = (lane - 1) * laneWidth;
        Vector3 spawnPos = new Vector3(xPos, 0, zPosition);

        // Determine obstacle type based on difficulty and randomness
        float difficulty = GameManager.Instance.GetCurrentSpeed() / GameManager.Instance.maxSpeed;
        int obstacleType = Random.Range(0, 4);

        GameObject obstacle = null;

        switch (obstacleType)
        {
            case 0: // Low obstacle (jump over)
                if (lowObstacles.Length > 0)
                    obstacle = Instantiate(lowObstacles[Random.Range(0, lowObstacles.Length)], spawnPos, Quaternion.identity);
                break;

            case 1: // High obstacle (slide under)
                if (highObstacles.Length > 0)
                {
                    obstacle = Instantiate(highObstacles[Random.Range(0, highObstacles.Length)], spawnPos, Quaternion.identity);
                    // Position it higher for sliding
                    obstacle.transform.position += Vector3.up * 1.2f;
                }
                break;

            case 2: // Full obstacle (dodge)
                if (fullObstacles.Length > 0)
                    obstacle = Instantiate(fullObstacles[Random.Range(0, fullObstacles.Length)], spawnPos, Quaternion.identity);
                break;

            case 3: // Moving obstacle (train/car)
                if (movingObstacles.Length > 0 && difficulty > 0.3f)
                {
                    obstacle = Instantiate(movingObstacles[Random.Range(0, movingObstacles.Length)], spawnPos, Quaternion.identity);
                    // Add moving script
                    MovingObstacle movingScript = obstacle.AddComponent<MovingObstacle>();
                    movingScript.speed = Random.Range(2f, 5f) * (difficulty > 0.6f ? 1.5f : 1f);
                    movingScript.direction = Random.value > 0.5f ? 1 : -1;
                }
                else if (fullObstacles.Length > 0)
                {
                    obstacle = Instantiate(fullObstacles[Random.Range(0, fullObstacles.Length)], spawnPos, Quaternion.identity);
                }
                break;
        }

        if (obstacle != null)
        {
            obstacle.tag = "Obstacle";
            obstacle.layer = LayerMask.NameToLayer("Obstacle");
        }
    }

    void SpawnCoins(int lane, float zPosition)
    {
        float xPos = (lane - 1) * laneWidth;
        int numCoins = Random.Range(minCoinsPerLine, maxCoinsPerLine + 1);

        // Decide coin pattern
        int pattern = Random.Range(0, 3);

        for (int i = 0; i < numCoins; i++)
        {
            Vector3 coinPos;

            switch (pattern)
            {
                case 0: // Straight line on ground
                    coinPos = new Vector3(xPos, 0.5f, zPosition + i * 2f);
                    break;

                case 1: // Arc pattern (need to jump)
                    coinPos = new Vector3(xPos, Mathf.Sin(i * 0.5f) * 2f + 0.5f, zPosition + i * 2f);
                    break;

                default: // Zigzag
                    coinPos = new Vector3(xPos + Mathf.Sin(i * Mathf.PI) * 1.5f, 0.5f, zPosition + i * 2f);
                    break;
            }

            if (coinPrefab != null)
            {
                GameObject coin = Instantiate(coinPrefab, coinPos, Quaternion.identity);
                coin.tag = "Coin";
                
                // Add rotation animation
                Coin coinScript = coin.AddComponent<Coin>();
                coinScript.rotationSpeed = 100f;
            }
        }
    }

    void SpawnPowerUp(int lane, float zPosition)
    {
        if (powerUpPrefabs.Length == 0) return;

        float xPos = (lane - 1) * laneWidth;
        Vector3 spawnPos = new Vector3(xPos, 1f, zPosition);

        int powerUpIndex = Random.Range(0, powerUpPrefabs.Length);
        GameObject powerUp = Instantiate(powerUpPrefabs[powerUpIndex], spawnPos, Quaternion.identity);
        powerUp.tag = "PowerUp";

        // Ensure PowerUp component exists
        if (powerUp.GetComponent<PowerUp>() == null)
        {
            PowerUp pu = powerUp.AddComponent<PowerUp>();
            pu.powerUpType = (PowerUpType)powerUpIndex;
            pu.duration = 5f;
        }
    }

    void SpawnEnvironment()
    {
        float playerZ = PlayerController.Instance.transform.position.z;

        if (playerZ + environmentSpawnDistance > lastEnvironmentSpawnZ)
        {
            lastEnvironmentSpawnZ = playerZ + environmentSpawnDistance;

            // Spawn environment on both sides
            for (int side = -1; side <= 1; side += 2)
            {
                if (environmentPrefabs.Length > 0)
                {
                    GameObject env = Instantiate(environmentPrefabs[Random.Range(0, environmentPrefabs.Length)]);
                    env.transform.position = new Vector3(side * 15f, 0, lastEnvironmentSpawnZ);
                }
            }
        }
    }

    void CleanupObjects()
    {
        float playerZ = PlayerController.Instance.transform.position.z;

        // Find and deactivate objects behind player
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.CompareTag("Obstacle") || obj.CompareTag("Coin") || obj.CompareTag("PowerUp"))
            {
                if (obj.transform.position.z < playerZ + despawnDistance)
                {
                    if (obj.CompareTag("Obstacle"))
                    {
                        // Return to pool instead of destroying
                        // For simplicity, just destroy in this version
                        Destroy(obj);
                    }
                    else
                    {
                        Destroy(obj);
                    }
                }
            }
        }
    }

    public void IncreaseDifficulty()
    {
        minSpawnInterval = Mathf.Max(0.5f, minSpawnInterval - 0.1f);
        maxSpawnInterval = Mathf.Max(1f, maxSpawnInterval - 0.2f);
    }
}
