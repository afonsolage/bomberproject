using CommonLib.Messaging;
using CommonLib.Messaging.Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCreationWindow : UIComponent
{
    public UI2DSprite _maleCharacter;
    public UI2DSprite _femaleCharacter;

    private PlayerGender _gender;

    public UILabel _name;

    private void Start()
    {
        var rand = new System.Random();

        SetSex((rand.Next((int)PlayerGender.None, (int)PlayerGender.Male + 1) == 0) ? true : false);
    }

    private void SetSex(bool male)
    {
        if(male)
        {
            _gender = PlayerGender.Male;

            _maleCharacter.gameObject.SetActive(true);
            _femaleCharacter.gameObject.SetActive(false);
        }
        else
        {
            _gender = PlayerGender.Female;

            _maleCharacter.gameObject.SetActive(false);
            _femaleCharacter.gameObject.SetActive(true);
        }
    }

    public void OnClickedMaleBtn()
    {
        SetSex(true);
    }

    public void OnClickedFemaleBtn()
    {
        SetSex(false);
    }

    private bool CheckValidName(string name)
    {
        // TODO : check if is a valid name.
        return true;
    }

    public void OnClickedCreateBtn()
    {
        if(string.IsNullOrEmpty(_name.text))
        {
            var msgHint = _parent.FindInstance(WindowType.MSG_HINT, true) as MessageHint;
            msgHint.AddMessageHint("You need to digit some name.");
            return;
        }

        if (!CheckValidName(_name.text))
        {
            var msgHint = _parent.FindInstance(WindowType.MSG_HINT, true) as MessageHint;
            msgHint.AddMessageHint("Invalid name.");
            return;
        }

        // Request player's on lobby.
        var lobbyStage = _parent.Stage as LobbyStage;
        lobbyStage.ServerConnection.Send(new CL_PLAYER_CREATE_REQ()
        {
            nick = _name.text,
            gender = _gender
        });
    }
}
