using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ButtonsController : Singleton<ButtonsController>
{
    private List<Object> list_buttonObjects = new List<Object>();

    public void DisableAllButtonsExcept(Transform _parent)
    {
        if (list_buttonObjects.Count > 0)
            return;

        var buttonExceptions = _parent.GetComponentsInChildren<Button>().ToList();
        var toggleExceptions = _parent.GetComponentsInChildren<Toggle>().ToList();
        var inputFieldExceptions = _parent.GetComponentsInChildren<InputField>().ToList();

        // Disable all active buttons
        var buttons = FindObjectsOfType<Button>();
        foreach (Button b in buttons)
        {
            if (b.tag != "AlwaysInteractable" && !buttonExceptions.Contains(b))
            {
                if (b.interactable)
                {
                    list_buttonObjects.Add(b);
                    b.interactable = false;
                }
            }
        }

        // Disable all active toggles
        var toggles = FindObjectsOfType<Toggle>();
        foreach (Toggle t in toggles)
        {
            if (t.interactable && !toggleExceptions.Contains(t))
            {
                list_buttonObjects.Add(t);
                t.interactable = false;
            }
        }

        // Disable all active inputfields
        var inputFields = FindObjectsOfType<InputField>();
        foreach (InputField i in inputFields)
        {
            if (i.interactable && !inputFieldExceptions.Contains(i))
            {
                list_buttonObjects.Add(i);
                i.interactable = false;
            }
        }
    }

    public void DisableAllButtons()
    {
        if (list_buttonObjects.Count > 0)
            return;

        // Disable all active buttons
        var buttons = FindObjectsOfType<Button>();
        foreach (Button b in buttons)
        {
            if (b.tag != "AlwaysInteractable")
            {
                if (b.interactable)
                {
                    list_buttonObjects.Add(b);
                    b.interactable = false;
                }
            }
        }

        // Disable all active toggles
        var toggles = FindObjectsOfType<Toggle>();
        foreach (Toggle t in toggles)
        {
            if (t.interactable)
            {
                list_buttonObjects.Add(t);
                t.interactable = false;
            }
        }

        // Disable all active inputfields
        var inputFields = FindObjectsOfType<InputField>();
        foreach (InputField i in inputFields)
        {
            if (i.interactable)
            {
                list_buttonObjects.Add(i);
                i.interactable = false;
            }
        }
    }

    public void EnableAllButtons()
    {
        foreach (var obj in list_buttonObjects)
        {
            if (obj != null)
            {
                // Re-enable buttons
                var b = obj as Button;
                if (b)
                {
                    b.interactable = true;
                    continue;
                }

                // Re-enable toggles
                var t = obj as Toggle;
                if (t)
                {
                    t.interactable = true;
                    continue;
                }

                // Re-enable inputfields
                var i = obj as InputField;
                if (i)
                {
                    i.interactable = true;
                    continue;
                }
            }
        }
        list_buttonObjects.Clear();
    }
}
