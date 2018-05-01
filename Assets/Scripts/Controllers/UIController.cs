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
    private GameObject Panel_RotatePlatform;

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
        Panel_RotatePlatform.SetActive(true);

        if (CR_HideText != null)
            StopCoroutine(CR_HideText);
        CR_HideText = StartCoroutine(HideStatusTextCR());
    }

    private void OnGameEnded(bool showStatusText)
    {
        if (!showStatusText)
            return;

        text_status.text = "GAME OVER!";
        Panel_RotatePlatform.SetActive(false);

        if (CR_HideText != null)
            StopCoroutine(CR_HideText);
        CR_HideText = StartCoroutine(HideStatusTextCR());
    }

    private IEnumerator HideStatusTextCR()
    {
        yield return new WaitForSeconds(1f);
        text_status.text = "";
    }    
}
