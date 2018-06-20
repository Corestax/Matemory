using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginController : Singleton<LoginController>
{
    [HideInInspector]
    public bool UserLogged;

    [HideInInspector]
    public DBResponseUserData responseUserData;
    [HideInInspector]
    public DBResponseUserError responseError;
    [HideInInspector]
    public DBResponseMessage responseMessage;

    public static event Action OnUserLoggedIn;
    public static event Action OnUserLoggedOut;

    private enum RequestTypes { LOGIN, SIGNUP, SAVE_GOOGLE_DATA }

    private void Start()
    {
        UserLogged = false;
    }

    #region SIGNUP
    public void Signup(string name, string email, string pass, Action<bool> callback = null)
    {
        WWWForm form = new WWWForm();

        form.AddField("auth_type", (int)DB.UserAuthTypes.SIGNUP);
        form.AddField("name", name);
        form.AddField("email", email);
        form.AddField("password", pass);

        SendRequest(DB.URL_USER, form, RequestTypes.SIGNUP, callback);
    }

    private void SignupCallback(bool success, DBResponseUserData userData = null, DBResponseUserError error = null)
    {
        if (!success)
            return;

        UserLogged = true;

        if (OnUserLoggedIn != null)
            OnUserLoggedIn();
    }
    #endregion

    #region LOGIN
    public void Login(string email, string pass, Action<bool> callback = null)
    {
        WWWForm form = new WWWForm();

        form.AddField("auth_type", (int)DB.UserAuthTypes.LOGIN);
        form.AddField("email", email);
        form.AddField("password", pass);

        SendRequest(DB.URL_USER, form, RequestTypes.LOGIN, callback);
    }

    private void LoginCallback(bool success, DBResponseUserData userData = null, DBResponseUserError error = null)
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

    public void SendGooglePlayGamesData(string name, string email, string token)
    {
        WWWForm form = new WWWForm();

        Debug.Log(String.Format("{0} {1} {2}", name, email, token));
        
        form.AddField("auth_type", (int)DB.UserAuthTypes.SAVE_GOOGLE_DATA);
        form.AddField("name", name);
        form.AddField("email", email);
        form.AddField("gpgToken", token);
        
        //SendRequest(DB.URL_USER, form, RequestTypes.SAVE_GOOGLE_DATA);
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
    private void SendRequest(string url, WWWForm form, RequestTypes type, Action<bool> externalCallback = null)
    {
        StartCoroutine(SendRequestCR(url, form, type, externalCallback));
    }

    IEnumerator SendRequestCR(string url, WWWForm form, RequestTypes type, Action<bool> externalCallback)
    {
        bool success;
        responseUserData = null;
        responseError = null;
        responseMessage = null;

        using (WWW www = new WWW(url, form))
        {
            yield return www;

            success = string.IsNullOrEmpty(www.error);

            // errors always have the same format
            if (!success)
                responseError = JsonUtility.FromJson<DBResponseUserError>(www.text);

            // call an external class callback
            if (externalCallback != null)
                externalCallback(success);

            switch (type)
            {
                case RequestTypes.LOGIN:
                    if (success)
                        responseUserData = JsonUtility.FromJson<DBResponseUserData>(www.text);

                    LoginCallback(success, responseUserData, responseError);
                    break;
                case RequestTypes.SIGNUP:
                    if (success)
                        responseUserData = JsonUtility.FromJson<DBResponseUserData>(www.text);

                    SignupCallback(success, responseUserData, responseError);
                    break;
                default:
                    break;
            }
        }
    }
    #endregion
}
