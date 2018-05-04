using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TimerController : MonoBehaviour
{
    [SerializeField]
    private float totalTime = 180f;
    [SerializeField]
    private GameObject panel_timer;
    [SerializeField]
    private Image image_timer;
    [SerializeField]
    private Text text_timer;

    private GameController gameController;
    private float timeLeft;
    private Coroutine CR_Timer;

    private void Start()
    {
        gameController = GameController.Instance;
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

    private void OnGameStarted(string statusText)
    {        
        StartTimer();
    }

    private void OnGameEnded(string statusText)
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
        panel_timer.SetActive(true);
        SetTimerText(totalTime);

        timeLeft = totalTime;
        image_timer.fillAmount = 1f;

        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            SetTimerText(timeLeft);
            image_timer.fillAmount = (timeLeft / totalTime);
            yield return null;
        }

        // Stop game
        gameController.StopGame("TIME'S UP!");
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

        panel_timer.SetActive(false);
        timeLeft = 0;
        image_timer.fillAmount = 0f;
        text_timer.text = "";
    }
}
