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

    private GameController gameController;
    private ModelsController modelsController;
    private UIController uiController;
    private ButtonsController buttonsController;
    private AudioManager audioManager;
#if UNITY_EDITOR
    private LevelsController levelsController;
#endif

    private void Start()
    {
        gameController = GameController.Instance;
        modelsController = ModelsController.Instance;
        uiController = UIController.Instance;
        buttonsController = ButtonsController.Instance;
        audioManager = AudioManager.Instance;
#if UNITY_EDITOR
        levelsController = LevelsController.Instance;
#endif
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
        if (currentIndex == index || index < 0 || index >= faders.Length)
            return;

        // Skip tutorial and process next action   
        if (index > 0 && GetIsTutorialCompleted(index) == 1)
        {
            currentIndex = index;
            if (index == 1)
            {
                // Rotate platform and show countdown                
                modelsController.RotatePlatformAndExplode();
            }
            else if (index == faders.Length-1)
            {
                // Start game
                gameController.StartGame();
            }
            return;
        }

        uiController.HideActivePanel();
        buttonsController.DisableAllButtons();
        StopAllCoroutines();
        StartCoroutine(ShowTutorialCR(index));
    }

    private IEnumerator ShowTutorialCR(int index)
    {
        IsActive = true; 
        IsFading = true;        
        SaveIsTutorialCompleted(index, true);

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
            levelsController.LoadLastSavedLevel();
#endif
        }
        // Memorize
        else if (currentIndex == 1)        
            modelsController.RotatePlatformAndExplode();        
        // Drag fruit
        else if (currentIndex == 2)        
            gameController.StartGame();        
    }

    #region PLAYERPREFS
    public int GetIsTutorialCompleted(int index)
    {
        string key = "TutorialComplete_" + index;
        if (PlayerPrefs.HasKey(key))
            return PlayerPrefs.GetInt(key);
        else
            return -1;
    }

    private void SaveIsTutorialCompleted(int index, bool state)
    {
        string key = "TutorialComplete_" + index;
        int isComplete = state ? 1 : -1;
        PlayerPrefs.SetInt(key, isComplete);
    }
    #endregion
}
