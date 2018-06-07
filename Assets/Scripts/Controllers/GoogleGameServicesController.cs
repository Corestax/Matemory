using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using GooglePlayGames.BasicApi.SavedGame;
using System;

public class GoogleGameServicesController : Singleton<GoogleGameServicesController>
{
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
}
