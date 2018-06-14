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
        Debug.Log("sign up");

        if (callback != null)
            callback(false, null);
    }

    public void Login(string email, string pass, Action<bool, UserError> callback = null)
    {
        Debug.Log("login");

        if (callback != null)
            callback(false, null);
    }
}
