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
    private int currentLevel = 0;

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
	
	//void Update ()
 //   {
 //       if (Input.GetKeyDown(KeyCode.P))
 //           LoadLastSavedLevel();
 //       if (Input.GetKeyDown(KeyCode.L))
 //           LoadFirstLevel();
 //       if (Input.GetKeyDown(KeyCode.N))
 //           LoadNextLevel();
	//}


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

    public void LoadNextLevel()
    {
        if (currentLevel-1 >= Levels.Count)
            return;

        currentLevel++;
        LoadLevel(currentLevel);
    }

    private void LoadLevel(int level)
    {
        if (level-1 >= Levels.Count)
            return;

        SaveLevel(level);
        currentLevel = level;

        // Spawn model
        ModelsController.ModelTypes type = Levels[level];
        modelsController.Spawn(type, true);
    }
    #endregion


    #region PLAYERPREFS
    private void SaveLevel(int level)
    {
        PlayerPrefs.SetInt("Level", level);
    }

    private int GetSavedLevel()
    {
        if (PlayerPrefs.HasKey("Level"))
            return PlayerPrefs.GetInt("Level");
        else
            return -1;
    }
    #endregion
}
