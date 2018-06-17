using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIController : Singleton<UIController>
{    
    [SerializeField]
    private Text text_console;
    [SerializeField]
    private Text text_status;
    [SerializeField]
    private GameObject buttons_zoom;
    [SerializeField]
    private GameObject buttons_rotatePlatform;
    [SerializeField]
    private GameObject buttons_top;
    [SerializeField]
    private Image image_corrector;

    private GameController gameController;
    private LevelsController levelsController;
    private MapsController mapsController;
    private ButtonsController buttonsController;
    private TimerController timeController;
    private TutorialsController tutorialController;
    private AudioManager audioManager;
    private MeshCombiner meshCombiner;

    public enum PanelTypes { NONE, MAIN_MENU, RESULTS, PLAY_LEVEL, SETTINGS, LOGIN }
    private PanelTypes activePanel;

    private const float FADESPEED = 0.25f;

    void Start()
    {
        gameController = GameController.Instance;
        levelsController = LevelsController.Instance;
        mapsController = MapsController.Instance;
        buttonsController = ButtonsController.Instance;
        timeController = TimerController.Instance;
        tutorialController = TutorialsController.Instance;
        audioManager = AudioManager.Instance;
        meshCombiner = MeshCombiner.Instance;
        activePanel = PanelTypes.NONE;

        // Define hint colors
        color_hintAlpha = image_hintFill.color;
        color_hintAlpha.a = 0f;
        color_hintFilled = color_hintAlpha;
        color_hintFilled.a = 1f;

        HideSettingsButton();
        HideCloseMapButton();

        LoadMusicSettings();

        // Start main menu enabled
        activePanel = PanelTypes.MAIN_MENU;
        fader_mainMenu.FadeIn(0f);
    }

    void OnEnable()
    {
        GameController.OnGameStarted += OnGameStarted;
        GameController.OnGameEnded += OnGameEnded;
        GameController.OnGamePaused += OnGamePaused;
        GameController.OnGameUnpaused += OnGameUnpaused;

        if (GameController.Instance.EnableAR)
        {
            ARCoreController.OnTrackingActive += OnTrackingActive;
            ARCoreController.OnTrackingLost += OnTrackingLost;
        }
    }

    void OnDisable()
    {

        GameController.OnGameStarted -= OnGameStarted;
        GameController.OnGameEnded -= OnGameEnded;
        GameController.OnGamePaused -= OnGamePaused;
        GameController.OnGameUnpaused -= OnGameUnpaused;

        if (gameController.EnableAR)
        {
            ARCoreController.OnTrackingActive -= OnTrackingActive;
            ARCoreController.OnTrackingLost -= OnTrackingLost;
        }
    }    

    private void OnGameStarted()
    {
        ShowHUD();
        ShowSettingsButton();
        RechargeHint(0f);        
    }

    private void OnGameEnded(GameController.EndGameTypes _type)
    {
        StopAllCoroutines();
        HideSettingsButton();
        HideHUD();
        ClearHint();
        ShowPanel(PanelTypes.RESULTS, _type);
    }

    private void OnGamePaused()
    {
        HideHUD();        
    }

    private void OnGameUnpaused()
    {
        ShowHUD();
    }

    private void OnTrackingActive()
    {
        if (!gameController.IsGameRunning || (gameController.IsGameRunning && gameController.IsGamePaused))
            return;

        ShowHUD();
    }

    private void OnTrackingLost()
    {
        if (!gameController.IsGameRunning || (gameController.IsGameRunning && gameController.IsGamePaused))
            return;

        HideHUD();
    }

    public void ShowPanel(PanelTypes panel, GameController.EndGameTypes _type = GameController.EndGameTypes.NONE)
    {
        // Hide last active panel (do not close if settings panel)
        if (activePanel != PanelTypes.MAIN_MENU)
            HideActivePanel();

        // Show new panel        
        switch (panel)
        {
            case PanelTypes.MAIN_MENU:
                ShowPanelMainMenu();
                break;

            case PanelTypes.RESULTS:
                ShowPanelResults(_type);
                break;

            case PanelTypes.PLAY_LEVEL:
                ShowPanelPlayLevel();
                break;

            case PanelTypes.SETTINGS:
                ShowPanelSettings();
                break;

            case PanelTypes.LOGIN:
                ShowPanelLogin();
                break;

            default:
                break;
        }
        activePanel = panel;        
    }    

    public void HidePanel(PanelTypes panel)
    {
        switch (panel)
        {
            case PanelTypes.MAIN_MENU:
                HidePanelMainMenu();
                break;

            case PanelTypes.RESULTS:
                HidePanelResults();
                break;

            case PanelTypes.PLAY_LEVEL:
                HidePanelPlayLevel();
                break;

            case PanelTypes.SETTINGS:
                HidePanelSettings();
                break;

            case PanelTypes.LOGIN:
                HidePanelLogin();
                break;

            default:
                break;
        }
        activePanel = PanelTypes.NONE;
        buttonsController.EnableAllButtons();
    }

    public void HideActivePanel()
    {
        HidePanel(activePanel);
    }

    #region MAIN MENU
    [SerializeField]
    private CanvasFader fader_mainMenu;

    private void ShowPanelMainMenu()
    {
        buttonsController.DisableAllButtonsExcept(fader_mainMenu.transform);
        fader_mainMenu.FadeIn(FADESPEED);
    }

    private void HidePanelMainMenu()
    {
        fader_mainMenu.FadeOut(FADESPEED);
    }

    public void OnPlayButtonClicked()
    {
        HidePanel(PanelTypes.MAIN_MENU);
        tutorialController.ShowTutorial(0);
    }
    #endregion


    #region PANEL RESULTS    
    [Header("Results Panel")]
    [SerializeField]
    private CanvasFader fader_Results;
    [SerializeField]
    private Text text_results;
    [SerializeField]
    private Image[] images_starFill;
    [SerializeField]
    private Button button_resultsNext;

    private void ShowPanelResults(GameController.EndGameTypes _type)
    {
        if (_type == GameController.EndGameTypes.NONE)
            return;

        ResetPanelResults();        
        StartCoroutine(ShowPanelResultsCR(_type));
    }

    private IEnumerator ShowPanelResultsCR(GameController.EndGameTypes _type)
    {
        buttonsController.DisableAllButtonsExcept(fader_Results.transform);

        // Fade in panel
        fader_Results.FadeIn(FADESPEED);
        while (fader_Results.IsFading)
            yield return null;

        // Win scenario
        if (_type == GameController.EndGameTypes.WIN)
        {
            audioManager.PlaySound(audioManager.audio_win);
            text_results.text = "COMPLETE!";

            float percent = (timeController.TimeLeft / timeController.TimeTotal) * 100f;            

            int starCount = 0;
            // 3 Stars
            if (percent >= 80.0f)
                starCount = 3;
            // 2 Stars
            else if (percent >= 60.0f)
                starCount = 2;
            // 1 Star
            else
                starCount = 1;

            // Star alpha color
            Color startColor = images_starFill[0].color;
            Color endColor = startColor;
            endColor.a = 1f;

            // Fade in stars consecutively
            yield return new WaitForSeconds(1f);
            for (int i = 0; i < starCount; i++)
            {                
                float fElapsed = 0.0f;
                float fDuration = FADESPEED;
                while (fElapsed < fDuration)
                {
                    fElapsed += Time.deltaTime;
                    images_starFill[i].color = Color.Lerp(startColor, endColor, (fElapsed / fDuration));
                    yield return null;
                }
                audioManager.PlaySound(audioManager.audio_star);
                yield return new WaitForSeconds(0.5f);
            }

            // If new level unlocked: save next level
            if(levelsController.CurrentLevel == levelsController.HighestLevel)
                levelsController.SaveLevel(levelsController.CurrentLevel + 1);            
        }
        // Lose scenario
        else if(_type == GameController.EndGameTypes.LOSE)
        {
            text_results.text = "TIME'S UP!";
            audioManager.PlaySound(audioManager.audio_lose);
        }
        button_resultsNext.gameObject.SetActive(true);
    }

    private void HidePanelResults()
    {
        fader_Results.FadeOut(FADESPEED);
    }    

    public void OnResultsNextClicked()
    {
        HideActivePanel();

        // Show map: animate character if progressing to new level
        if (levelsController.CurrentLevel == levelsController.HighestLevel)
            gameController.ShowMap(true, false);
        else
            gameController.ShowMap(false, false);
    }    
    
    private void ResetPanelResults()
    {
        // Hide next button
        button_resultsNext.gameObject.SetActive(false);

        // Reset star color to full alpha
        Color color = images_starFill[0].color;
        color.a = 0f;
        for (int i = 0; i < images_starFill.Length; i++)
            images_starFill[i].color = color;
    }
    #endregion


    #region PLAY LEVEL
    [SerializeField]
    private CanvasFader fader_playLevel;
    [SerializeField]
    private Text text_playLevel;

    private void ShowPanelPlayLevel()
    {
        buttonsController.DisableAllButtonsExcept(fader_playLevel.transform);
        text_playLevel.text = "Level " + (mapsController.GetCharacterLevel());
        fader_playLevel.FadeIn(FADESPEED);
    }

    private void HidePanelPlayLevel()
    {
        fader_playLevel.FadeOut(FADESPEED);
    }
    
    public void OnPlayLevelClicked()
    {
        gameController.StopGame(GameController.EndGameTypes.NONE);
        HideActivePanel();
        gameController.HideMap(false);
        levelsController.LoadLevel();
    }
    #endregion


    #region COUNTDOWN
    private Coroutine CR_ShowCountdown;
    public void ShowCountdown()
    {
        if (CR_ShowCountdown != null)
            StopCoroutine(CR_ShowCountdown);
        CR_ShowCountdown = StartCoroutine(ShowCountdownCR());
    }

    private IEnumerator ShowCountdownCR()
    {
        int timeLeft = 5;
        while (timeLeft > 0)
        {
            if (timeLeft > 1)
                ShowStatusText(timeLeft.ToString(), Color.green);
            else
                ShowStatusText(timeLeft.ToString(), Color.green, 1f);
            timeLeft--;
            audioManager.PlaySound(audioManager.audio_countdown);
            yield return new WaitForSeconds(1f);
        }
    }

    public void HideCountdown()
    {
        if (CR_ShowCountdown != null)
        {
            StopCoroutine(CR_ShowCountdown);
            CR_ShowCountdown = null;
        }

        HideStatusText(0f);
    }
    #endregion


    #region SETTINGS
    [SerializeField]
    private CanvasFader fader_settings;
    [SerializeField]
    private GameObject button_settings;
    [SerializeField]
    private GameObject button_settingsMap;
    [SerializeField]
    private Text text_buttonMusic;

    public void OnSettingsButtonClicked()
    {
        ShowPanel(PanelTypes.SETTINGS);
    }

    private void ShowPanelSettings()
    {
        if (!MapsController.Instance.IsMapShowing)
            gameController.PauseGame();

        // If settings panel was opened from Main Menu: Do not show MAPS button
        if (activePanel == PanelTypes.MAIN_MENU)
            button_settingsMap.SetActive(false);
        else
            button_settingsMap.SetActive(true);

        buttonsController.DisableAllButtonsExcept(fader_settings.transform);
        fader_settings.FadeIn(FADESPEED);
    }

    private void HidePanelSettings()
    {
        if(!MapsController.Instance.IsMapShowing)
            gameController.UnpauseGame();

        fader_settings.FadeOut(FADESPEED);
    }

    public void ShowSettingsButton()
    {
        button_settings.SetActive(true);
    }

    public void HideSettingsButton()
    {
        button_settings.SetActive(false);
    }
    #endregion


    #region LOGIN
    [SerializeField]
    private CanvasFader fader_LoginPanel;

    public void ShowPanelLogin()
    {
        // reset ActivePanel to Login
        fader_LoginPanel.GetComponent<LoginPanel>().ResetStatus();

        fader_LoginPanel.FadeIn(FADESPEED);
        buttonsController.DisableAllButtonsExcept(fader_LoginPanel.transform);
    }

    public void HidePanelLogin()
    {
        fader_LoginPanel.FadeOut(FADESPEED);
    }

    public void OnClickCloseLoginPanel()
    {
        ShowPanel(PanelTypes.SETTINGS);
    }
    #endregion


    #region MUSIC
    public void OnToggleMusic()
    {
        if (audioManager.IsPlayingMusic)
            PauseMusic();
        else
            UnpauseMusic();
    }

    private void LoadMusicSettings()
    {
        if (PlayerPrefs.HasKey("Music"))
        {
            if (PlayerPrefs.GetInt("Music") == 0)
                PauseMusic();
            else if (PlayerPrefs.GetInt("Music") == 1)
                UnpauseMusic();
        }
        else
        {
            audioManager.PlayMusic();
        }
    }

    private void PauseMusic()
    {
        text_buttonMusic.text = "MUSIC OFF";
        audioManager.PauseMusic();
        PlayerPrefs.SetInt("Music", 0);
    }

    private void UnpauseMusic()
    {
        text_buttonMusic.text = "MUSIC ON";
        audioManager.UnpauseMusic();
        PlayerPrefs.SetInt("Music", 1);
    }    
    #endregion


    #region SHOW/HIDE MAP
    [SerializeField]
    private GameObject button_closeMap;
    public void ShowCloseMapButton()
    {
        button_closeMap.SetActive(true);
    }

    public void HideCloseMapButton()
    {
        button_closeMap.SetActive(false);
    }

    public void OnMapShowButtonClicked()
    {
        gameController.ShowMap(false, true);
    }

    public void OnMapHideButtonClicked()
    {        
        gameController.HideMap(true);
    }
    #endregion    

    public void ShowHUD()
    {
        if (!gameController.IsGameRunning || gameController.IsGamePaused)
            return;

        buttons_top.SetActive(true);
        buttons_zoom.SetActive(true);
        buttons_rotatePlatform.SetActive(true);
    }

    public void HideHUD()
    {
        HideActivePanel();        
        HideCorrector();

        buttons_top.SetActive(false);
        buttons_zoom.SetActive(false);
        buttons_rotatePlatform.SetActive(false);

        ShowStatusText("", Color.red);
    }    


    #region HINT BUTTON
    [Header("Hint Button")]
    [SerializeField]
    private Image image_hintFill;
    [SerializeField]
    private float hintBlinkRate = 0.4f;

    private Coroutine CR_OnHintClicked;
    private Coroutine CR_RechargeHint;
    private const float HINT_RECHARGE_TIME = 10f;
    private Color color_hintAlpha;
    private Color color_hintFilled;

    public void OnHintClicked()
    {
        if (!gameController.IsGameRunning || gameController.IsGamePaused || image_hintFill.fillAmount < 1f)
            return;

        // Stop hint recharge
        if (CR_OnHintClicked != null)
        {
            StopCoroutine(CR_OnHintClicked);
            CR_OnHintClicked = null;
        }

        RechargeHint();
        CR_OnHintClicked = StartCoroutine(OnHintClickedCR());
    }

    private IEnumerator OnHintClickedCR()
    {
        meshCombiner.Show();
        yield return new WaitForSeconds(3f);
        meshCombiner.Hide();
    }    

    private void RechargeHint(float _rechargeTime = HINT_RECHARGE_TIME)
    {
        ClearHint();
        CR_RechargeHint = StartCoroutine(RechargeHintCR(_rechargeTime));
    }

    private IEnumerator RechargeHintCR(float _rechargeTime)
    {
        float fElapsed = 0f;
        float fDuration = _rechargeTime;
        while (fElapsed <= fDuration)
        {
            fElapsed += Time.deltaTime;
            image_hintFill.fillAmount = Mathf.Lerp(0f, 1f, fElapsed / fDuration);
            yield return null;
        }

        // Blink
        while (true)
        {
            fElapsed = 0f;
            fDuration = hintBlinkRate;

            // Fade to alpha
            while (fElapsed <= fDuration)
            {
                fElapsed += Time.deltaTime;
                image_hintFill.color = Color.Lerp(color_hintFilled, color_hintAlpha, fElapsed / fDuration);
                yield return null;
            }

            // Fade to opaque
            fElapsed = 0f;
            while (fElapsed <= fDuration)
            {
                fElapsed += Time.deltaTime;
                image_hintFill.color = Color.Lerp(color_hintAlpha, color_hintFilled, fElapsed / fDuration);
                yield return null;
            }

            yield return null;
        }
    }

    public void ClearHint()
    {
        // Stop coroutines
        if (CR_OnHintClicked != null)
        {
            StopCoroutine(CR_OnHintClicked);
            CR_OnHintClicked = null;
        }
        if (CR_RechargeHint != null)
        {
            StopCoroutine(CR_RechargeHint);
            CR_RechargeHint = null;
        }

        image_hintFill.fillAmount = 0f;
        image_hintFill.color = color_hintFilled;
    }
    #endregion


    #region SHOW STATUS TEXT
    public Color Color_statusText;

    public void ShowConsoleText(string text)
    {
        text_console.text += "\n" + text;
    }

    public void ShowStatusText(string text, Color color, float hideDelay = 0f)
    {
        text_status.text = text;
        text_status.color = color;

        // Stop hide status coroutine
        if (CR_HideStatusText != null)
        {
            StopCoroutine(CR_HideStatusText);
            CR_HideStatusText = null;
        }

        // Hide text
        if (hideDelay > 0f)
            HideStatusText(hideDelay);
    }

    public void HideStatusText(float _delay)
    {
        // Stop hide status coroutine
        if (CR_HideStatusText != null)
        {
            StopCoroutine(CR_HideStatusText);
            CR_HideStatusText = null;
        }

        // Hide message after delay
        CR_HideStatusText = StartCoroutine(HideStatusTextCR(_delay));
    }

    private Coroutine CR_HideStatusText;
    private IEnumerator HideStatusTextCR(float _delay)
    {
        yield return new WaitForSeconds(_delay);
        ShowStatusText("", Color.red);
    }
    #endregion
    

    public void ShowCorrector()
    {
        image_corrector.gameObject.SetActive(true);
    }

    public void HideCorrector()
    {
        image_corrector.gameObject.SetActive(false);
    }
}
