﻿using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using System;
using System.Collections;
using System.Collections.Generic;
using GooglePlayGames.Native.Cwrapper;
using UnityEngine;
using UnityEngine.UI;

public class GoogleGameServicesController : Singleton<GoogleGameServicesController>
{
    [SerializeField]
    private GameObject loginResult;

    public static event Action OnUserLoggedIn;
    public static event Action OnUserLoggedOut;

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
        if (Authenticated)
        {
            if (OnUserLoggedIn != null)
                OnUserLoggedIn();
        }
        else
        {
            if (OnUserLoggedOut != null)
                OnUserLoggedOut();
        }
    }

    public void SignIn()
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
                if (OnUserLoggedIn != null)
                    OnUserLoggedIn();
            }
            else
                Debug.LogWarning("Failed to sign in with Google Play Games.");

            ShowLoginResult();
        });
    }


    public void SignOut()
    {
        ((PlayGamesPlatform)Social.Active).SignOut();

        if (OnUserLoggedOut != null)
            OnUserLoggedOut();
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
        if (!Authenticated)
            callback(0);
        
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


    private void ShowLoginResult()
    {
        if (CRShowLoginResult != null)
            StopCoroutine(CRShowLoginResult);
        
        CRShowLoginResult = StartCoroutine(ShowLoginResultCR());
    }

    private Coroutine CRShowLoginResult;
    IEnumerator ShowLoginResultCR()
    {
        if (loginResult != null)
        {
            string resultText = Authenticated ? "LOGIN SUCCESSFUL" : "LOGIN FAILED";

            loginResult.GetComponent<Text>().text = resultText;
            loginResult.GetComponent<CanvasFader>().FadeIn(0.25f);
        }
        
        yield return new WaitForSecondsRealtime(1.5f);
        
        if (loginResult != null)
            loginResult.GetComponent<CanvasFader>().FadeOut(0.25f);

        CRShowLoginResult = null;
    }
}
