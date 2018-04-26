using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TimerController : Singleton<TimerController>
{
    [SerializeField]
    private GameController game;
    [SerializeField]
    private Image image_timer;
    [SerializeField]
    private Text text_timer;

    private const float TOTAL_TIME = 100f;

    private float timeLeft;
    private Coroutine CR_Timer;

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
        StartTimer();
    }

    private void OnGameEnded(bool showStatusText)
    {
        StopTimer();
    }

    public void StartTimer()
    {
        if (CR_Timer != null)
            StopCoroutine(CR_Timer);

        CR_Timer = StartCoroutine(StartTimerCR());
    }

    private IEnumerator StartTimerCR()
    {
        SetTimerText(TOTAL_TIME);

        timeLeft = TOTAL_TIME;
        image_timer.fillAmount = 1f;

        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            SetTimerText(timeLeft);
            image_timer.fillAmount = (timeLeft / TOTAL_TIME);
            yield return null;
        }

        // Stop game
        game.StopGame();
    }

    private void SetTimerText(float time)
    {
        int intTime = Mathf.CeilToInt(time);
        text_timer.text = string.Format("{0}:{1:d2}", intTime / 60, intTime % 60);
    }

    public void StopTimer()
    {
        if (CR_Timer != null)
        {
            StopCoroutine(CR_Timer);
            CR_Timer = null;
        }

        timeLeft = 0;
        image_timer.fillAmount = 0f;
        text_timer.text = "";
    }
}
