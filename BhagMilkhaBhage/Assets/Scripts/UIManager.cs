using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UIManager - Handles all UI elements, menus, and HUD
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Screens")]
    public GameObject mainMenuScreen;
    public GameObject gameScreen;
    public GameObject pauseScreen;
    public GameObject gameOverScreen;

    [Header("HUD Elements")]
    public Text scoreText;
    public Text coinsText;
    public Text distanceText;
    public Text highScoreText;
    public Slider donProximitySlider;
    public Image shieldIcon;
    public Image speedBoostIcon;
    public Image magnetIcon;
    public Image doubleScoreIcon;

    [Header("Game Over")]
    public Text finalScoreText;
    public Text newHighScoreText;
    public Button restartButton;
    public Button mainMenuButton;

    [Header("Main Menu")]
    public Button playButton;
    public Button settingsButton;
    public Button charactersButton;
    public Button shopButton;

    [Header("Settings")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle dayNightToggle;

    void Start()
    {
        InitializeUI();
        SetupButtonListeners();
    }

    void InitializeUI()
    {
        // Show main menu initially
        if (mainMenuScreen != null)
            mainMenuScreen.SetActive(true);

        if (gameScreen != null)
            gameScreen.SetActive(false);

        if (pauseScreen != null)
            pauseScreen.SetActive(false);

        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);

        // Load high score
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (highScoreText != null)
            highScoreText.text = "High Score: " + highScore.ToString();
    }

    void SetupButtonListeners()
    {
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);

        if (charactersButton != null)
            charactersButton.onClick.AddListener(OnCharactersClicked);

        if (shopButton != null)
            shopButton.onClick.AddListener(OnShopClicked);
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        // Update HUD
        UpdateHUD();

        // Handle pause input
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (GameManager.Instance.isGameRunning && !GameManager.Instance.isGameOver)
            {
                GameManager.Instance.PauseGame();
                UpdatePauseUI();
            }
        }
    }

    void UpdateHUD()
    {
        if (!GameManager.Instance.isGameRunning) return;

        if (scoreText != null)
            scoreText.text = GameManager.Instance.score.ToString();

        if (coinsText != null)
            coinsText.text = GameManager.Instance.coins.ToString();

        if (distanceText != null)
            distanceText.text = GameManager.Instance.distance.ToString("F0") + "m";

        // Update don proximity slider
        if (donProximitySlider != null)
        {
            float normalizedDistance = GameManager.Instance.donCurrentDistance / GameManager.Instance.donMaxDistance;
            donProximitySlider.value = normalizedDistance;
        }

        // Update power-up icons
        UpdatePowerUpIcons();
    }

    void UpdatePowerUpIcons()
    {
        PlayerController player = PlayerController.Instance;
        if (player == null) return;

        if (shieldIcon != null)
            shieldIcon.enabled = player.isShieldActive;

        if (CoinCollector.Instance != null)
        {
            if (magnetIcon != null)
                magnetIcon.enabled = CoinCollector.Instance.isMagnetActive;
        }

        // Speed boost and double score would need additional tracking
        // For now, we'll show them when active (this is simplified)
    }

    void UpdatePauseUI()
    {
        bool isPaused = Time.timeScale == 0f;
        if (pauseScreen != null)
            pauseScreen.SetActive(isPaused);
    }

    #region Button Click Handlers

    void OnPlayClicked()
    {
        if (mainMenuScreen != null)
            mainMenuScreen.SetActive(false);

        if (gameScreen != null)
            gameScreen.SetActive(true);

        GameManager.Instance.StartGame();
        PlayerController.Instance.ResetPlayer();
    }

    void OnRestartClicked()
    {
        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);

        GameManager.Instance.StartGame();
        PlayerController.Instance.ResetPlayer();
    }

    void OnMainMenuClicked()
    {
        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);

        if (gameScreen != null)
            gameScreen.SetActive(false);

        if (mainMenuScreen != null)
            mainMenuScreen.SetActive(true);

        Time.timeScale = 1f;
    }

    void OnSettingsClicked()
    {
        // Open settings panel (implementation depends on your UI structure)
        Debug.Log("Settings clicked");
    }

    void OnCharactersClicked()
    {
        // Open character selection screen
        Debug.Log("Characters clicked");
    }

    void OnShopClicked()
    {
        // Open shop screen
        Debug.Log("Shop clicked");
    }

    #endregion

    #region Volume Controls

    public void OnMusicVolumeChanged(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
    }

    public void OnSFXVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
    }

    public void OnDayNightToggled(bool isOn)
    {
        PlayerPrefs.SetInt("DayNightEnabled", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    #endregion

    public void ShowGameOver()
    {
        if (gameOverScreen != null)
            gameOverScreen.SetActive(true);

        if (finalScoreText != null)
            finalScoreText.text = "Score: " + GameManager.Instance.score.ToString();

        // Check for new high score
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (newHighScoreText != null)
        {
            if (GameManager.Instance.score >= highScore && GameManager.Instance.score > 0)
            {
                newHighScoreText.text = "NEW HIGH SCORE!";
                newHighScoreText.gameObject.SetActive(true);
            }
            else
            {
                newHighScoreText.gameObject.SetActive(false);
            }
        }
    }

    public void LoadSavedSettings()
    {
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 0.7f);
        bool dayNight = PlayerPrefs.GetInt("DayNightEnabled", 1) == 1;

        if (musicVolumeSlider != null)
            musicVolumeSlider.value = musicVol;

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = sfxVol;

        if (dayNightToggle != null)
            dayNightToggle.isOn = dayNight;
    }
}

/// <summary>
/// MainMenu - Handles main menu animations and transitions
/// </summary>
public class MainMenu : MonoBehaviour
{
    public GameObject titleObject;
    public GameObject[] menuButtons;
    public Animator menuAnimator;

    void Start()
    {
        // Animate title
        if (titleObject != null)
        {
            LeanTweenType leanType = LeanTweenType.easeInOutSine;
            // Note: This assumes LeanTween plugin, otherwise use Unity Animation
        }

        // Stagger button animations
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] != null)
            {
                // Add entrance animation
                CanvasGroup group = menuButtons[i].GetComponent<CanvasGroup>();
                if (group == null)
                    group = menuButtons[i].AddComponent<CanvasGroup>();
                
                group.alpha = 0;
                // Would animate in with coroutine or tweening library
            }
        }
    }
}

/// <summary>
/// CharacterSelector - Allows players to choose and unlock characters
/// </summary>
[System.Serializable]
public class CharacterData
{
    public string characterName;
    public Sprite characterPortrait;
    public GameObject characterPrefab;
    public bool isUnlocked;
    public int unlockCost;
    public string description;
}

public class CharacterSelector : MonoBehaviour
{
    public CharacterData[] characters;
    public int currentCharacterIndex = 0;
    public int playerCoins = 0;

    public Image characterPortrait;
    public Text characterName;
    public Text characterDescription;
    public Button selectButton;
    public Button unlockButton;
    public Text costText;

    void Start()
    {
        playerCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        LoadCharacters();
        UpdateDisplay();
    }

    void LoadCharacters()
    {
        // Load character unlock status from PlayerPrefs
        for (int i = 0; i < characters.Length; i++)
        {
            bool unlocked = PlayerPrefs.GetInt("Char_Unlocked_" + i, i == 0 ? 1 : 0) == 1;
            characters[i].isUnlocked = unlocked;
        }
    }

    public void NextCharacter()
    {
        currentCharacterIndex = (currentCharacterIndex + 1) % characters.Length;
        UpdateDisplay();
    }

    public void PreviousCharacter()
    {
        currentCharacterIndex = (currentCharacterIndex - 1 + characters.Length) % characters.Length;
        UpdateDisplay();
    }

    public void SelectCharacter()
    {
        if (characters[currentCharacterIndex].isUnlocked)
        {
            PlayerPrefs.SetInt("SelectedCharacter", currentCharacterIndex);
            PlayerPrefs.Save();
            Debug.Log("Character selected: " + characters[currentCharacterIndex].characterName);
        }
    }

    public void UnlockCharacter()
    {
        CharacterData selectedChar = characters[currentCharacterIndex];
        
        if (!selectedChar.isUnlocked && playerCoins >= selectedChar.unlockCost)
        {
            playerCoins -= selectedChar.unlockCost;
            selectedChar.isUnlocked = true;
            
            PlayerPrefs.SetInt("TotalCoins", playerCoins);
            PlayerPrefs.SetInt("Char_Unlocked_" + currentCharacterIndex, 1);
            PlayerPrefs.Save();
            
            UpdateDisplay();
        }
    }

    void UpdateDisplay()
    {
        CharacterData selectedChar = characters[currentCharacterIndex];
        
        if (characterPortrait != null && selectedChar.characterPortrait != null)
            characterPortrait.sprite = selectedChar.characterPortrait;
        
        if (characterName != null)
            characterName.text = selectedChar.characterName;
        
        if (characterDescription != null)
            characterDescription.text = selectedChar.description;
        
        if (selectButton != null)
            selectButton.interactable = selectedChar.isUnlocked;
        
        if (unlockButton != null)
            unlockButton.gameObject.SetActive(!selectedChar.isUnlocked);
        
        if (costText != null)
            costText.text = selectedChar.unlockCost.ToString() + " Coins";
    }
}
