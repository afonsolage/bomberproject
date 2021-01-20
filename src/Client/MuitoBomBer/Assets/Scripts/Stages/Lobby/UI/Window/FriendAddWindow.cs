using CommonLib.Messaging.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

internal class FriendAddWindow : UIComponent
{
    [SerializeField]
    private UIInput _friendName;

    public void Start()
    {
        _friendName.value = "";
    }

    public void OnCancelClick()
    {
        Close();
    }

    public void OnConfirmClick()
    {
        //TODO: Maybe add some special caracteres or naming rules, the same use on character creation?

        if (_friendName.value != null && _friendName.value.Length > 0)
        {
            var lobby = StageManager.GetCurrent<LobbyStage>();
            lobby.ServerConnection.Send(new CL_FRIEND_REQUEST_REQ()
            {
                nick = _friendName.value,
            });

            lobby.ShowWaiting("Adding friend...");
        }
    }

    public void Close()
    {
        _parent.Destroy(WindowType.FRIEND_ADD);
    }

}
