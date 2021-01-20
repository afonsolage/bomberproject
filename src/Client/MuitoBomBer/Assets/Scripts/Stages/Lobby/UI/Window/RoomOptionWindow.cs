using CommonLib.Messaging.Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomOptionWindow : UIComponent
{
    public UILabel _title;
    public UILabel _password;

    public UIToggle _checkBox;

    private void Start()
    {
        // Set new z value in transform, because if not will to glitch with models of players.
        Vector3 trans = transform.localPosition;
        trans.z = -100f;

        transform.localPosition = trans;
    }

    public void OnClickedConfirmBtn()
    {
        var inRoomWindow = _parent.FindInstance(WindowType.ROOM) as RoomWindow;
        if (string.Equals(_title, inRoomWindow.Title))
        {
            Close();

            return;
        }
        else if(_checkBox.value && string.IsNullOrEmpty(_password.text))
        {
            var msgHint = _parent.FindInstance(WindowType.MSG_HINT, true) as MessageHint;
            msgHint.AddMessageHint("You need to digit password.");

            return;
        }
        else
        {
            var lobbyStage = _parent.Stage as LobbyStage;
            lobbyStage.ServerConnection.Send(new CL_ROOM_SETTING_REQ()
            {
                title = _title.text,
                pw = (_checkBox.value) ? _password.text : string.Empty
            });

            inRoomWindow.Password = (_checkBox.value) ? _password.text : string.Empty;
        }
    }

    public void Close()
    {
        _parent.Destroy(WindowType.ROOM_OPTION);
    }

    public void OnClickedCancelBtn()
    {
        Close();
    }

    public void SetTitle(string title)
    {
        _title.text = title;
    }

    public void SetPassword(string pw)
    {
        if(!string.IsNullOrEmpty(pw))
        {
            _password.text = pw;
            _checkBox.value = true;
        }
        else
        {
            _password.text = string.Empty;
            _checkBox.value = false;
        }
    }

    public void OnValueChangePassword()
    {
        if(_checkBox.value)
        {
            _password.gameObject.SetActive(true);
        }
        else
        {
            _password.gameObject.SetActive(false);
            _password.text = string.Empty;
        }
    }
}
