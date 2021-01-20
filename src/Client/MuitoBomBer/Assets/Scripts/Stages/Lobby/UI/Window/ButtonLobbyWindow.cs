using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class ButtonLobbyWindow : UIComponent
{
    internal void OnClickedChat()
    {
        _parent.Instanciate(WindowType.CHAT);
    }
}
