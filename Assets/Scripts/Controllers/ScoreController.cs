using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreController : Singleton<ScoreController>
{    
    public int HighScore { get; private set; }
    public int CurrentScore { get; private set; }

    private TimerController timerController;
    private MapsController mapsController;
    private LevelsController levelsController;
    private LoginController loginController;

    void Start()
    {
        timerController = TimerController.Instance;
        mapsController = MapsController.Instance;
        levelsController = LevelsController.Instance;
        loginController = LoginController.Instance;
        HighScore = 0;
        CurrentScore = 0;
    }

    private void OnEnable()
    {        
        GameController.OnGameStarted += OnGameStarted;
        GameController.OnGameEnded += OnGameEnded;
    }

    private void OnDisable()
    {
        GameController.OnGameStarted -= OnGameStarted;
        GameController.OnGameEnded -= OnGameEnded;
    }

    private void OnGameStarted()
    {
        CurrentScore = 0;
    }

    private void OnGameEnded(GameController.EndGameTypes _endGameType)
    {
        if (_endGameType == GameController.EndGameTypes.WIN)
        {            
            CalculateScore();
            SaveScore();
        }
    }

    private void CalculateScore()
    {
        CurrentScore = (int)timerController.TimeLeft * 10;
    }    

    public void SaveScore()
    {
        // Save new high score for current level
        if (CurrentScore > HighScore)
        {            
            int level = levelsController.CurrentLevel;

            // Save score locally and online (if user is logged in)
            SaveLocalScore(level, CurrentScore);
            SaveOnlineScore(loginController.Email, level, CurrentScore);
        }
    }        

    public void LoadHighScore(int level)
    {
        GetHighScore(level, CompareHighscores);
    }

    public void GetHighScore(int level, Action<int, int> callback = null)
    {
        if (loginController.isLoggedIn)
            GetOnlineScore(loginController.Email, level, callback);
        else
            HighScore = GetScoreLocal(level);
    }


    #region LOCAL SCORE
    public int GetScoreLocal(int level)
    {
        if (PlayerPrefs.HasKey("HighScore_" + level))
            return PlayerPrefs.GetInt("HighScore_" + level);

        return 0;
    }

    private void SaveLocalScore(int level, int score)
    {
        PlayerPrefs.SetInt("HighScore_" + level, score);
        HighScore = score;
    }
    #endregion


    #region LOCAL SCORE
    private void GetOnlineScore(string email, int level, Action<int, int> callback)
    {
        StartCoroutine(GetOnlineScoreCR(email, level, callback));
    }

    private IEnumerator GetOnlineScoreCR(string email, int level, Action<int, int> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("auth_type", (int)DB.ScoreAuthTypes.GET_HIGHSCORE);
        form.AddField("email", email);
        form.AddField("level", level);

        using (WWW www = new WWW(DB.URL_SCORE, form))
        {
            yield return www;

            if (string.IsNullOrEmpty(www.error))
            {
                // Retrieve high score
                int score = int.Parse(www.text);
                Debug.Log("High score retrieved for level " + level + ": " + score);
                if (callback != null)
                    callback(level, score);
            }
            else
            {
                Debug.LogWarning(www.error);
            }
        }
    }

    private void SaveOnlineScore(string email, int level, int score)
    {
        if (!loginController.isLoggedIn)
            return;

        StartCoroutine(SaveOnlineScoreCR(email, level, score));
    }

    private IEnumerator SaveOnlineScoreCR(string email, int level, int score)
    {
        WWWForm form = new WWWForm();
        form.AddField("auth_type", (int)DB.ScoreAuthTypes.SAVE_HIGHSCORE);
        form.AddField("email", email);
        form.AddField("level", level);
        form.AddField("score", score);

        using (WWW www = new WWW(DB.URL_SCORE, form))
        {
            yield return www;

            if (string.IsNullOrEmpty(www.error))
            {
                // Score saved successfully
                Debug.Log("High score saved for level " + level + ": " + score);
            }
            else
            {
                Debug.LogWarning(www.error);
            }
        }
    }
    #endregion


    private void CompareHighscores(int level, int onlineScore)
    {
        // Compare highScore from PlayerPrefs vs DB
        // NOTE: This is a callback function that executes after retrieving highscore from DB
        int localScore = GetScoreLocal(level);
        if (localScore > onlineScore)
        {
            print("LOCAL SCORE IS HIGHER... SAVING TO DB: " + localScore + " > " + onlineScore);
            // Update DB
            SaveOnlineScore(loginController.Email, level, localScore);
        }
        else if (onlineScore > localScore)
        {
            print("ONLINE SCORE IS HIGHER... SAVING LOCALLY: " + onlineScore + " > " + localScore);
            // Update playerprefs
            SaveLocalScore(level, onlineScore);
        }
    }
}
