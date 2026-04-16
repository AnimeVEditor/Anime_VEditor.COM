using UnityEngine;

/// <summary>
/// Main Game Manager - Controls game state, score, speed, and game over logic
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    public static GameManager Instance;
    public bool isGameRunning = false;
    public bool isGameOver = false;

    [Header("Score & Distance")]
    public float distance = 0f;
    public int score = 0;
    public int coins = 0;
    public float scoreMultiplier = 1f;

    [Header("Speed Settings")]
    public float initialSpeed = 5f;
    public float maxSpeed = 15f;
    public float speedIncreaseRate = 0.1f; // Speed increase per second
    private float currentSpeed;

    [Header("Don (Villain)")]
    public Transform donTransform;
    public float donMaxDistance = 10f;
    public float donCurrentDistance = 0f;
    public float donCatchUpSpeed = 2f;

    [Header("UI References")]
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public UnityEngine.UI.Text scoreText;
    public UnityEngine.UI.Text coinsText;
    public UnityEngine.UI.Text distanceText;

    [Header("Audio")]
    public AudioClip crashSound;
    public AudioClip coinSound;
    public AudioClip powerUpSound;

    private bool isPaused = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        currentSpeed = initialSpeed;
        donCurrentDistance = donMaxDistance;
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (!isGameRunning || isGameOver || isPaused) return;

        // Increase distance
        distance += currentSpeed * Time.deltaTime;
        
        // Calculate score based on distance
        score = Mathf.FloorToInt(distance * scoreMultiplier);

        // Increase speed over time
        if (currentSpeed < maxSpeed)
        {
            currentSpeed += speedIncreaseRate * Time.deltaTime;
        }

        // Update UI
        UpdateUI();
    }

    public void StartGame()
    {
        isGameRunning = true;
        isGameOver = false;
        distance = 0f;
        score = 0;
        coins = 0;
        currentSpeed = initialSpeed;
        donCurrentDistance = donMaxDistance;
        scoreMultiplier = 1f;
        Time.timeScale = 1f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    public void PlayerHitObstacle()
    {
        if (isGameOver) return;

        // Play crash sound
        if (crashSound != null)
            AudioSource.PlayClipAtPoint(crashSound, Camera.main.transform.position);

        // Slow down player temporarily
        currentSpeed = Mathf.Max(initialSpeed, currentSpeed - 3f);

        // Don gets closer
        donCurrentDistance -= donCatchUpSpeed;

        // Check if caught
        if (donCurrentDistance <= 0f)
        {
            GameOver();
        }
    }

    public void CollectCoin(int amount = 1)
    {
        coins += amount;
        if (coinSound != null)
            AudioSource.PlayClipAtPoint(coinSound, Camera.main.transform.position);
    }

    public void ActivatePowerUp(PowerUpType type, float duration)
    {
        StartCoroutine(PowerUpEffect(type, duration));
    }

    private System.Collections.IEnumerator PowerUpEffect(PowerUpType type, float duration)
    {
        if (powerUpSound != null)
            AudioSource.PlayClipAtPoint(powerUpSound, Camera.main.transform.position);

        switch (type)
        {
            case PowerUpType.SpeedBoost:
                float originalSpeed = currentSpeed;
                currentSpeed *= 1.5f;
                yield return new WaitForSeconds(duration);
                currentSpeed = originalSpeed;
                break;

            case PowerUpType.Shield:
                PlayerController.Instance.isShieldActive = true;
                yield return new WaitForSeconds(duration);
                PlayerController.Instance.isShieldActive = false;
                break;

            case PowerUpType.CoinMagnet:
                CoinCollector.Instance.isMagnetActive = true;
                yield return new WaitForSeconds(duration);
                CoinCollector.Instance.isMagnetActive = false;
                break;

            case PowerUpType.DoubleScore:
                scoreMultiplier = 2f;
                yield return new WaitForSeconds(duration);
                scoreMultiplier = 1f;
                break;
        }
    }

    public void GameOver()
    {
        isGameOver = true;
        isGameRunning = false;
        Time.timeScale = 0f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Save high score
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (score > highScore)
        {
            PlayerPrefs.SetInt("HighScore", score);
            PlayerPrefs.Save();
        }
    }

    public void PauseGame()
    {
        if (isGameOver) return;
        
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (pausePanel != null)
            pausePanel.SetActive(isPaused);
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score.ToString();

        if (coinsText != null)
            coinsText.text = "Coins: " + coins.ToString();

        if (distanceText != null)
            distanceText.text = "Distance: " + distance.ToString("F0") + "m";
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
}

public enum PowerUpType
{
    SpeedBoost,
    Shield,
    CoinMagnet,
    DoubleScore
}
