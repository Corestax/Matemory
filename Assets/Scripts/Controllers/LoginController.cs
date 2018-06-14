using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginController : Singleton<LoginController>
{
    [HideInInspector]
    public bool UserLogged;

    public void Signup(string name, string email, string pass, Action<bool, UserError> callback = null)
    {
        WWWForm form = new WWWForm();

        form.AddField("auth_type", (int)DB.UserAuthTypes.SIGNUP);
        form.AddField("name", name);
        form.AddField("email", email);
        form.AddField("password", pass);

        SendRequest(DB.URL_USER, form, callback);
    }

    public void Login(string email, string pass, Action<bool, UserError> callback = null)
    {
        WWWForm form = new WWWForm();

        form.AddField("auth_type", (int)DB.UserAuthTypes.LOGIN);
        form.AddField("email", email);
        form.AddField("password", pass);

        SendRequest(DB.URL_USER, form, callback);
    }

    private void SendRequest(string url, WWWForm form, Action<bool, UserError> callback = null)
    {
        StartCoroutine(SendRequestCR(url, form, callback));
    }

    IEnumerator SendRequestCR(string url, WWWForm form, Action<bool, UserError> callback = null)
    {
        using (WWW www = new WWW(url, form))
        {
            yield return www;

            // save UserData (name)?
            if (string.IsNullOrEmpty(www.error))
                Debug.Log(www.text);
            else
                Debug.LogWarning(www.text);
            
            if (callback != null)
                callback(false, null);
        }
    }
}
