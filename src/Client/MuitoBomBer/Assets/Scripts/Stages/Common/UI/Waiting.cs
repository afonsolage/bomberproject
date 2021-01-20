using System.Collections.Generic;
using UnityEngine;

public class Waiting : UIComponent
{
    public GameObject _message;

    public void AddWaitingMessage(string text, EventDelegate.Callback onCancel = null)
    {
        var go = NGUITools.AddChild(gameObject, _message);

        var msg = go.GetComponent<WaitingComponent>() as WaitingComponent;
        if(msg)
        {
            msg.SetText(text);
            msg.ShowCancel(onCancel != null);
            if (onCancel != null)
            {
                msg.SetOnCancel(onCancel);
            }
        }
    }
}
