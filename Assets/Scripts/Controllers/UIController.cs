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
    private GameObject RotatePlatformButtons;
    [SerializeField]
    private GameObject ZoomButtons;

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
    }

    void OnDisable()
    {

        GameController.OnGameStarted -= OnGameStarted;
        GameController.OnGameEnded -= OnGameEnded;
    }

    private void OnGameStarted(bool showStatusText)
    {
        if (!showStatusText)
            return;

        text_status.text = "START!";
        RotatePlatformButtons.SetActive(true);
        //ZoomButtons.SetActive(true);

        if (CR_HideText != null)
            StopCoroutine(CR_HideText);
        CR_HideText = StartCoroutine(HideStatusTextCR());
    }

    private void OnGameEnded(bool showStatusText)
    {
        if (!showStatusText)
            return;

        text_status.text = "GAME OVER!";
        RotatePlatformButtons.SetActive(false);
        //ZoomButtons.SetActive(false);

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
