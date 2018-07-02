using System;
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

    public ModelsController.ModelTypes GetLevelCharacter(int level)
    {
        ModelsController.ModelTypes character = ModelsController.ModelTypes.NONE;

        if (Levels.ContainsKey(level))
            character = Levels[level];

        return character;
    }

    public void SetLevel(int level)
    {
        if (level - 1 >= Levels.Count)
            level = Levels.Count;

        // Update level & high score
        CurrentLevel = level;

        // Update highest level
        if (CurrentLevel > HighestLevel)
            HighestLevel = CurrentLevel;

        // Set character position in map
        mapsController.SetCharacterLevel(level);
    }

    public void SetHighestLevel(int level)
    {
        HighestLevel = level;
    }

    #region LOAD LEVEL
    public void GetLastSavedLevel(Action<int> callback = null)
    {
        int level;

        if (loginController.isLoggedIn)
        {
            GetLevelOnline(loginController.Email, (db_level) =>
            {
                level = CompareLevels(db_level);

                if (callback != null)
                    callback(level);
            });
        }
        else
        {
            level = GetLevelLocal();

            if (callback != null)
                callback(level);
        }
    }


    public void LoadLastSavedLevel()
    {
        GetLastSavedLevel((level) => {
            LoadLevel(level > 0 ? level : 1);
        });
    }

    private int CompareLevels(int onlineLevel)
    {
        // Compare level from PlayerPrefs vs DB
        // NOTE: This is a callback function that executes after retrieving level from DB
        int localLevel = GetLevelLocal();

        HighestLevel = localLevel >= onlineLevel ? localLevel : onlineLevel;

        // Update DB or playerprefs
        if (localLevel > onlineLevel)
            SaveLevelOnline(loginController.Email, localLevel);
        else if (onlineLevel > localLevel)
            SaveLevelLocal(onlineLevel);

        return HighestLevel;
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
        SetLevel(level);

        // Spawn model
        ModelsController.ModelTypes type = Levels[level];
        modelsController.Spawn(type);
    }
    #endregion


    #region SAVE LEVEL    
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
        SaveLevelOnline(loginController.Email, level);
    }
    #endregion


    #region ONLINE
    private void GetLevelOnline(string email, Action<int> callback = null)
    {
        StartCoroutine(GetLevelOnlineCR(email, callback));
    }

    private IEnumerator GetLevelOnlineCR(string email, Action<int> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("auth_type", (int)DB.LevelAuthTypes.GET_LEVEL);
        form.AddField("email", email);

        using (WWW www = new WWW(DB.URL_LEVEL, form))
        {
            yield return www;

            if (string.IsNullOrEmpty(www.error))
            {
                // Retrieve high score
                CurrentLevel = int.Parse(www.text);

                //Debug.Log("Level retrieved online: " + CurrentLevel);

                if (callback != null)
                    callback(CurrentLevel);
            }
            else
            {
                Debug.LogWarning(www.error);
            }
        }
    }

    private void SaveLevelOnline(string email, int level)
    {
        if (!loginController.isLoggedIn)
            return;

        StartCoroutine(SaveLevelOnlineCR(email, level));
    }

    private IEnumerator SaveLevelOnlineCR(string email, int level)
    {
        WWWForm form = new WWWForm();
        form.AddField("auth_type", (int)DB.LevelAuthTypes.SAVE_LEVEL);
        form.AddField("email", email);
        form.AddField("level", level);

        using (WWW www = new WWW(DB.URL_LEVEL, form))
        {
            yield return www;

            if (string.IsNullOrEmpty(www.error))
            {
                // Retrieve high score
                //Debug.Log("Level saved online: " + CurrentLevel);
            }
            else
            {
                Debug.LogWarning(www.error);
            }
        }
    }
    #endregion


    #region LOCAL
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
