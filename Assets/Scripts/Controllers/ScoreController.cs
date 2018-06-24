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
            SaveOnlineScore(loginController.UserName, level, CurrentScore);
        }
    }        

    public void LoadHighScore(int level)
    {
        GetHighScore(level, CompareHighscores);
    }

    public void GetHighScore(int level, Action<int, int> callback = null)
    {
        if (loginController.isLoggedIn)
            GetOnlineScore(loginController.UserName, level, callback);
        else
            GetLocalScore(level);
    }


    #region LOCAL SCORE
    public int GetLocalScore(int level)
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
    private void GetOnlineScore(string userName, int level, Action<int, int> callback)
    {
        StartCoroutine(GetOnlineScoreCR(loginController.UserName, level, callback));
    }

    private IEnumerator GetOnlineScoreCR(string userName, int level, Action<int, int> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("auth_type", (int)DB.UserAuthTypes.GET_HIGHSCORE);
        form.AddField("username", userName);
        form.AddField("level", level);

        using (WWW www = new WWW(DB.URL_USER, form))
        {
            yield return www;

            if (!string.IsNullOrEmpty(www.error))
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

    private void SaveOnlineScore(string userName, int level, int score)
    {
        if (!loginController.isLoggedIn)
            return;

        StartCoroutine(SaveOnlineScoreCR(loginController.UserName, level, score));
    }

    private IEnumerator SaveOnlineScoreCR(string userName, int level, int score)
    {
        WWWForm form = new WWWForm();
        form.AddField("auth_type", (int)DB.UserAuthTypes.SAVE_HIGHSCORE);
        form.AddField("username", userName);
        form.AddField("level", level);
        form.AddField("score", score);

        using (WWW www = new WWW(DB.URL_USER, form))
        {
            yield return www;

            if (!string.IsNullOrEmpty(www.error))
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
        int localScore = GetLocalScore(level);
        if (localScore > onlineScore)
        {
            // Update DB
            SaveOnlineScore(loginController.UserName, level, localScore);
        }
        else if (onlineScore > localScore)
        {
            // Update playerprefs
            SaveLocalScore(level, onlineScore);
        }
    }
}
