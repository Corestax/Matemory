using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginController : Singleton<LoginController>
{
    [HideInInspector]
    public bool UserLogged;

    public static event Action OnUserLoggedIn;
    public static event Action OnUserLoggedOut;

    private enum RequestTypes { LOGIN, SIGNUP, SAVE_GOOGLE_DATA }

    private void Start()
    {
        UserLogged = false;
    }

    #region SIGNUP
    public void Signup(string name, string email, string pass, Action<bool, UserError> callback = null)
    {
        WWWForm form = new WWWForm();

        form.AddField("auth_type", (int)DB.UserAuthTypes.SIGNUP);
        form.AddField("name", name);
        form.AddField("email", email);
        form.AddField("password", pass);

        SendRequest(DB.URL_USER, form, RequestTypes.SIGNUP, callback);
    }

    private void SignupCallback(bool success, UserData userData = null, UserError error = null)
    {
        if (!success)
            return;

        UserLogged = true;

        if (OnUserLoggedIn != null)
            OnUserLoggedIn();
    }
    #endregion

    #region LOGIN
    public void Login(string email, string pass, Action<bool, UserError> callback = null)
    {
        WWWForm form = new WWWForm();

        form.AddField("auth_type", (int)DB.UserAuthTypes.LOGIN);
        form.AddField("email", email);
        form.AddField("password", pass);

        SendRequest(DB.URL_USER, form, RequestTypes.LOGIN, callback);
    }

    private void LoginCallback(bool success, UserData userData = null, UserError error = null)
    {
        if (!success)
            return;

        SetUserLogged();
    }


    public void SetUserLogged()
    {
        UserLogged = true;

        if (OnUserLoggedIn != null)
            OnUserLoggedIn();
    }
    #endregion

    #region LOGOUT
    public void Logout()
    {
        StartCoroutine(LogoutCR());
    }

    
    IEnumerator LogoutCR()
    {
        yield return new WaitForSecondsRealtime(0.25f);

        SetUserLogout();
    }

    
    public void SetUserLogout()
    {
        UserLogged = false;

        if (OnUserLoggedOut != null)
            OnUserLoggedOut();
    }
    #endregion

    #region SEND_REQUEST
    private void SendRequest(string url, WWWForm form, RequestTypes type, Action<bool, UserError> callback = null)
    {
        StartCoroutine(SendRequestCR(url, form, type, callback));
    }

    IEnumerator SendRequestCR(string url, WWWForm form, RequestTypes type, Action<bool, UserError> callback = null)
    {
        bool success;
        UserData userData = null;
        UserError error = null;

        using (WWW www = new WWW(url, form))
        {
            yield return www;

            // save UserData (name)?
            if (string.IsNullOrEmpty(www.error))
            {
                success = true;
                userData = JsonUtility.FromJson<UserData>(www.text);
            }
            else
            {
                success = false;
                error = JsonUtility.FromJson<UserError>(www.text);
            }
                
            if (callback != null)
            {
                callback(success, error);
            }


            switch (type)
            {
                case RequestTypes.LOGIN:
                    LoginCallback(success, userData, error);
                    break;
                case RequestTypes.SIGNUP:
                    SignupCallback(success, userData, error);
                    break;
                default:
                    break;
            }
                
        }
    }
    #endregion
}
