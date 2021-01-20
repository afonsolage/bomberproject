using CommonLib.Messaging.Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomPasswordWindow : UIComponent
{
    private uint _roomIndex;
    public uint RoomIndex { set { _roomIndex = value; } }

    public UILabel _roomPassword;

    public void OnClickedConfirmBtn()
    {
        if (string.IsNullOrEmpty(_roomPassword.text))
        {
            var msgHint = _parent.FindInstance(WindowType.MSG_HINT, true) as MessageHint;
            msgHint.AddMessageHint("You need to digit password.");
        }
        else
        {
            CurrentStage<LobbyStage>().ServerConnection.Send(new CL_ROOM_JOIN_REQ()
            {
                index = _roomIndex,
                password = _roomPassword.text
            });
        }
    }

    public void OnClickedCancelBtn()
    {
        Close();
    }

    public void Close()
    {
        _parent.Destroy(WindowType.ROOM_PASSWORD);
    }
}
