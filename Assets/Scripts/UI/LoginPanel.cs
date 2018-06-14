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
    private GameObject go_LoginMessage;

    public enum PanelTypes { LOGIN, SIGNUP }
    private PanelTypes ActivePanel;


    private void OnEnable()
    {
        ShowPanel(PanelTypes.LOGIN);
    }

    public void ShowPanel(PanelTypes panel)
    {
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

    public void ResetStatus()
    {
        ShowPanel(PanelTypes.LOGIN);

        input_Name.text = "";
        input_Email.text = "";
        input_Pass.text = "";

        HideLoginMessage();
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

