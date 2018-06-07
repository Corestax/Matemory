using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialsController : Singleton<TutorialsController>
{
    [SerializeField]
    private CanvasFader[] faders;

    private bool[] IsCompleted;

    private int currentIndex = -1;
    private float fadeSpeed = 0.25f;

    public bool IsActive { get; private set; }
    public bool IsFading { get; private set; }

    private ButtonsController buttonsController;

    private void Start()
    {
        buttonsController = ButtonsController.Instance;
        IsCompleted = new bool[faders.Length];
    }

    private void OnEnable()
    {
        GameController.OnGameEnded += OnGameEnded;
    }

    private void OnDisable()
    {
        GameController.OnGameEnded -= OnGameEnded;
    }

    private void OnGameEnded(GameController.EndGameTypes _type)
    {
        currentIndex = -1;
        StopAllCoroutines();
        StartCoroutine(HideActiveTutorialCR());
    }

    public void ShowTutorial(int index)
    {
        if (currentIndex == index || index >= faders.Length)
            return;

        // Skip tutorial and start game      
        if (index > 0 && IsCompleted[index] == true)
        {
            currentIndex = index;
            if (index == 1)
            {
                // Rotate platform
                ModelsController.Instance.RotatePlatformAndExplode();
                ShowCountdown();
            }
            else
            {
                // Check if all tutorials marked as complete
                int count = 0;
                foreach(var c in IsCompleted)
                {
                    if (c == true)
                        count++;
                }

                // Start game
                if (count == IsCompleted.Length)
                    GameController.Instance.StartGame();
            }
            return;
        }
        
        buttonsController.DisableAllButtons();
        StopAllCoroutines();
        StartCoroutine(ShowTutorialCR(index));
    }

    private IEnumerator ShowTutorialCR(int index)
    {
        IsActive = true; 
        IsFading = true;
        IsCompleted[index] = true;

        // Fade out previous tutorial
        yield return StartCoroutine(HideActiveTutorialCR());

        // Fade in next tutorial
        currentIndex = index;
        faders[index].FadeIn(fadeSpeed);
        while (faders[index].IsFading)
            yield return null;
        IsFading = false;        
    }

    private IEnumerator HideActiveTutorialCR()
    {
        if (currentIndex != -1)
        {            
            if (faders[currentIndex].Alpha > 0f)
                faders[currentIndex].FadeOut(fadeSpeed);
            while (faders[currentIndex].IsFading)
                yield return null;
        }
    }

    public void OnNextClicked()
    {
        StopAllCoroutines();
        StartCoroutine(OnNextClickedCR());
    }    

    private IEnumerator OnNextClickedCR()
    {
        yield return StartCoroutine(HideActiveTutorialCR());
        buttonsController.EnableAllButtons();
        IsActive = false;

        // Place platform
        if (currentIndex == 0)
        {
#if UNITY_EDITOR
            LevelsController.Instance.LoadLastSavedLevel();
#endif
        }
        // Memorize
        else if (currentIndex == 1)
        {
            ModelsController.Instance.RotatePlatformAndExplode();
            ShowCountdown();
        }
        // Pinch zoom
        else if (currentIndex == 2)
        {
            ShowTutorial(3);
        }
        // Spin
        else if (currentIndex == 3)
        {
            ShowTutorial(4);
        }
        // Drag fruit
        else if (currentIndex == 4)
        {
            GameController.Instance.StartGame();
        }
    }

    private void ShowCountdown()
    {
        StartCoroutine(ShowCountdownCR());
    }

    private IEnumerator ShowCountdownCR()
    {
        int timeLeft = 5;
        while (timeLeft > 0)
        {
            if(timeLeft > 1)
                UIController.Instance.ShowStatusText(timeLeft.ToString(), Color.green);
            else
                UIController.Instance.ShowStatusText(timeLeft.ToString(), Color.green, 1f);
            timeLeft--;
            yield return new WaitForSeconds(1f);
        }
    }
}
