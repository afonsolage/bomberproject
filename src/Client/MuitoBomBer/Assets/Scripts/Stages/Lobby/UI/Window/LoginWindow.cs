using CommonLib.Messaging.Client;
using CommonLib.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonLib.Messaging;
using System;

public class LoginWindow : UIComponent
{
    public UIInput _loginInput;
    public UIInput _passInput;

    public UIButton _loginBtn;
    public UIButton _registerBtn;

    public UIButton _facebookBtn;

    // Register
    public UIInput _regLoginInput;
    public UIInput _regPasswordInput;
    public UIInput _regPassword2Input;
    public UIInput _regEmailInput;

    public UIButton _backBtn;
    public UIButton _toRegisterBtn;

    // Start is called just before any of the Update methods is called the first time
    private void Start()
    {
        OnClickedBackBtn();
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public void OnClickedBackBtn()
    {
        // Enable components of the login stage.
        _loginInput.gameObject.SetActive(true);
        _passInput.gameObject.SetActive(true);
        _loginBtn.gameObject.SetActive(true);
        _registerBtn.gameObject.SetActive(true);
        _facebookBtn.gameObject.SetActive(true);

        // Disable components of the register stage.
        _regLoginInput.gameObject.SetActive(false);
        _regPasswordInput.gameObject.SetActive(false);
        _regPassword2Input.gameObject.SetActive(false);
        _regEmailInput.gameObject.SetActive(false);
        _backBtn.gameObject.SetActive(false);
        _toRegisterBtn.gameObject.SetActive(false);
    }

    public void OnFacebookLoginBtn()
    {
        //var stage = StageManager.GetCurrent<LobbyStage>();
        //stage.ShowWaiting("Authenticating...");
        //stage.ServerConnection.Send(new CL_FB_AUTH_REQ()
        //{
        //    token = "123123123123123123123123123",
        //    id ="8454857546421313",
        //    name = "Afonso Lage",
        //    email = "lage.afonso@gmail.com",
        //});

        //FacebookLoginSDK.DoLogin((accessToken, id, name, email) =>
        //{
        //    if (accessToken == null)
        //    {
        //        StageManager.GetCurrent<LobbyStage>().ShowHint("Failed to authenticate. Please try again.");
        //    }
        //    else
        //    {
        //        CLog.D("Returned data from fb auth: {0}, {1}, {2}, {3}", accessToken, id, name, email);

        //        var stage = StageManager.GetCurrent<LobbyStage>();
        //        stage.ShowWaiting("Authenticating...");
        //        stage.ServerConnection.Send(new CL_FB_AUTH_REQ()
        //        {
        //            token = accessToken,
        //            id = id,
        //            name = name,
        //            email = email,
        //        });
        //    }
        //});
    }

    public void OnClickedToRegisterBtn()
    {
        if (string.IsNullOrEmpty(_regLoginInput.value))
        {
            var msgHint = _parent.FindInstance(WindowType.MSG_HINT, true) as MessageHint;
            msgHint.AddMessageHint("You need to digit your login.");

            if (!_regLoginInput.isSelected)
                _regLoginInput.isSelected = true;

            return;
        }

        if (string.IsNullOrEmpty(_regPasswordInput.value))
        {
            var msgHint = _parent.FindInstance(WindowType.MSG_HINT, true) as MessageHint;
            msgHint.AddMessageHint("You need digit your password.");

            if (!_regPasswordInput.isSelected)
                _regPasswordInput.isSelected = true;

            return;
        }

        if (string.IsNullOrEmpty(_regPassword2Input.value))
        {
            var msgHint = _parent.FindInstance(WindowType.MSG_HINT, true) as MessageHint;
            msgHint.AddMessageHint("You need digit your password.");

            if (!_regPassword2Input.isSelected)
                _regPassword2Input.isSelected = true;

            return;
        }

        if (string.IsNullOrEmpty(_regEmailInput.value))
        {
            var msgHint = _parent.FindInstance(WindowType.MSG_HINT, true) as MessageHint;
            msgHint.AddMessageHint("You need digit your email.");

            if (!_regEmailInput.isSelected)
                _regEmailInput.isSelected = true;

            return;
        }

        if (!IsValidEmail(_regEmailInput.value))
        {
            var msgHint = _parent.FindInstance(WindowType.MSG_HINT, true) as MessageHint;
            msgHint.AddMessageHint("You did not enter a valid email.");

            if (!_regEmailInput.isSelected)
                _regEmailInput.isSelected = true;

            return;
        }

        if (_regPasswordInput.value != _regPassword2Input.value)
        {
            var msgHint = _parent.FindInstance(WindowType.MSG_HINT, true) as MessageHint;
            msgHint.AddMessageHint("Passwords do not match.");

            // Reset values of the passwords.
            _regPasswordInput.value = "";
            _regPassword2Input.value = "";

            if (!_regPasswordInput.isSelected)
                _regPasswordInput.isSelected = true;

            return;
        }

        var lobbyStage = _parent.Stage as LobbyStage;

        lobbyStage.ServerConnection.Send(new CL_REGISTER_REQ()
        {
            login = _regLoginInput.value,
            password = _regPasswordInput.value,
            email = _regEmailInput.value
        });

        _backBtn.isEnabled = false;
        _toRegisterBtn.isEnabled = false;
    }

    public void OnClickedRegisterBtn()
    {
        // Disable components of the login stage.
        _loginInput.gameObject.SetActive(false);
        _passInput.gameObject.SetActive(false);
        _loginBtn.gameObject.SetActive(false);
        _registerBtn.gameObject.SetActive(false);
        _facebookBtn.gameObject.SetActive(false);

        // Enable components of the register stage.
        _regLoginInput.gameObject.SetActive(true);
        _regPasswordInput.gameObject.SetActive(true);
        _regPassword2Input.gameObject.SetActive(true);
        _regEmailInput.gameObject.SetActive(true);
        _backBtn.gameObject.SetActive(true);
        _toRegisterBtn.gameObject.SetActive(true);
    }

    internal void OnClickedLoginBtn()
    {
        if (string.IsNullOrEmpty(_loginInput.value))
        {
            var msgHint = _parent.FindInstance(WindowType.MSG_HINT, true) as MessageHint;
            msgHint.AddMessageHint("You need to digit your login");

            if (!_loginInput.isSelected)
                _loginInput.isSelected = true;

            return;
        }

        if (string.IsNullOrEmpty(_passInput.value))
        {
            var msgHint = _parent.FindInstance(WindowType.MSG_HINT, true) as MessageHint;
            msgHint.AddMessageHint("You need to digit your password;");

            if (!_passInput.isSelected)
                _passInput.isSelected = true;

            return;
        }

        var lobbyStage = _parent.Stage as LobbyStage;

        lobbyStage.ShowWaiting("Authenticating...");
        lobbyStage.ServerConnection.Send(new CL_AUTH_REQ()
        {
            login = _loginInput.value,
            pass = _passInput.value,
            deviceID = SystemInfo.deviceUniqueIdentifier,
        });
    }

    internal void AuthFailed(MessageError error)
    {
        var msgHint = _parent.FindInstance(WindowType.MSG_HINT, true) as MessageHint;

        switch (error)
        {
            case MessageError.AUTH_FAIL:
                msgHint.AddMessageHint("Invalid username or password! Please try again.");
                break;
            default:
                msgHint.AddMessageHint("Failed to authenticate. Please try again.");
                break;
        }

        _passInput.value = "";
    }
}
