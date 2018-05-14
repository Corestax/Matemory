using UnityEngine;

public class ScoreController : Singleton<ScoreController>
{
    public int HighScore { get; private set; }
    public int CurrentScore { get; private set; }

    private ModelsController modelsController;

    void Start()
    {
        modelsController = ModelsController.Instance;

        HighScore = PlayerPrefs.GetInt("HighScore");
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
        if(_endGameType == GameController.EndGameTypes.WIN)
            SaveScore();
    }

    public void SaveScore()
    {
        // New high score
        if (CurrentScore > HighScore)
        {
            string modelType = modelsController.ActiveModelType.ToString();
            PlayerPrefs.SetInt("HighScore_" + modelType, CurrentScore);
            HighScore = CurrentScore;
            print("New high score!");
        }
    }

    public void AddScore(int _points)
    {
        CurrentScore += _points;
    }
}
