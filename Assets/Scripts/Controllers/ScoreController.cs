using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreController : Singleton<ScoreController>
{    
    public int HighScore { get; private set; }
    public int CurrentScore { get; private set; }

    private TimerController timerController;
    private MapsController mapsController;
    private GoogleGameServicesController googleGameServices;

    void Start()
    {
        timerController = TimerController.Instance;
        mapsController = MapsController.Instance;
        googleGameServices = GoogleGameServicesController.Instance;
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
            int level = LevelsController.Instance.CurrentLevel;
            PlayerPrefs.SetInt("HighScore_M" + mapsController.MapIndex + "_" + level, CurrentScore);
            HighScore = CurrentScore;
            print("New high score for level " + level + ": " + HighScore);

            // Save Score To Leaderboard
            googleGameServices.AddScoreToLeaderboard(level, CurrentScore);
        }
    }

    public void LoadHighScore()
    {
        HighScore = 0;
        int level = MapsController.Instance.GetCharacterLevel();
        
        // Retrieve last saved score for current level from PlayerPrefs
        if (PlayerPrefs.HasKey("HighScore_M" + mapsController.MapIndex + "_" +  level))
        {
            HighScore = PlayerPrefs.GetInt("HighScore_" + level);
        }
        // or leaderboard
        else
        {
            googleGameServices.GetHighestUserScore(level, (score) =>
            {
                if (score == 0)
                    return;
                
                HighScore = score;
                PlayerPrefs.SetInt("HighScore_M" + mapsController.MapIndex + "_" + level, HighScore);
            });
        }
    }
}
