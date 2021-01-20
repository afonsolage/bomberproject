using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MsgBox : UIComponent
{
    public UILabel textLabel;

    internal void Start()
    {

    }

    internal void SetText(string text)
    {
        textLabel.text = text;
    }

    internal void OnOKClick()
    {
        _parent.Destroy(WindowType.MSG_BOX);
    }
}
