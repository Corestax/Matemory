using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoogleSignInController : Singleton<GoogleSignInController>
{
    [HideInInspector]
    public bool isLoggedIn = false;

    private void Start()
    {
        ManualInit();
    }

    public void ManualInit()
    {
        ChangeLogging(false);

        string client_id = "491925000490-fud0svp2cu1sj2miklv2bsf1qg7526rm.apps.googleusercontent.com";

        LCGoogleLoginBridge.InitWithClientID(client_id);
    }

    public void SignInNormal(Action<bool> callback = null)
    {
        LCGoogleLoginBridge.LoginUser((loggedIn) => {
            isLoggedIn = loggedIn;

            if (callback != null)
                callback(loggedIn);
        }, false);
    }

    public void SignInSilent(Action<bool> callback = null)
    {
        LCGoogleLoginBridge.LoginUser((loggedIn) => {
            isLoggedIn = loggedIn;

            if (callback != null)
                callback(loggedIn);
        }, true);
    }

    public void SignOut()
    {
        LCGoogleLoginBridge.LogoutUser();
    }

    public string GetUserID()
    {
        return LCGoogleLoginBridge.GSIUserID();
    }

    public string GetUserEmail()
    {
        return LCGoogleLoginBridge.GSIEmail();
    }

    public string GetUserName()
    {
        return LCGoogleLoginBridge.GSIUserName();
    }

    public string GetPhotoUrl()
    {
        return LCGoogleLoginBridge.GSIPhotoUrl();
    }

    public string GetUserToken()
    {
        return LCGoogleLoginBridge.GSIIDUserToken();
    }

    public void GrantedScopes()
    {
        string[] scopes = LCGoogleLoginBridge.GSIGrantedScopes();
        if (scopes == null || scopes.Length <= 0)
        {
            PrintMessage("GrantedScopes: None");
        }
        else
        {
            string scopeStr = "";
            foreach (string scope in scopes)
            {
                scopeStr += " " + scope;
            }
            PrintMessage("GrantedScopes: " + scopes.Length + scopeStr);
        }
    }

    public void AccessToken()
    {
        PrintMessage("AccessToken: " + LCGoogleLoginBridge.GSIAccessToken() + AdditionalAndroidOnlyNotes());
    }

    public void RefreshToken()
    {
        PrintMessage("RefreshToken: " + LCGoogleLoginBridge.GSIRefreshToken() + AdditionalAndroidOnlyNotes());
    }

    static string AdditionalAndroidOnlyNotes()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            return " >> Its always null for android platform";
        }
        return "";
    }


    bool logsEnabled = false;
    public void ChangeLogging(bool enable)
    {
        logsEnabled = enable;
        LCGoogleLoginBridge.ChangeLoggingLevel(enable);
        PrintMessage("ChangeLogging: " + enable);
    }

    void PrintMessage(string message)
    {
        if (logsEnabled)
            Debug.Log("Google Login: " + message);
    }
}
