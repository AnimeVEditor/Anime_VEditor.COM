using UnityEngine;

/// <summary>
/// Player Controller - Handles player movement, lane switching, jumping, and sliding
/// Supports both touch swipe controls and keyboard input for testing
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Singleton")]
    public static PlayerController Instance;

    [Header("Lane Settings")]
    public int currentLane = 1; // 0: Left, 1: Middle, 2: Right
    public float laneDistance = 3f; // Distance between lanes
    private Vector3 targetPosition;

    [Header("Movement Speed")]
    private float moveSpeed;
    public float laneSwitchSpeed = 10f;
    public float gravity = -20f;

    [Header("Jump Settings")]
    public float jumpForce = 8f;
    public float airTime = 0.5f;
    private bool isGrounded = true;
    private float verticalVelocity = 0f;

    [Header("Slide Settings")]
    public float slideDuration = 0.8f;
    private bool isSliding = false;
    private float slideTimer = 0f;
    private float originalHeight = 1.8f;
    private float slideHeight = 0.9f;

    [Header("Shield")]
    public bool isShieldActive = false;
    public GameObject shieldVisual;

    [Header("Character")]
    public CharacterController characterController;
    public Animator animator;
    public Transform groundCheck;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;

    [Header("Swipe Detection")]
    public float minSwipeDistance = 50f;
    public float maxSwipeTime = 0.5f;
    private Vector2 swipeStartPos;
    private Vector2 swipeEndPos;
    private float swipeStartTime;

    [Header("Power-up Effects")]
    public Material normalMaterial;
    public Material shieldMaterial;
    public Renderer characterRenderer;

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
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponent<Animator>();

        originalHeight = characterController.height;
        targetPosition = transform.position;
        
        // Initialize renderer materials
        if (characterRenderer == null)
            characterRenderer = GetComponentInChildren<Renderer>();
    }

    void Update()
    {
        if (!GameManager.Instance.isGameRunning || GameManager.Instance.isGameOver) return;

        moveSpeed = GameManager.Instance.GetCurrentSpeed();

        // Handle Input
        HandleKeyboardInput();
        HandleSwipeInput();

        // Calculate target lane position
        targetPosition.x = (currentLane - 1) * laneDistance;

        // Smooth lane switching
        Vector3 newPos = Vector3.MoveTowards(transform.position, targetPosition, laneSwitchSpeed * Time.deltaTime);

        // Apply gravity
        if (!isGrounded)
        {
            verticalVelocity += gravity * Time.deltaTime;
            newPos.y += verticalVelocity * Time.deltaTime;
        }

        // Ground check
        if (newPos.y <= 0)
        {
            newPos.y = 0;
            verticalVelocity = 0;
            isGrounded = true;
        }

        // Handle sliding
        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            characterController.height = Mathf.Lerp(characterController.height, slideHeight, Time.deltaTime * 10f);
            
            if (slideTimer <= 0)
            {
                isSliding = false;
                characterController.height = originalHeight;
            }
        }

        // Move character
        characterController.Move((newPos - transform.position) + Vector3.forward * moveSpeed * Time.deltaTime);

        // Update animations
        UpdateAnimations();

        // Update shield visual
        if (shieldVisual != null)
            shieldVisual.SetActive(isShieldActive);
    }

    void HandleKeyboardInput()
    {
        // For testing in Unity Editor
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            SwitchLane(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            SwitchLane(1);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            Slide();
        }
    }

    void HandleSwipeInput()
    {
        // Touch input for mobile
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    swipeStartPos = touch.position;
                    swipeStartTime = Time.time;
                    break;

                case TouchPhase.Ended:
                    swipeEndPos = touch.position;
                    DetectSwipe();
                    break;
            }
        }
    }

    void DetectSwipe()
    {
        float swipeTime = Time.time - swipeStartTime;
        if (swipeTime > maxSwipeTime) return;

        Vector2 swipeDelta = swipeEndPos - swipeStartPos;

        if (swipeDelta.magnitude < minSwipeDistance) return;

        float x = swipeDelta.x;
        float y = swipeDelta.y;

        if (Mathf.Abs(x) > Mathf.Abs(y))
        {
            // Horizontal swipe
            if (x > 0)
                SwitchLane(1); // Swipe Right
            else
                SwitchLane(-1); // Swipe Left
        }
        else
        {
            // Vertical swipe
            if (y > 0)
                Jump(); // Swipe Up
            else
                Slide(); // Swipe Down
        }
    }

    public void SwitchLane(int direction)
    {
        if (isSliding) return; // Can't switch lanes while sliding

        currentLane += direction;
        currentLane = Mathf.Clamp(currentLane, 0, 2);
    }

    public void Jump()
    {
        if (!isGrounded || isSliding) return;

        isGrounded = false;
        verticalVelocity = jumpForce;
        
        if (animator != null)
            animator.SetTrigger("Jump");
    }

    public void Slide()
    {
        if (!isGrounded || isSliding) return;

        isSliding = true;
        slideTimer = slideDuration;
        
        if (animator != null)
            animator.SetTrigger("Slide");
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsSliding", isSliding);
        animator.SetFloat("Speed", moveSpeed);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            if (isShieldActive)
            {
                // Shield protects from one hit
                isShieldActive = false;
                Destroy(collision.gameObject);
            }
            else
            {
                GameManager.Instance.PlayerHitObstacle();
                
                // Play hit animation
                if (animator != null)
                    animator.SetTrigger("Hit");
            }
        }
        else if (collision.gameObject.CompareTag("Coin"))
        {
            GameManager.Instance.CollectCoin();
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("PowerUp"))
        {
            PowerUp powerUp = collision.gameObject.GetComponent<PowerUp>();
            if (powerUp != null)
            {
                GameManager.Instance.ActivatePowerUp(powerUp.powerUpType, powerUp.duration);
                Destroy(collision.gameObject);
            }
        }
    }

    public void ResetPlayer()
    {
        currentLane = 1;
        targetPosition = new Vector3(0, 0, 0);
        transform.position = targetPosition;
        isGrounded = true;
        isSliding = false;
        isShieldActive = false;
        verticalVelocity = 0;
        characterController.height = originalHeight;
    }
}
