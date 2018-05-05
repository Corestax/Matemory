using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasFader : MonoBehaviour
{
    public bool IsFading { get { return enabled; } private set { enabled = value; } }
    public float Alpha { get { return fader.alpha; } }

    private CanvasGroup fader;
    private bool isFadeIn;
    private float elapsed;
    private float duration;
    private float endAlpha;
    private float currAlpha;

    public UnityEvent OnFadeOut;

    void Start()
    {
        fader = GetComponent<CanvasGroup>();
        enabled = false;
        elapsed = 0.0f;
        endAlpha = 1f;
        currAlpha = fader.alpha;

        if (Alpha > 0f)
            isFadeIn = true;
    }

    public void FadeIn(float seconds, float _endAlpha = 1f)
    {
        //if (isFadeIn)
        //    return;

        if (!fader)
            fader = GetComponent<CanvasGroup>();

        elapsed = 0.0f;
        duration = seconds;
        currAlpha = fader.alpha;
        endAlpha = _endAlpha;
        enabled = true;
        isFadeIn = true;
    }

    public void FadeOut(float seconds, float _endAlpha = 0f)
    {
        //if (!isFadeIn)
        //    return;

        if (!fader)
            fader = GetComponent<CanvasGroup>();

        elapsed = 0.0f;
        duration = seconds;
        currAlpha = fader.alpha;
        endAlpha = _endAlpha;
        enabled = true;
        isFadeIn = false;
    }

    public void SetAlpha(float alpha)
    {
        if (!fader)
            fader = GetComponent<CanvasGroup>();

        enabled = false;
        fader.alpha = alpha;
        currAlpha = alpha;
        endAlpha = alpha;
        isFadeIn = (alpha > 0f) ? true : false;

        if (alpha == 1f)
        {
            fader.interactable = true;
            fader.blocksRaycasts = true;
        }
        else
        {
            fader.interactable = false;
            fader.blocksRaycasts = false;
        }
    }

    void Update()
    {
        elapsed += Time.deltaTime;

        // Fade
        fader.alpha = Mathf.Lerp(currAlpha, endAlpha, elapsed / duration);

        // Stop
        if (elapsed >= duration)
        {
            elapsed = duration;
            enabled = false;

            if (isFadeIn)
            {
                fader.interactable = true;
                fader.blocksRaycasts = true;
            }
            else
            {
                fader.interactable = false;
                fader.blocksRaycasts = false;

                if (OnFadeOut != null)
                    OnFadeOut.Invoke();
            }
        }
    }
}
