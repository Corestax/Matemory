using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : Singleton<UIController>
{
    [SerializeField]
    private GameController game;
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

    private Coroutine CR_HideText;

    void Start()
    {

    }

    void Update()
    {

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

    private void ShowUI()
    {
        if (!GameController.Instance.IsGameRunning)
            return;

        buttons_rotatePlatform.SetActive(true);
        if (GameController.Instance.EnableAR)
            text_pinchZoom.gameObject.SetActive(true);
        else
            buttons_zoom.SetActive(true);
    }

    private void HideUI()
    {
        buttons_rotatePlatform.SetActive(false);
        if (GameController.Instance.EnableAR)
            text_pinchZoom.gameObject.SetActive(false);
        else
            buttons_zoom.SetActive(false);
    }

    private void OnGameStarted(bool showStatusText)
    {
        if (!showStatusText)
            return;

        ShowUI();

        text_status.text = "START!";
        if (CR_HideText != null)
            StopCoroutine(CR_HideText);
        CR_HideText = StartCoroutine(HideStatusTextCR());
    }

    private void OnGameEnded(bool showStatusText)
    {
        if (!showStatusText)
            return;

        HideUI();

        text_status.text = "GAME OVER!";
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
