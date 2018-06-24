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

    private MapsController mapsController;
    private ModelsController modelsController;
    private ScoreController scoreController;
    public int CurrentLevel = 0;
    public int HighestLevel = 0;

	void Start ()
    {
        mapsController = MapsController.Instance;
        modelsController = ModelsController.Instance;
        scoreController = ScoreController.Instance;

        // Populate dictionary of levels
        Levels = new Dictionary<int, ModelsController.ModelTypes>();
        foreach (var item in LevelObjects)
            Levels.Add(item.Level, item.Type);
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
        LoadLevel(mapsController.GetCharacterLevel());
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
        mapsController.SetCharacterLevel(level);        

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

        // Load high score
        scoreController.LoadHighScore(level);

        // Save level
        PlayerPrefs.SetInt("Level", level);
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
