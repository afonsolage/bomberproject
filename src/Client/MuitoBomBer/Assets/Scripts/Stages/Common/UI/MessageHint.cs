using System.Collections.Generic;
using UnityEngine;

public class MessageHint : UIComponent
{
    public GameObject _messageHint;

    public void AddMessageHint(string text)
    {
        var go = NGUITools.AddChild(gameObject, _messageHint);

        var hint = go.GetComponent<MessageHintComponent>() as MessageHintComponent;
        if(hint)
        {
            hint.SetText(text);
        }
    }
}
