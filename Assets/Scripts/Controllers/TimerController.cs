using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TimerController : Singleton<TimerController>
{
    public float TimeTotal = 180f;

    [SerializeField]
    private GameObject panel_timer;
    [SerializeField]
    private Image image_timer;
    [SerializeField]
    private Text text_timer;

    public float TimeLeft { get; private set; }

    private GameController gameController;
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

    private void OnGameStarted()
    {        
        StartTimer();
    }

    private void OnGameEnded(GameController.EndGameTypes _type)
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
        SetTimerText(TimeTotal);

        TimeLeft = TimeTotal;
        image_timer.fillAmount = 1f;

        while (TimeLeft > 0)
        {
            TimeLeft -= Time.deltaTime;
            SetTimerText(TimeLeft);
            image_timer.fillAmount = (TimeLeft / TimeTotal);
            yield return null;
        }

        // Stop game
        gameController.StopGame(GameController.EndGameTypes.LOSE);
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
        TimeLeft = 0;
        image_timer.fillAmount = 0f;
        text_timer.text = "";
    }
}
