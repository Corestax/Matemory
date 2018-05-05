using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIController : Singleton<UIController>
{
    [SerializeField]
    private CanvasFader fader_complete;
    [SerializeField]
    private Text text_status;
    [SerializeField]
    private Text text_console;
    [SerializeField]
    private Text text_pinchZoom;
    [SerializeField]
    private GameObject buttons_zoom;
    [SerializeField]
    private GameObject buttons_rotatePlatform;

    private GameController gameController;
    private TimerController timeController;
    private AudioManager audioManager;
    private Coroutine CR_HideText;

    [Serializable] public enum PANELTYPES { NONE, COMPLETE }
    private PANELTYPES activePanel;

    private const float FADESPEED = 0.25f;

    void Start()
    {
        gameController = GameController.Instance;
        timeController = TimerController.Instance;
        audioManager = AudioManager.Instance;
        activePanel = PANELTYPES.NONE;
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

    public void ShowPanel(PANELTYPES panel)
    {
        // Hide last active panel
        HidePanel(activePanel);

        // Show new panel        
        switch (panel)
        {
            case PANELTYPES.NONE:
                break;

            case PANELTYPES.COMPLETE:
                ShowPanelResults();
                break;

            default:
                break;
        }
        activePanel = panel;
    }    

    public void HidePanel(PANELTYPES panel)
    {        
        switch (panel)
        {
            case PANELTYPES.COMPLETE:
                fader_complete.FadeOut(FADESPEED);
                break;

            default:
                break;
        }
        activePanel = PANELTYPES.NONE;
    }

    #region PANEL RESULTS
    [SerializeField]
    private Image[] images_starFill;
    [SerializeField]
    private Button button_resultsNext;

    private void ShowPanelResults()
    {
        ResetPanelResults();
        StartCoroutine(ShowPanelResultsCR());
    }

    private IEnumerator ShowPanelResultsCR()
    {
        // Fade in panel
        fader_complete.FadeIn(FADESPEED);
        while (fader_complete.IsFading)
            yield return null;

        // Win scenario
        if (timeController.TimeLeft > 0)
        {
            audioManager.PlaySound(audioManager.audio_win);
            float percentUsed = (timeController.TimeLeft / timeController.TimeTotal) * 100f;

            // Fade in star alpha color
            Color startColor = images_starFill[0].color;
            Color endColor = startColor;
            endColor.a = 1f;

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

            // Show stars
            for (int i = 0; i < starCount; i++)
            {
                images_starFill[i].color = Color.Lerp(startColor, endColor, FADESPEED);
                yield return new WaitForSeconds(FADESPEED + 1f);
            }
        }
        // Lose scenario
        else
        {
            audioManager.PlaySound(audioManager.audio_lose);
        }
        button_resultsNext.gameObject.SetActive(true);
    }

    public void OnResultsNextClicked()
    {
        HidePanel(activePanel);

        gameController.Restart();
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

    private void OnGameStarted(string statusText)
    {
        //if (string.IsNullOrEmpty(statusText))
        //    return;

        ShowUI();

        text_status.text = statusText;
        if (CR_HideText != null)
            StopCoroutine(CR_HideText);
        CR_HideText = StartCoroutine(HideStatusTextCR());
    }

    private void OnGameEnded(string statusText)
    {
        //if (string.IsNullOrEmpty(statusText))
        //    return;

        HideUI();
        ShowPanel(PANELTYPES.COMPLETE);

        text_status.text = statusText;
        if (CR_HideText != null)
            StopCoroutine(CR_HideText);
        CR_HideText = StartCoroutine(HideStatusTextCR());
    }

    private IEnumerator HideStatusTextCR()
    {
        yield return new WaitForSeconds(1f);
        text_status.text = "";
    }    

    public void ShowConsoleText(string text)
    {
        text_console.text += "\n" + text;
    }
}
