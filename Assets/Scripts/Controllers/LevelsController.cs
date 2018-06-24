﻿using System;
using System.Collections;
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
    private LoginController loginController;
    public int CurrentLevel = 0;
    public int HighestLevel = 0;

	void Start ()
    {
        mapsController = MapsController.Instance;
        modelsController = ModelsController.Instance;
        scoreController = ScoreController.Instance;
        loginController = LoginController.Instance;

        // Populate dictionary of levels
        Levels = new Dictionary<int, ModelsController.ModelTypes>();
        foreach (var item in LevelObjects)
            Levels.Add(item.Level, item.Type);
    }

    #region LOAD LEVEL
    public void LoadLastSavedLevel()
    {
        if (loginController.isLoggedIn)
        {
            GetLevelOnline(loginController.UserName, CompareLevels);
        }
        else
        {
            // Retrieve local level
            int level = GetLevelLocal();

            // Load highest level
            if (level != -1)
                LoadLevel(level);
            else
                LoadFirstLevel();
        }
    }

    private void CompareLevels(int onlineLevel)
    {
        // Compare level from PlayerPrefs vs DB
        // NOTE: This is a callback function that executes after retrieving level from DB
        int localLevel = GetLevelLocal();
        if (localLevel > onlineLevel)
        {
            // Update DB
            SaveLevelOnline(loginController.UserName, localLevel);
            HighestLevel = localLevel;
        }
        else if (onlineLevel > localLevel)
        {
            // Update playerprefs
            SaveLevelLocal(onlineLevel);
            HighestLevel = onlineLevel;
        }

        // Load highest level
        if (HighestLevel != -1)
            LoadLevel(HighestLevel);
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

        // Save level locally and online (if user is logged in)
        SaveLevelLocal(level);
        SaveLevelOnline(loginController.UserName, level);
    }
    #endregion


    #region DB
    private void GetLevelOnline(string userName, Action<int> callback = null)
    {
        StartCoroutine(GetLevelOnlineCR(userName, callback));
    }

    private IEnumerator GetLevelOnlineCR(string userName, Action<int> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("auth_type", (int)DB.UserAuthTypes.GET_LEVEL);
        form.AddField("username", userName);

        using (WWW www = new WWW(DB.URL_USER, form))
        {
            yield return www;

            if (!string.IsNullOrEmpty(www.error))
            {
                // Retrieve high score
                CurrentLevel = int.Parse(www.text);
                Debug.Log("Level retrieved online: " + CurrentLevel);

                if (callback != null)
                    callback(CurrentLevel);
            }
            else
            {
                Debug.LogWarning(www.error);
            }
        }
    }

    private void SaveLevelOnline(string userName, int level)
    {
        if (!loginController.isLoggedIn)
            return;

        StartCoroutine(SaveLevelOnlineCR(userName, level));
    }

    private IEnumerator SaveLevelOnlineCR(string userName, int level)
    {
        WWWForm form = new WWWForm();
        form.AddField("auth_type", (int)DB.UserAuthTypes.SAVE_LEVEL);
        form.AddField("username", userName);
        form.AddField("level", level);

        using (WWW www = new WWW(DB.URL_USER, form))
        {
            yield return www;

            if (!string.IsNullOrEmpty(www.error))
            {
                // Retrieve high score
                Debug.Log("Level saved online: " + CurrentLevel);
            }
            else
            {
                Debug.LogWarning(www.error);
            }
        }
    }
    #endregion


    #region LOCAL LEVEL
    public int GetLevelLocal()
    {
        if (PlayerPrefs.HasKey("Level"))
            return PlayerPrefs.GetInt("Level");
        else
            return -1;
    }

    private void SaveLevelLocal(int level)
    {
        PlayerPrefs.SetInt("Level", level);
    }
    #endregion
}
