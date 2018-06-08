using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelsController : Singleton<LevelsController>
{
    [Serializable]
    public struct LevelObject
    {
        public int Level;
        public ModelsController.ModelTypes Type;
    }

    [SerializeField]
    private LevelObject[] LevelObjects;

    public Dictionary<int, ModelsController.ModelTypes> Levels { get; private set; }

    private ModelsController modelsController;
    public int CurrentLevel = 0;
    public int HighestLevel = 0;

	void Start ()
    {
        modelsController = ModelsController.Instance;

        // Populate dictionary of levels
        Levels = new Dictionary<int, ModelsController.ModelTypes>();
        foreach (var item in LevelObjects)
        {
            string name = item.Type.ToString();
            Levels.Add(item.Level, item.Type);
        }
    }

    #region LOAD LEVEL
    public void LoadLastSavedLevel()
    {
        int level = GetSavedLevel();
        if (level != -1)
            LoadLevel(level);
        else
            LoadFirstLevel();
    }

    public void LoadFirstLevel()
    {
        LoadLevel(1);
    }

    public void LoadLevel()
    {
        LoadLevel(CurrentLevel);
    }

    public void LoadLevel(int level)
    {
        if (level-1 >= Levels.Count)
            return;

        // Update level & high score
        CurrentLevel = level;

        // Save highest level
        if (CurrentLevel > HighestLevel)
            HighestLevel = CurrentLevel;

        // Set character position in map
        MapsController.Instance.SetCharacterPosition(level);
        ScoreController.Instance.LoadHighScore();

        // Spawn model
        ModelsController.ModelTypes type = Levels[level];
        modelsController.Spawn(type);
    }
    #endregion


    #region PLAYERPREFS    
    public void SaveLevel(int level)
    {
        if (level <= HighestLevel)
            return;

        // Update levels
        CurrentLevel = level;
        HighestLevel = CurrentLevel;

        // Save level
        PlayerPrefs.SetInt("Level", level);

        ScoreController.Instance.LoadHighScore();
    }

    private int GetSavedLevel()
    {
        // Retrieve last saved level
        if (PlayerPrefs.HasKey("Level"))
            return PlayerPrefs.GetInt("Level");
        else
            return -1;
    }
    #endregion
}
