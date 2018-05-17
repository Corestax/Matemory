using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIController : Singleton<UIController>
{    
    [SerializeField]
    private CanvasFader fader_models;
    [SerializeField]
    private Text text_console;
    [SerializeField]
    private Text text_status;
    [SerializeField]
    private Text text_pinchZoom;
    [SerializeField]
    private GameObject buttons_zoom;
    [SerializeField]
    private GameObject buttons_rotatePlatform;    

    private GameController gameController;
    private ModelsController modelsController;
    private TimerController timeController;
    private AudioManager audioManager;
    private MeshCombiner meshCombiner;

    public enum PanelTypes { NONE, MODELS, RESULTS }
    private PanelTypes activePanel;

    private const float FADESPEED = 0.25f;

    void Start()
    {
        gameController = GameController.Instance;
        modelsController = ModelsController.Instance;
        timeController = TimerController.Instance;
        audioManager = AudioManager.Instance;
        meshCombiner = MeshCombiner.Instance;
        activePanel = PanelTypes.NONE;
    }

    void OnEnable()
    {
        GameController.OnGameStarted += OnGameStarted;
        GameController.OnGameEnded += OnGameEnded;
        ARCoreController.OnTrackingActive += ShowUI;
        ARCoreController.OnTrackingLost += HideUI;
    }

    void OnDisable()
    {

        GameController.OnGameStarted -= OnGameStarted;
        GameController.OnGameEnded -= OnGameEnded;
        ARCoreController.OnTrackingActive -= ShowUI;
        ARCoreController.OnTrackingLost -= HideUI;
    }

    private void OnGameStarted()
    {
        ShowUI();
        RechargeHint(0f);        
    }

    private void OnGameEnded(GameController.EndGameTypes _type)
    {
        HideUI();
        ClearHint();
        ShowPanel(PanelTypes.RESULTS, _type);
    }

    public void ShowPanel(PanelTypes panel, GameController.EndGameTypes _type = GameController.EndGameTypes.NONE)
    {
        // Hide last active panel
        HidePanel(activePanel);

        // Show new panel        
        switch (panel)
        {
            case PanelTypes.NONE:
                break;

            case PanelTypes.MODELS:
                ShowPanelModels();
                break;

            case PanelTypes.RESULTS:
                ShowPanelResults(_type);
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
            case PanelTypes.MODELS:
                HidePanelModels();
                break;

            case PanelTypes.RESULTS:
                HidePanelResults();
                break;

            default:
                break;
        }
        activePanel = PanelTypes.NONE;
    }

    public void HideActivePanel()
    {
        HidePanel(activePanel);
    }


    #region PANEL MODELS
    public void OnClosePanelModelsClicked()
    {
        HidePanel(PanelTypes.MODELS);
    } 

    public void TogglePanelModels()
    {
        if (fader_models.Alpha == 0f)
            ShowPanel(PanelTypes.MODELS);
        else
            HidePanel(PanelTypes.MODELS);
    }

    private void ShowPanelModels()
    {
        fader_models.FadeIn(FADESPEED);
    }

    private void HidePanelModels()
    {
        fader_models.FadeOut(FADESPEED);
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
        // Fade in panel
        fader_Results.FadeIn(FADESPEED);
        while (fader_Results.IsFading)
            yield return null;

        // Win scenario
        if (_type == GameController.EndGameTypes.WIN)
        {
            audioManager.PlaySound(audioManager.audio_win);
            text_results.text = "COMPLETE!";

            float percentUsed = (timeController.TimeLeft / timeController.TimeTotal) * 100f;            

            int starCount = 0;
            // 1 Star
            if (percentUsed >= 80.0f)
                starCount = 1;
            // 2 Stars
            else if (percentUsed >= 60.0f)
                starCount = 2;
            // 3 Stars
            else
                starCount = 3;

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
        HidePanel(activePanel);
        ShowPanel(PanelTypes.MODELS);
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


    private void ShowUI()
    {
        if (!gameController.IsGameRunning)
            return;

        buttons_rotatePlatform.SetActive(true);
        if (gameController.EnableAR)
            text_pinchZoom.gameObject.SetActive(true);
        else
            buttons_zoom.SetActive(true);
    }

    private void HideUI()
    {
        buttons_rotatePlatform.SetActive(false);
        if (gameController.EnableAR)
            text_pinchZoom.gameObject.SetActive(false);
        else
            buttons_zoom.SetActive(false);
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

    public void OnHintClicked()
    {
        if (!gameController.IsGameRunning || image_hintFill.fillAmount < 1f)
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
        if (CR_RechargeHint != null)
            StopCoroutine(CR_RechargeHint);

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
            Color col_filled = image_hintFill.color;
            col_filled.a = 1f;
            Color col_alpha = col_filled;
            col_alpha.a = 0f;

            while (fElapsed <= fDuration)
            {
                fElapsed += Time.deltaTime;
                image_hintFill.color = Color.Lerp(col_filled, col_alpha, fElapsed / fDuration);
                yield return null;
            }

            fElapsed = 0f;
            while (fElapsed <= fDuration)
            {
                fElapsed += Time.deltaTime;
                image_hintFill.color = Color.Lerp(col_alpha, col_filled, fElapsed / fDuration);
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

        Color color = image_hintFill.color;
        color.a = 0f;
        image_hintFill.color = color;
    }
    #endregion


    public void ShowStatusText(string text, Color color)
    {
        text_status.text = text;
        text_status.color = color;
    }

    public void ShowConsoleText(string text)
    {
        text_console.text += "\n" + text;
    }
}
