using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(LayoutElement))]
public class UIRequiresGoogleLogin : MonoBehaviour
{
    void Start()
    {
        CheckUserStatus();
    }

    private void OnEnable()
    {
        //GoogleGameServicesController.OnUserLoggedIn += CheckUserStatus;
        //GoogleGameServicesController.OnUserLoggedOut += CheckUserStatus;
    }

    private void OnDisable()
    {
        //GoogleGameServicesController.OnUserLoggedIn -= CheckUserStatus;
        //GoogleGameServicesController.OnUserLoggedOut -= CheckUserStatus;
    }

    public void CheckUserStatus()
    {
        CanvasGroup canvas = GetComponent<CanvasGroup>();
        LayoutElement layout = GetComponent<LayoutElement>();

        if (IsUserLoggedIn())
        {
            canvas.alpha = 1f;
            canvas.interactable = true;
            canvas.blocksRaycasts = true;
            layout.ignoreLayout = false;
        }
        else
        {
            canvas.alpha = 0f;
            canvas.interactable = false;
            canvas.blocksRaycasts = false;
            layout.ignoreLayout = true;
        }
    }


    private bool IsUserLoggedIn()
    {
        return false; //GoogleGameServicesController.Instance.Authenticated;
    }
}
