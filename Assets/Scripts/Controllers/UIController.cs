using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIController : Singleton<UIController>
{
    [SerializeField]
    private CanvasFader fader_Results;
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

    [SerializeField]
    private Text text_results;
    [SerializeField]
    private Image[] images_starFill;
    [SerializeField]
    private Button button_resultsNext;

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
            yield return new WaitForSeconds(2f);
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
                yield return new WaitForSeconds(1f);
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

    private Coroutine CR_OnHintClicked;
    public void OnHintClicked()
    {
        if (CR_OnHintClicked != null)
            StopCoroutine(CR_OnHintClicked);

        CR_OnHintClicked = StartCoroutine(OnHintClickedCR());
    }

    private IEnumerator OnHintClickedCR()
    {
        meshCombiner.Show();
        yield return new WaitForSeconds(3f);
        meshCombiner.Hide();
    }

    private void OnGameStarted()
    {
        ShowUI();
    }

    private void OnGameEnded(GameController.EndGameTypes _type)
    {
        HideUI();
        ShowPanel(PanelTypes.RESULTS, _type);
    }

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
