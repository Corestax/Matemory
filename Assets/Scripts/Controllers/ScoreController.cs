using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ScoreMap
{
    public int Level;
    public List<int> HighScores;
}

public class ScoreController : Singleton<ScoreController>
{    
    // Store high scores for each level independently of maps
    // Key: MapIndex        Value: Level, Highscore
    //private Dictionary<int , Dictionary<int, int>> Scores;   
    //public Dictionary<int, int> HighScores;

    public int HighScore { get; private set; }
    public int CurrentScore { get; private set; }

    private TimerController timerController;
    private ModelsController modelsController;

    void Start()
    {
        timerController = TimerController.Instance;
        modelsController = ModelsController.Instance;
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
        //print(CurrentScore);
    }

    public void SaveScore()
    {
        // Save new high score for current level
        if (CurrentScore > HighScore)
        {
            int level = LevelsController.Instance.CurrentLevel;
            PlayerPrefs.SetInt("HighScore_" + level, CurrentScore);
            HighScore = CurrentScore;
            print("New high score for level " + level + ": " + HighScore);
        }
    }

    public void LoadHighScore()
    {
        // Retrieve last saved score for current level
        HighScore = 0;
        int level = LevelsController.Instance.CurrentLevel;
        if (PlayerPrefs.HasKey("HighScore_" + level))
        {
            HighScore = PlayerPrefs.GetInt("HighScore_" + level);
            print("Loaded previously saved score for level " + level + ": " + HighScore);
        }
    }
}
