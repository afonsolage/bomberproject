using CommonLib.Messaging.Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMenuComponent : UIComponent
{
    public Camera uiCamera;

    public UILabel _nick;

    public UIButton _info;
    public UIButton _kick;
    public UIButton _owner;

    public UI2DSprite _background;

    private ulong _playerIndex = 0;
    private string _playerNick = string.Empty;

    protected Transform _trans;
    protected Vector3 _pos = Vector3.zero;
    protected Vector3 _size = Vector3.zero;

    private Vector2 _lastTouch = Vector2.zero;

    void Start()
    {
        _trans = transform;
        _pos = _trans.localPosition;

        if (uiCamera == null) uiCamera = NGUITools.FindCameraForLayer(gameObject.layer);

        SetPosition(_lastTouch.x, _lastTouch.y);
    }

    public void SetOwner(bool owner)
    {
        var lobbyStage = CurrentStage<LobbyStage>();
        var mainPlayer = lobbyStage?.MainPlayer;

        if (owner)
        {
            _background.height = 210;
            _background.width = 168;

            _kick.gameObject.SetActive(true);
            _owner.gameObject.SetActive(true);

            if(mainPlayer?.Index == _playerIndex)
            {
                _kick.enabled = false;
                _owner.enabled = false;

                _kick.SetState(UIButtonColor.State.Disabled, true);
                _owner.SetState(UIButtonColor.State.Disabled, true);
            }
            else
            {
                _kick.enabled = true;
                _owner.enabled = true;

                _kick.SetState(UIButtonColor.State.Normal, true);
                _owner.SetState(UIButtonColor.State.Normal, true);
            }
        }
        else
        {
            _background.height = 110;
            _background.width = 168;

            _kick.gameObject.SetActive(false);
            _owner.gameObject.SetActive(false);
        }
    }

    public void SetPlayerInfo(ulong idx, string nick)
    {
        _playerIndex = idx;
        _playerNick = nick;

        _nick.text = _playerNick;

        // 
        var stage = CurrentStage<LobbyStage>();
        var roomController = stage?.RoomController;
        var room = roomController?.FindRoom(stage.MainPlayer.RoomIndex);

        if (room?.Owner == stage.MainPlayer.Nick)
        {
            SetOwner(true);
        }
        else
        {
            SetOwner(false);
        }
    }

    public void SetLastTouch(float x, float y)
    {
        _lastTouch.x = x;
        _lastTouch.y = y;
    }

    public void SetPosition(float x, float y)
    {
        // Set value from last touch on screen.
        _pos.x = x;
        _pos.y = y;

        if (uiCamera != null)
        {
            // Since the screen can be of different than expected size, we want to convert
            // mouse coordinates to view space, then convert that to world position.
            _pos.x = Mathf.Clamp01(_pos.x / Screen.width);
            _pos.y = Mathf.Clamp01(_pos.y / Screen.height);

            // Calculate the ratio of the camera's target orthographic size to current screen size
            float activeSize = uiCamera.orthographicSize / _trans.parent.lossyScale.y;
            float ratio = (Screen.height * 0.5f) / activeSize;

            // Calculate the maximum on-screen size of the tooltip window
            Vector2 max = new Vector2(ratio * _size.x / Screen.width, ratio * _size.y / Screen.height);

            // Limit the tooltip to always be visible
            _pos.x = Mathf.Min(_pos.x, 1f - max.x);
            _pos.y = Mathf.Max(_pos.y, max.y);

            // Update the absolute position and save the local one
            _trans.position = uiCamera.ViewportToWorldPoint(_pos);
            _pos = _trans.localPosition;
            _pos.x = Mathf.Round(_pos.x);
            _pos.y = Mathf.Round(_pos.y);
        }
        else
        {
            // Don't let the tooltip leave the screen area
            if (_pos.x + _size.x > Screen.width) _pos.x = Screen.width - _size.x;
            if (_pos.y - _size.y < 0f) _pos.y = _size.y;

            // Simple calculation that assumes that the camera is of fixed size
            _pos.x -= Screen.width * 0.5f;
            _pos.y -= Screen.height * 0.5f;
        }

        _trans.localPosition = _pos;
    }

    public void OnClickedInfo()
    {
        OnClickedCloseBtn();
    }

    public void OnClickedKick()
    {
        var lobbyStage = CurrentStage<LobbyStage>();
        var mainPlayer = lobbyStage.MainPlayer;
        var roomIndex = mainPlayer.RoomIndex;
        var room = lobbyStage.RoomController.FindRoom(roomIndex);

        if (roomIndex == 0 || room == null)
        {
            var msgHint = _parent.FindInstance(WindowType.MSG_HINT, true) as MessageHint;
            msgHint?.AddMessageHint("General error. Something is not normal!");
        }
        else
        {
            lobbyStage.ServerConnection.Send(new CL_ROOM_KICK_PLAYER_REQ() { playerIndex = _playerIndex, roomIndex = mainPlayer.RoomIndex });
        }

        OnClickedCloseBtn();
    }

    public void OnClickedOwner()
    {
        var lobbyStage = CurrentStage<LobbyStage>();
        var mainPlayer = lobbyStage.MainPlayer;
        var roomIndex = mainPlayer.RoomIndex;
        var room = lobbyStage.RoomController.FindRoom(roomIndex);

        if (roomIndex == 0 || room == null)
        {
            var msgHint = _parent.FindInstance(WindowType.MSG_HINT, true) as MessageHint;
            msgHint?.AddMessageHint("General error. Something is not normal!");
        }
        else
        {
            lobbyStage.ServerConnection.Send(new CL_ROOM_TRANSFER_OWNER_REQ() { playerIndex = _playerIndex, roomIndex = mainPlayer.RoomIndex });
        }

        OnClickedCloseBtn();
    }

    public void OnClickedCloseBtn()
    {
        // Let to check menu is already instancied, if yes, let to destroy.
        _parent.FindInstance(WindowType.ROOM_PLAYER_MENU, false, true);
    }
}
