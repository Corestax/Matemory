using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginButton : MonoBehaviour
{
    [SerializeField]
    private Text text_Button;

    private const string loginText = "LOGIN";
    private const string logoutText = "LOGOUT";


    private void OnEnable()
    {
        LoginController.OnUserLoggedIn += CheckUserStatus;
        LoginController.OnUserLoggedOut += CheckUserStatus;
    }

    private void OnDisable()
    {
        LoginController.OnUserLoggedIn -= CheckUserStatus;
        LoginController.OnUserLoggedOut -= CheckUserStatus;
    }

    public void OnClick()
    {
        if (IsUserLoggedIn())
        {
            if (GoogleGameServicesController.Instance.Authenticated)
                GoogleGameServicesController.Instance.SignOut();
            
            else
                LoginController.Instance.Logout();
        }
        else
            UIController.Instance.ShowPanel(UIController.PanelTypes.LOGIN);
    }

    public void LoginCallback(bool status, string error)
    {
        Debug.Log("Google Login");
    }

    public void UserLoggedIn()
    {
        text_Button.text = logoutText;
    }


    public void UserLoggedOut()
    {
        text_Button.text = loginText;
    }


    public void CheckUserStatus()
    {
        if (IsUserLoggedIn())
            UserLoggedIn();
        else
            UserLoggedOut();
    }


    private bool IsUserLoggedIn()
    {
        return GoogleGameServicesController.Instance.Authenticated || LoginController.Instance.UserLogged;
    }

}
