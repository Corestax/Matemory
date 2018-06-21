using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : MonoBehaviour
{
    [SerializeField]
    private InputField input_Name;
    [SerializeField]
    private InputField input_Email;
    [SerializeField]
    private InputField input_Pass;
    [SerializeField]
    private Button button_ResetPass;
    [SerializeField]
    private Button button_ResendEmail;
    [SerializeField]
    private Text text_OkButton;
    [SerializeField]
    private Text text_LoginMenu;
    [SerializeField]
    private Text text_SignupMenu;
    [SerializeField]
    private Color color_ActiveMenu;
    [SerializeField]
    private Color color_NotActiveMenu;
    [SerializeField]
    private GameObject go_LoginMessage;

    public enum PanelTypes { LOGIN, SIGNUP, RESET_PASSWORD, RESEND_EMAIL }
    private PanelTypes ActivePanel;

    private LoginController loginController;

    private void Start()
    {
        loginController = LoginController.Instance;
    }

    private void OnEnable()
    {
        ShowPanel(PanelTypes.LOGIN);
    }

    #region SHOW_PANELS
    public void ShowPanel(PanelTypes panel)
    {
        ResetFieldsError();
        HideLoginMessage();

        switch (panel)
        {
            case PanelTypes.LOGIN:
                ShowLoginPanel();
                break;
            case PanelTypes.SIGNUP:
                ShowSignupPanel();
                break;
            case PanelTypes.RESET_PASSWORD:
                ShowResetPasswordPanel();
                break;
            case PanelTypes.RESEND_EMAIL:
                ShowResendEmailPanel();
                break;
            default:
                break;
        }

        ActivePanel = panel;
    }


    private void ShowLoginPanel()
    {
        text_SignupMenu.color = color_NotActiveMenu;
        text_LoginMenu.color = color_ActiveMenu;

        input_Name.gameObject.SetActive(false);
        input_Email.gameObject.SetActive(true);
        input_Pass.gameObject.SetActive(true);
        button_ResetPass.gameObject.SetActive(true);
        button_ResendEmail.gameObject.SetActive(false);

        SetOkButtonText("LOGIN");
    }

    private void ShowSignupPanel()
    {
        text_SignupMenu.color = color_ActiveMenu;
        text_LoginMenu.color = color_NotActiveMenu;

        button_ResetPass.gameObject.SetActive(false);
        input_Name.gameObject.SetActive(true);
        input_Email.gameObject.SetActive(true);
        input_Pass.gameObject.SetActive(true);
        button_ResendEmail.gameObject.SetActive(false);

        SetOkButtonText("SING UP");
    }

    private void ShowResetPasswordPanel()
    {
        text_SignupMenu.color = color_NotActiveMenu;
        text_LoginMenu.color = color_NotActiveMenu;

        button_ResetPass.gameObject.SetActive(false);
        input_Name.gameObject.SetActive(false);
        input_Email.gameObject.SetActive(true);
        input_Pass.gameObject.SetActive(false);
        button_ResendEmail.gameObject.SetActive(false);

        SetOkButtonText("RESET");
    }

    private void ShowResendEmailButton()
    {
        if (ActivePanel != PanelTypes.LOGIN)
            return;

        button_ResetPass.gameObject.SetActive(false);
        button_ResendEmail.gameObject.SetActive(true);

    }

    private void ShowResendEmailPanel()
    {
        ShowResetPasswordPanel();
        SetOkButtonText("RESEND");
    }
    #endregion

    #region GOOGLE_LOGIN
    public void GoogleLoginCallback(bool success)
    {
        string message = success ? "LOGIN SUCCESSFUL" : "LOGIN FAILED";

        ShowLoginMessage(message);

        if (success)
        {
            loginController.SetUserLogged();
            StartCoroutine(OnSuccessfullLoginCR());
        }
    }
    #endregion

    #region LOGIN/SIGNUP
    public void LoginSignCallback(bool success)
    {
        List<string> messages = new List<string>();

        if (success)
        {
            messages.Add("LOGIN SUCCESSFUL");
            ShowLoginMessage(string.Join("\n", messages.ToArray()));

            StartCoroutine(OnSuccessfullLoginCR());
        }
        else
            ShowResponseErrors();
    }

    IEnumerator OnSuccessfullLoginCR()
    {
        yield return new WaitForSecondsRealtime(1f);

        UIController.Instance.HideActivePanel();
    }
    #endregion

    #region RESET_PASSWORD
    public void ResetPasswordCallback(bool success)
    {
        DBResponseMessage message = loginController.responseMessage;

        if (success)
            ShowLoginMessage(message.message);
        else
            ShowResponseErrors();

    }
    #endregion

    #region RESEND_EMAIL
    public void ResendEmailCallback(bool success)
    {
        DBResponseMessage message = loginController.responseMessage;

        if (success)
            ShowLoginMessage(message.message);
        else
            ShowResponseErrors();
    }
    #endregion

    #region ONCLICKS
    public void OnClickSend()
    {
        ResetFieldsError();

        string name = input_Name.text;
        string email = input_Email.text;
        string pass = input_Pass.text;

        switch (ActivePanel)
        {
            case PanelTypes.LOGIN:
                loginController.Login(email, pass, LoginSignCallback);
                break;
            case PanelTypes.SIGNUP:
                loginController.Signup(name, email, pass, LoginSignCallback);
                break;
            case PanelTypes.RESET_PASSWORD:
                loginController.ResetPassword(email, ResetPasswordCallback);
                break;
            case PanelTypes.RESEND_EMAIL:
                loginController.ResendVerificationEmail(email, ResendEmailCallback);
                break;
            default:
                break;
        }

        HideLoginMessage();
    }

    public void OnClickShowLoginPanel()
    {
        ShowPanel(PanelTypes.LOGIN);
    }

    public void OnClickShowSignupPanel()
    {
        ShowPanel(PanelTypes.SIGNUP);
    }

    public void OnClickShowResetPassword()
    {
        ShowPanel(PanelTypes.RESET_PASSWORD);
    }

    public void OnClickShowResendEmail()
    {
        ShowPanel(PanelTypes.RESEND_EMAIL);
    }

    public void OnClickGoogleSignIn()
    {
        GoogleSignInController.Instance.SignInNormal(GoogleLoginCallback);
    }
    #endregion

    #region LOGIN_MESSAGE
    private void ShowLoginMessage(string messsage)
    {
        go_LoginMessage.GetComponent<Text>().text = messsage;
        go_LoginMessage.GetComponent<CanvasFader>().FadeIn(0.25f);
    }


    private void HideLoginMessage()
    {
        go_LoginMessage.GetComponent<Text>().text = "";
        go_LoginMessage.GetComponent<CanvasFader>().FadeOut(0.25f);
    }

    private void RunLoginMessageTimeout(float time = 1.5f)
    {
        if (CRLoginMessageTimeout != null)
            StopCoroutine(CRLoginMessageTimeout);

        CRLoginMessageTimeout = StartCoroutine(LoginMessageTimeoutCR(time));
    }

    Coroutine CRLoginMessageTimeout;
    IEnumerator LoginMessageTimeoutCR(float time)
    {
        yield return new WaitForSecondsRealtime(1.5f);

        HideLoginMessage();

        CRLoginMessageTimeout = null;
    }
    #endregion

    public void ResetStatus()
    {
        ShowPanel(PanelTypes.LOGIN);

        input_Name.text = "";
        input_Email.text = "";
        input_Pass.text = "";

        ResetFieldsError();
        HideLoginMessage();
    }

    private void SetFieldErrorStatus(InputField field, bool status = true)
    {
        var transformBorder = field.transform.Find("Image_BorderError");

        if (transformBorder != null)
            transformBorder.gameObject.SetActive(status);
    }

    private void SetOkButtonText(string text)
    {
        text_OkButton.text = text;
    }

    private void ResetFieldsError()
    {
        SetFieldErrorStatus(input_Name, false);
        SetFieldErrorStatus(input_Email, false);
        SetFieldErrorStatus(input_Pass, false);
    }

    private void ShowResponseErrors()
    {
        DBResponseUserError error = loginController.responseError;
        List<string> messages = new List<string>();

        if (error == null)
            return;

        // Email not verified
        if (error.error == "Email not verified.")
        {
            ShowResendEmailButton();
        }

        // name field
            if (!string.IsNullOrEmpty(error.name))
        {
            messages.Add(error.name.ToUpper());
            SetFieldErrorStatus(input_Name, true);
        }

        // email field
        if (!string.IsNullOrEmpty(error.email))
        {
            messages.Add(error.email.ToUpper());
            SetFieldErrorStatus(input_Email, true);
        }

        // password field
        if (!string.IsNullOrEmpty(error.password))
        {
            messages.Add(error.password.ToUpper());
            SetFieldErrorStatus(input_Pass, true);
        }

        // if general error
        if (messages.Count == 0)
            messages.Add(error.error);

        ShowLoginMessage(string.Join("\n", messages.ToArray()));
    }
}

