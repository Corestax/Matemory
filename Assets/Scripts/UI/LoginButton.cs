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


    private void OnEnable() {
        GoogleGameServicesController.OnUserLoggedIn += CheckUserStatus;
        GoogleGameServicesController.OnUserLoggedOut += CheckUserStatus;
    }

    private void OnDisable()
    {
        GoogleGameServicesController.OnUserLoggedIn -= CheckUserStatus;
        GoogleGameServicesController.OnUserLoggedOut -= CheckUserStatus;
    }

    public void OnClick()
    {
        if (IsUserLoggedIn())
            GoogleGameServicesController.Instance.SignOut();
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
        return GoogleGameServicesController.Instance.Authenticated;
    }

}
