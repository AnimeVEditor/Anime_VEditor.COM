using UnityEngine;

/// <summary>
/// Coin - Rotating coin that player can collect
/// </summary>
public class Coin : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public float bobbingAmplitude = 0.3f;
    public float bobbingSpeed = 2f;
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Rotate coin
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Bob up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * bobbingSpeed) * bobbingAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.CollectCoin();
            
            // Play collection effect
            ParticleSystem particles = GetComponent<ParticleSystem>();
            if (particles != null)
                particles.Play();
            
            Destroy(gameObject);
        }
    }
}

/// <summary>
/// PowerUp - Collectible power-up with different effects
/// </summary>
public class PowerUp : MonoBehaviour
{
    public PowerUpType powerUpType;
    public float duration = 5f;
    public float rotationSpeed = 50f;
    public Color[] powerUpColors;

    void Start()
    {
        // Set color based on power-up type
        if (powerUpColors.Length > (int)powerUpType)
        {
            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
                rend.material.color = powerUpColors[(int)powerUpType];
        }
    }

    void Update()
    {
        // Rotate power-up
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Float effect
        float floatY = Mathf.Sin(Time.time * 3f) * 0.2f;
        Vector3 pos = transform.position;
        pos.y = 1f + floatY;
        transform.position = pos;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.ActivatePowerUp(powerUpType, duration);
            Destroy(gameObject);
        }
    }
}

/// <summary>
/// MovingObstacle - Obstacles that move across lanes (trains, cars)
/// </summary>
public class MovingObstacle : MonoBehaviour
{
    public float speed = 5f;
    public int direction = 1; // 1: left to right, -1: right to left
    public float moveDistance = 10f;
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Move obstacle sideways
        Vector3 newPos = startPosition + Vector3.right * direction * speed * Time.time;
        
        // Clamp movement
        float xOffset = newPos.x - startPosition.x;
        if (Mathf.Abs(xOffset) > moveDistance)
        {
            direction *= -1;
            speed *= -1;
        }

        transform.position = newPos;
    }
}

/// <summary>
/// CoinCollector - Handles coin magnet functionality
/// </summary>
public class CoinCollector : MonoBehaviour
{
    public static CoinCollector Instance;
    public bool isMagnetActive = false;
    public float magnetRange = 8f;
    public float magnetForce = 15f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Update()
    {
        if (!isMagnetActive) return;

        // Find all coins in range
        GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");
        
        foreach (GameObject coin in coins)
        {
            float distance = Vector3.Distance(transform.position, coin.transform.position);
            
            if (distance < magnetRange)
            {
                // Pull coin towards player
                Vector3 direction = (transform.position - coin.transform.position).normalized;
                coin.transform.position += direction * magnetForce * Time.deltaTime;
            }
        }
    }
}

/// <summary>
/// CameraFollow - Smoothly follows the player
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Offset")]
    public Vector3 offset = new Vector3(0, 5, -8);

    [Header("Smooth Settings")]
    public float smoothSpeed = 5f;
    public float rotationSmoothSpeed = 2f;

    [Header("Limits")]
    public float minY = 3f;
    public float maxY = 10f;

    void LateUpdate()
    {
        if (target == null) return;

        // Calculate desired position
        Vector3 desiredPosition = target.position + offset;
        
        // Keep camera at consistent height relative to ground
        desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        desiredPosition.z = target.position.z + offset.z;

        // Smooth follow
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // Always look at player
        Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
        Quaternion smoothedRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
        transform.rotation = smoothedRotation;
    }
}

/// <summary>
/// DonChase - The villain chasing the player (visual only)
/// </summary>
public class DonChase : MonoBehaviour
{
    public Transform playerTransform;
    public float baseDistance = 10f;
    private float currentDistance;

    void Update()
    {
        if (playerTransform == null || GameManager.Instance == null) return;

        // Get current distance from game manager
        currentDistance = GameManager.Instance.donCurrentDistance;

        // Position don behind player
        Vector3 donPos = playerTransform.position;
        donPos.z -= (baseDistance - currentDistance);
        donPos.y = 0;

        transform.position = donPos;

        // Make don visible only when close
        float visibilityThreshold = 5f;
        gameObject.SetActive(currentDistance < visibilityThreshold * 2);
    }
}

/// <summary>
/// ParallaxBackground - Creates depth with scrolling background layers
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    public float parallaxFactor = 0.3f;
    private float lastPlayerX;
    private Transform playerTransform;

    void Start()
    {
        playerTransform = PlayerController.Instance?.transform;
        lastPlayerX = playerTransform != null ? playerTransform.position.z : 0;
    }

    void Update()
    {
        if (playerTransform == null) return;

        float playerZ = playerTransform.position.z;
        float distanceMoved = playerZ - lastPlayerX;
        
        transform.position += Vector3.forward * distanceMoved * parallaxFactor;
        
        lastPlayerX = playerZ;

        // Loop background
        if (transform.position.z > playerZ + 50)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, playerZ - 50);
        }
    }
}

/// <summary>
/// DayNightCycle - Simple day/night cycle with lighting changes
/// </summary>
public class DayNightCycle : MonoBehaviour
{
    [Header("Lighting")]
    public Light sunLight;
    public Light moonLight;

    [Header("Cycle Settings")]
    public float dayDuration = 120f; // Seconds per full day
    private float dayTimer = 0f;

    [Header("Sky Colors")]
    public Color daySkyColor = new Color(0.5f, 0.7f, 1f);
    public Color nightSkyColor = new Color(0.1f, 0.1f, 0.3f);
    public Color sunsetSkyColor = new Color(1f, 0.4f, 0.2f);

    void Update()
    {
        if (!GameManager.Instance.isGameRunning) return;

        dayTimer += Time.deltaTime;
        float normalizedTime = (dayTimer % dayDuration) / dayDuration;

        // Rotate sun/moon
        if (sunLight != null)
        {
            float angle = normalizedTime * 360f - 90f;
            sunLight.transform.rotation = Quaternion.Euler(angle, -30f, 0);
        }

        // Interpolate sky color
        Color skyColor;
        if (normalizedTime < 0.25f) // Morning
        {
            skyColor = Color.Lerp(nightSkyColor, daySkyColor, normalizedTime * 4);
        }
        else if (normalizedTime < 0.45f) // Day
        {
            skyColor = daySkyColor;
        }
        else if (normalizedTime < 0.5f) // Sunset
        {
            skyColor = Color.Lerp(daySkyColor, sunsetSkyColor, (normalizedTime - 0.45f) * 20);
        }
        else if (normalizedTime < 0.75f) // Evening
        {
            skyColor = Color.Lerp(sunsetSkyColor, nightSkyColor, (normalizedTime - 0.5f) * 4);
        }
        else // Night
        {
            skyColor = nightSkyColor;
        }

        RenderSettings.skybox.SetColor("_SkyColor", skyColor);
        RenderSettings.ambientLight = skyColor * 0.5f;

        // Toggle lights
        if (sunLight != null && moonLight != null)
        {
            float brightness = 1f - Mathf.Abs(normalizedTime - 0.5f) * 2;
            sunLight.intensity = Mathf.Max(0, brightness);
            moonLight.intensity = Mathf.Max(0, 1f - brightness);
        }
    }
}
