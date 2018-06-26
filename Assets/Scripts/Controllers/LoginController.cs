using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginController : Singleton<LoginController>
{
    [HideInInspector]
    public bool isLoggedIn;

    [HideInInspector]
    public DBResponseUserData responseUserData;
    [HideInInspector]
    public DBResponseUserError responseError;
    [HideInInspector]
    public DBResponseMessage responseMessage;

    public string UserName { get; private set; }
    public string Email { get; private set; }

    public static event Action OnUserLoggedIn;
    public static event Action OnUserLoggedOut;

    private enum RequestTypes { LOGIN, SIGNUP, SAVE_GOOGLE_DATA, RESET_PASSWORD, RESEND_EMAIL }

    private void Start()
    {
        //Debug.Log("isUserLogeedIn: " + UserLogged);

        AutoLogin();
    }

    #region SIGNUP
    public void Signup(string name, string email, string pass, Action<bool, DBResponseUserError> callback = null)
    {
        WWWForm form = new WWWForm();

        form.AddField("auth_type", (int)DB.UserAuthTypes.SIGNUP);
        form.AddField("name", name);
        form.AddField("email", email);
        form.AddField("password", pass);

        StartCoroutine(SignupCR(form, callback));
    }

    IEnumerator SignupCR(WWWForm form, Action<bool, DBResponseUserError> callback)
    {
        using (WWW www = new WWW(DB.URL_USER, form))
        {
            yield return www;

            bool success = string.IsNullOrEmpty(www.error);

            // errors always have same format
            if (!success)
                responseError = JsonUtility.FromJson<DBResponseUserError>(www.text);
            else
            {
                responseUserData = JsonUtility.FromJson<DBResponseUserData>(www.text);

                SetUserLogged();

                UserName = responseUserData.name;
                Email = responseUserData.email;
            }

            // call an external class callback
            if (callback != null)
                callback(success, responseError);
        }
    }
    #endregion

    #region LOGIN
    public void Login(string email, string pass, Action<bool, DBResponseUserError> callback = null)
    {
        WWWForm form = new WWWForm();

        form.AddField("auth_type", (int)DB.UserAuthTypes.LOGIN);
        form.AddField("email", email);
        form.AddField("password", pass);

        StartCoroutine(LoginCR(form, callback));
    }

    IEnumerator LoginCR(WWWForm form, Action<bool, DBResponseUserError> callback)
    {
        using (WWW www = new WWW(DB.URL_USER, form))
        {
            yield return www;

            bool success = string.IsNullOrEmpty(www.error);

            // errors always have same format
            if (!success)
                responseError = JsonUtility.FromJson<DBResponseUserError>(www.text);
            else
            {
                responseUserData = JsonUtility.FromJson<DBResponseUserData>(www.text);

                SetUserLogged();
                UserName = responseUserData.name;
                Email = responseUserData.email;
            }

            // call an external class callback
            if (callback != null)
                callback(success, responseError);
        }
    }

    public void SetUserLogged()
    {
        isLoggedIn = true;

        if (OnUserLoggedIn != null)
            OnUserLoggedIn();
    }

    private void AutoLogin()
    {
        if (!PlayerPrefs.HasKey("login_type"))
            return;
        
        if (PlayerPrefs.GetString("login_type") == "google")
            GoogleSignInController.Instance.SignInSilent((success) =>
            {
                SaveGoogleData();
                SetUserLogged();
            });

        else
        {
            var email = PlayerPrefs.GetString("email") ?? null;
            var password = PlayerPrefs.GetString("password") ?? null;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return;

            Login(email, password);
        }
    }

    #endregion

    #region LOGOUT
    public void Logout()
    {
        if (GoogleSignInController.Instance.isLoggedIn)
            GoogleSignInController.Instance.SignOut();

        StartCoroutine(LogoutCR());
    }

    
    IEnumerator LogoutCR()
    {
        yield return new WaitForSecondsRealtime(0.25f);

        SetUserLogout();

        SceneManager.LoadScene(0);
    }

    
    public void SetUserLogout()
    {
        isLoggedIn = false;

        if (OnUserLoggedOut != null)
            OnUserLoggedOut();
        
        // reset playerPrefs
        if (PlayerPrefs.HasKey("login_type"))
            PlayerPrefs.DeleteKey("login_type");
        
        if (PlayerPrefs.HasKey("email"))
            PlayerPrefs.DeleteKey("email");
        
        if (PlayerPrefs.HasKey("password"))
            PlayerPrefs.DeleteKey("password");
    }
    #endregion

    #region RESET_PASSWORD
    public void ResetPassword(string email, Action<bool, DBResponseMessage, DBResponseUserError> callback = null)
    {
        WWWForm form = new WWWForm();

        form.AddField("auth_type", (int)DB.UserAuthTypes.RESET_PASSWORD);
        form.AddField("email", email);

        StartCoroutine(ResetPasswordCR(form, callback));
    }

    IEnumerator ResetPasswordCR(WWWForm form, Action<bool, DBResponseMessage, DBResponseUserError> callback)
    {
        using (WWW www = new WWW(DB.URL_USER, form))
        {
            yield return www;

            bool success = string.IsNullOrEmpty(www.error);

            // errors always have same format
            if (!success)
                responseError = JsonUtility.FromJson<DBResponseUserError>(www.text);
            else
                responseMessage = JsonUtility.FromJson<DBResponseMessage>(www.text);

            // call an external class callback
            if (callback != null)
                callback(success, responseMessage, responseError);
        }
    }
    #endregion

    #region RESEND_EMAIL
    public void ResendVerificationEmail(string email, Action<bool, DBResponseMessage, DBResponseUserError> callback = null)
    {
        WWWForm form = new WWWForm();

        form.AddField("auth_type", (int)DB.UserAuthTypes.RESEND_EMAIL);
        form.AddField("email", email);

        StartCoroutine(ResetPasswordCR(form, callback));
    }

    IEnumerator ResendVerificationEmailCR(WWWForm form, Action<bool, DBResponseMessage, DBResponseUserError> callback)
    {
        using (WWW www = new WWW(DB.URL_USER, form))
        {
            yield return www;

            bool success = string.IsNullOrEmpty(www.error);

            // errors always have same format
            if (!success)
                responseError = JsonUtility.FromJson<DBResponseUserError>(www.text);
            else
                responseMessage = JsonUtility.FromJson<DBResponseMessage>(www.text);

            // call an external class callback
            if (callback != null)
                callback(success, responseMessage, responseError);
        }
    }
    #endregion

    #region GOOGLE_LOGIN

    public void SaveGoogleData()
    {
        var googleSignInController = GoogleSignInController.Instance;

        if (!googleSignInController.isLoggedIn)
            return;
        
        WWWForm form = new WWWForm();

        form.AddField("auth_type", (int)DB.UserAuthTypes.SAVE_GOOGLE_DATA);
        form.AddField("name", googleSignInController.GetUserName());
        form.AddField("email", googleSignInController.GetUserEmail());
        form.AddField("token", googleSignInController.GetUserToken());

        UserName = googleSignInController.GetUserName();
        StartCoroutine(SaveGoogleDataCR(form));
    }

    IEnumerator SaveGoogleDataCR(WWWForm form)
    {
        using (WWW www = new WWW(DB.URL_USER, form))
        {
            yield return www;

            if (string.IsNullOrEmpty(www.error))
                Debug.LogWarning(www.text);
        }
    }
    #endregion
}
