using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialsController : Singleton<TutorialsController>
{
    [SerializeField]
    private CanvasFader[] faders;

    private int currentIndex = -1;
    private float fadeSpeed = 0.25f;

    public bool IsActive { get; private set; }
    public bool IsFading { get; private set; }

    public void ShowTutorial(int index)
    {
        if (currentIndex == index || index >= faders.Length)
            return;

        StopAllCoroutines();
        StartCoroutine(ShowTutorialCR(index));
    }

    private IEnumerator ShowTutorialCR(int index)
    {
        IsActive = true; 
        IsFading = true;

        // Fade out previous tutorial
        yield return StartCoroutine(HideActiveTutorial());

        // Fade in next tutorial
        currentIndex = index;
        faders[index].FadeIn(fadeSpeed);
        while (faders[index].IsFading)
            yield return null;
        IsFading = false;        
    }

    private IEnumerator HideActiveTutorial()
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
        yield return StartCoroutine(HideActiveTutorial());
        IsActive = false;

        // Place platform
        if (currentIndex == 0)
        {

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
