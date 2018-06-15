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
    private Text text_LoginMenu;
    [SerializeField]
    private Text text_SignupMenu;
    [SerializeField]
    private Color color_ActiveMenu;
    [SerializeField]
    private Color color_NotActiveMenu;
    [SerializeField]
    private Color color_FieldErrorBackground;
    [SerializeField]
    private GameObject go_LoginMessage;

    public enum PanelTypes { LOGIN, SIGNUP }
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

    public void ShowPanel(PanelTypes panel)
    {
        ResetFieldsBackground();
        HideLoginMessage();

        switch (panel)
        {
            case PanelTypes.LOGIN:
                ShowLoginPanel();
                break;
            case PanelTypes.SIGNUP:
                ShowSignupPanel();
                break;
            default:
                break;
        }

        ActivePanel = panel;
    }

    public void OnClickShowLoginPanel()
    {
        ShowPanel(PanelTypes.LOGIN);
    }

    public void OnClickShowSignupPanel()
    {
        ShowPanel(PanelTypes.SIGNUP);
    }

    #region GOOGLE_LOGIN
    public void OnClickGoogleLogin()
    {
        GoogleGameServicesController.Instance.SignIn(GoogleLoginCallback);
    }

    public void GoogleLoginCallback(bool success)
    {
        string message = success ? "LOGIN SUCCESSFUL" : "LOGIN FAILED";

        if (!success)
        {
            ShowLoginMessage(message);
            RunLoginMessageTimeout(1.5f);
        }
        else
            UIController.Instance.ShowPanel(UIController.PanelTypes.SETTINGS);
    }
    #endregion

    #region LOGIN/SIGNUP
    public void OnClickSend()
    {
        ResetFieldsBackground();

        string name = input_Name.text;
        string email = input_Email.text;
        string pass = input_Pass.text;

        if (ActivePanel == PanelTypes.LOGIN)
            loginController.Login(email, pass, LoginSignCallback);
        else
            loginController.Signup(name, email, pass, LoginSignCallback);

        HideLoginMessage();
    }

    public void LoginSignCallback(bool success, UserError error = null)
    {
        List<string> messages = new List<string>();

        if (success)
        {
            messages.Add("LOGIN SUCCESSFUL");
            StartCoroutine(OnSuccessfullLoginCR());
        }
        else
        {
            if (!string.IsNullOrEmpty(error.name))
            {
                messages.Add(error.name.ToUpper());
                input_Name.GetComponent<Image>().color = color_FieldErrorBackground;
            }

            if (!string.IsNullOrEmpty(error.email))
            {
                messages.Add(error.email.ToUpper());
                input_Email.GetComponent<Image>().color = color_FieldErrorBackground;
            }

            if (!string.IsNullOrEmpty(error.password))
            {
                messages.Add(error.password.ToUpper());
                input_Pass.GetComponent<Image>().color = color_FieldErrorBackground;
            }
        }

        if (!success && messages.Count == 0)
            messages.Add(error.error);

        ShowLoginMessage(string.Join("\n", messages.ToArray()));
    }

    IEnumerator OnSuccessfullLoginCR()
    {
        yield return new WaitForSecondsRealtime(0.75f);

        UIController.Instance.ShowPanel(UIController.PanelTypes.SETTINGS);
    }
    #endregion


    public void ResetStatus()
    {
        ShowPanel(PanelTypes.LOGIN);

        input_Name.text = "";
        input_Email.text = "";
        input_Pass.text = "";

        ResetFieldsBackground();
        HideLoginMessage();
    }

    private void ResetFieldsBackground()
    {
        input_Name.GetComponent<Image>().color = Color.white;
        input_Email.GetComponent<Image>().color = Color.white;
        input_Pass.GetComponent<Image>().color = Color.white;
    }

    private void ShowLoginPanel()
    {
        text_SignupMenu.color = color_NotActiveMenu;
        text_LoginMenu.color = color_ActiveMenu;

        input_Name.gameObject.SetActive(false);
    }

    private void ShowSignupPanel()
    {
        text_SignupMenu.color = color_ActiveMenu;
        text_LoginMenu.color = color_NotActiveMenu;

        input_Name.gameObject.SetActive(true);
    }


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
}

