using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(LayoutElement))]
public class UIRequiresLogin : MonoBehaviour
{
    [SerializeField]
    private bool isShowing;

    private CanvasGroup canvas;
    private LayoutElement layout;

    void Start()
    {
        canvas = GetComponent<CanvasGroup>();
        layout = GetComponent<LayoutElement>();

        CheckUserStatus();
    }

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

    public void CheckUserStatus()
    {
        if ((IsUserLoggedIn() && isShowing) || (!IsUserLoggedIn() && !isShowing))
            ShowUI();
        else
            HideUI();
    }


    private void ShowUI()
    {
        canvas.alpha = 1f;
        canvas.interactable = true;
        canvas.blocksRaycasts = true;
        layout.ignoreLayout = false;
    }


    private void HideUI()
    {
        canvas.alpha = 0f;
        canvas.interactable = false;
        canvas.blocksRaycasts = false;
        layout.ignoreLayout = true;
    }


    private bool IsUserLoggedIn()
    {
        return LoginController.Instance.isLoggedIn;
    }
}
