﻿using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using System;
using System.Collections;
using System.Collections.Generic;
using GooglePlayGames.Native.Cwrapper;
using UnityEngine;

public class GoogleGameServicesController : Singleton<GoogleGameServicesController>
{
    private enum GetLeaderboardIdByLevel {
        CgkIqqKPyKgOEAIQAQ = 1,
        CgkIqqKPyKgOEAIQAg = 2,
        CgkIqqKPyKgOEAIQAw = 3,
        CgkIqqKPyKgOEAIQBA = 4,
        CgkIqqKPyKgOEAIQBQ = 5,
        CgkIqqKPyKgOEAIQBg = 6,
        CgkIqqKPyKgOEAIQBw = 7,
        CgkIqqKPyKgOEAIQCA = 8,
        CgkIqqKPyKgOEAIQCQ = 9,
        CgkIqqKPyKgOEAIQCg = 10,
        CgkIqqKPyKgOEAIQCw = 11,
        CgkIqqKPyKgOEAIQDA = 12,
        CgkIqqKPyKgOEAIQDQ = 13,
        CgkIqqKPyKgOEAIQDg = 14,
        CgkIqqKPyKgOEAIQDw = 15,
        CgkIqqKPyKgOEAIQEA = 16,
        CgkIqqKPyKgOEAIQEQ = 17,
        CgkIqqKPyKgOEAIQEg = 18,
        CgkIqqKPyKgOEAIQEw = 19,
        CgkIqqKPyKgOEAIQFA = 20
    }


    private bool mAuthenticating = false;

    // Use this for initialization
    void Start()
    {
        Init();
    }

    private void Init()
    {
        if (Authenticated || mAuthenticating)
        {
            Debug.LogWarning("Ignoring repeated call to Authenticate().");
            return;
        }
                
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
                .EnableSavedGames()
                .Build();        

        PlayGamesPlatform.InitializeInstance(config);
        
        PlayGamesPlatform.Activate();
        
        mAuthenticating = true;
        Social.localUser.Authenticate((bool success) =>
        {
            mAuthenticating = false;
            if (success)
            {
                // if we signed in successfully, load data from cloud
                Debug.Log("Login successful!");
            }
            else
            {
                // no need to show error message (error messages are shown automatically
                // by plugin)
                Debug.LogWarning("Failed to sign in with Google Play Games.");
            }
        });
    }

    public void AddScoreToLeaderboard(int level, int score)
    {
        GetLeaderboardIdByLevel lId;

        if (!Authenticated)
            return;

        if (!Enum.IsDefined(typeof(GetLeaderboardIdByLevel), level))
            return;

        try
        {
            lId = GetLevelId(level);
        }
        catch (Exception e)
        {
            return;
        }

        Social.ReportScore(score, lId.ToString(), (bool success) => {
            Debug.Log("Score updated");
        });
    }

    public void ShowLeaderboard()
    {
        if (Authenticated)
            Social.ShowLeaderboardUI();
    }

    public void GetHighestUserScore(int level, Action<int> callback = null)
    {        
        GetLeaderboardIdByLevel levelId;

        try
        {
            levelId = GetLevelId(level);
        } catch (Exception e)
        {
            Debug.LogWarning(e.Message);

            if (callback != null)
                callback(0);
            
            return;
        }

        PlayGamesPlatform.Instance.LoadScores(
            levelId.ToString(),
            LeaderboardStart.PlayerCentered,
            1,
            LeaderboardCollection.Public,
            LeaderboardTimeSpan.AllTime,
        (LeaderboardScoreData data) =>
        {
            int score = 0;
            
            if (data.PlayerScore != null)
                score = (int) data.PlayerScore.value;

            if (callback != null)
                callback(score);
        });
    }

    public bool Authenticating
    {
        get
        {
            return mAuthenticating;
        }
    }

    public bool Authenticated
    {
        get
        {
            return Social.Active.localUser.authenticated;
        }
    }

    private GetLeaderboardIdByLevel GetLevelId(int level)
    {
        if (!Enum.IsDefined(typeof(GetLeaderboardIdByLevel), level))
            throw new Exception("Wrong level");

        GetLeaderboardIdByLevel lId = (GetLeaderboardIdByLevel)level;

        return lId;
    }
}
