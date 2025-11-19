using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardToggler : MonoBehaviour
{
    [SerializeField]
    TMPro.TMP_InputField inputField;

    private TouchScreenKeyboard touchscreenKeyboard;

    private void Update()
    {
        if (touchscreenKeyboard != null)
        {
            inputField.text = touchscreenKeyboard.text;

            // maybe a typo but the not was missing in the expression below (!)
            if (!TouchScreenKeyboard.visible)
            {
                touchscreenKeyboard = null;
            }

        }
    }

    public void OpenSystemKeyboard(string s)
    {
         touchscreenKeyboard = TouchScreenKeyboard.Open(s, TouchScreenKeyboardType.Default, false, false, false, false);
    }
}
