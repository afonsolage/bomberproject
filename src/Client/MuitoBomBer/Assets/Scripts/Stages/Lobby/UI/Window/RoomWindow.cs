using CommonLib.Messaging.Client;
using CommonLib.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomWindow : UIComponent
{
    public readonly int MAX_SLOTS_IN_ROOM = 6;

    public UILabel _idRoom;
    public UILabel _titleRoom;

    public GameObject _passwordObject;
    public UILabel _passwordLabel;

    public UIButton _readyButton;
    public UIButton _startButton;

    public UIGrid _grid;

    public GameObject _roomObject;

    protected List<PlayerSlotComponent> _slots = new List<PlayerSlotComponent>();

    public UIButton _settingButton;

    // Information of room.
#pragma warning disable 0414
    private uint _index;
    private string _title;
    public string Title { get { return _title; } set { _title = value; } }

    private string _owner;
    private uint _maxPlayer;
    private uint _playerCnt;
    private uint _mapId;

    private string _password;
    public string Password { get { return _password; } set { _password = value; } }
#pragma warning restore 0414

    private void Start()
    {
        RefreshAll();
    }

    private void RemoveAllSlots()
    {
        if (_slots.Count > 0)
        {
            foreach (var slot in _slots)
            {
                NGUITools.Destroy(slot.gameObject);
            }

            _slots.Clear();
        }
    }

    private void RefreshSlots()
    {
        //if(_slots.Count != _maxPlayer)
        {
            RemoveAllSlots();

            //for (var i = 0; i < _maxPlayer; i++)
            for (var i = 0; i < MAX_SLOTS_IN_ROOM; i++)
            {
                var go = NGUITools.AddChild(_grid.gameObject, _roomObject);
                go.gameObject.name = string.Format("Slot_{0}", i);

                var component = go.GetComponent<PlayerSlotComponent>();
                component.SlotIndex = i;

                _slots.Add(component);
            }
        }

        _grid.Reposition();
    }

    internal void RefreshAll()
    {
        RefreshInfo();
        RefreshSlots();
        RefreshPlayers();
        RefreshButtons();
    }

    public void RefreshInfo()
    {
        var waitingRoom = CurrentStage<LobbyStage>().RoomController.WaitingRoom;
        var info = waitingRoom.Info;

        _index = info.Index;
        _title = info.Name;
        _owner = info.Owner;
        _maxPlayer = info.MaxPlayer;
        _playerCnt = info.PlayerCnt;
        _mapId = info.MapId;
        _password = info.Password;

        // Update on component.
        _idRoom.text = string.Format("{0}.", _index.ToString());
        _titleRoom.text = _title;

        if (info.IsPublic)
        {
            _passwordObject.gameObject.SetActive(true);
            _passwordLabel.text = _password;
        }
        else
        {
            _passwordObject.gameObject.SetActive(false);
            _passwordLabel.text = string.Empty;
        }
    }

    public void RefreshPlayers()
    {
        var waitingRoom = CurrentStage<LobbyStage>().RoomController.WaitingRoom;
        if (waitingRoom == null)
            return;

        var mainPlayer = CurrentStage<LobbyStage>().MainPlayer;
        var isOwner = waitingRoom.Info.Owner == mainPlayer?.Nick;

        foreach (var slot in _slots)
        {
            slot.Hide();
            slot.SetInteractableDragDropItem(isOwner);
        }

        foreach (var player in waitingRoom.Players)
        {
            var slot = FindSlot(waitingRoom.FindPlayerSlot(player.Index).SlotIndex);
            if (slot)
            {
                slot.Show(player.Nick, player.Index, player.Level, waitingRoom.Info.Owner == player.Nick);

                slot.SetPing(player.Ping);
                slot.Offline(player.Offline);
                slot.Ready(player.Ready);
            }
        }
    }

    internal void DragDropSlots(ulong playerIndex1, ulong playerIndex2, int oldIndex, int newIndex)
    {
        // Get two slots will be changed in position.
        var oldSlot = FindSlot(oldIndex);
        var newSlot = FindSlot(newIndex);

        if (oldSlot == null || newSlot == null)
        {
            CLog.E("Failed to find slot.");
            return;
        }

        // Keep player index in the slot.
        oldSlot.PlayerIndex = playerIndex1;
        newSlot.PlayerIndex = playerIndex2;

        // Change slot index for new index.
        oldSlot.SlotIndex = newIndex;
        newSlot.SlotIndex = oldIndex;

        // Update info in WaitingRoom.
        var waitingRoom = CurrentStage<LobbyStage>().RoomController.WaitingRoom;
        if (waitingRoom == null)
            return;

        var oldWaitingSlot = waitingRoom.FindSlotByIndex(oldIndex);
        var newWaitingSlot = waitingRoom.FindSlotByIndex(newIndex);

        oldWaitingSlot._playerIndex = playerIndex2;
        newWaitingSlot._playerIndex = playerIndex1;

        //        oldSlot.gameObject.name = string.Format("Slot_{0}", newIndex);
        //        newSlot.gameObject.name = string.Format("Slot_{0}", oldIndex);

        oldSlot._dragDropItem.DragDrop(newSlot.gameObject);
    }

    private PlayerSlotComponent FindSlot(int index)
    {
        foreach (var slot in _slots)
        {
            if (slot.SlotIndex == index)
                return slot;
        }

        throw new Exception("This should never happens. Something is wrong.");
    }

    public void OnClickedCloseBtn()
    {
        // Let to check menu is already instancied, if yes, let to destroy.
        _parent.FindInstance(WindowType.ROOM_PLAYER_MENU, false, true);

        CurrentStage<LobbyStage>().RoomController.LeaveWaitingRoom();
    }

    public void OnClickedOptionBtn()
    {
        if (!_parent.FindInstance(WindowType.ROOM_OPTION))
        {
            var optionRoom = _parent.Instanciate(WindowType.ROOM_OPTION) as RoomOptionWindow;
            if (optionRoom)
            {
                optionRoom.SetTitle(_title);
                optionRoom.SetPassword(_password);
            }
        }
    }

    public void OnClickedStartBtn()
    {
        var lobbyStage = _parent.Stage as LobbyStage;
        lobbyStage.ServerConnection.Send(new CL_ROOM_START_REQ() { });
    }

    public void OnClickedReadyBtn()
    {
        var lobbyStage = CurrentStage<LobbyStage>();
        var mainPlayer = lobbyStage.MainPlayer;

        lobbyStage.ServerConnection.Send(new CL_PLAYER_READY_REQ() { ready = !mainPlayer.Ready });
    }

    internal void RefreshButtons()
    {
        if (_readyButton == null || _startButton == null)
            return;

        var waitingRoom = CurrentStage<LobbyStage>().RoomController.WaitingRoom;

        if (waitingRoom == null)
            return;

        var owner = waitingRoom.Info.Owner;
        var mainPlayer = CurrentStage<LobbyStage>().MainPlayer;

        if (owner == mainPlayer.Nick)
        {
            _readyButton.gameObject.SetActive(false);
            _startButton.gameObject.SetActive(true);

            var btnEnabled = true;

            foreach (var player in waitingRoom.Players)
            {
                if (!player.Ready && player.Nick != owner)
                {
                    btnEnabled = false;
                    break;
                }
            }

            _startButton.isEnabled = btnEnabled;

            _settingButton.gameObject.SetActive(true);
        }
        else
        {
            _readyButton.gameObject.SetActive(true);
            _startButton.gameObject.SetActive(false);

            _settingButton.gameObject.SetActive(false);
        }
    }

    internal void ChangeSlotPosReq(int currentSlotIndex, int newSlotIndex)
    {
        var lobbyStage = CurrentStage<LobbyStage>();
        lobbyStage.ServerConnection.Send(new CL_ROOM_CHANGE_SLOT_POS_REQ()
        {
            currentSlot = currentSlotIndex,
            newSlot = newSlotIndex
        });
    }

    internal void RefreshPing(ulong playerIndex, long ping)
    {
        // Search current slot from player and let to update for current ping.
        foreach (var slot in _slots)
        {
            if (slot?.PlayerIndex == playerIndex)
            {
                // Player found, let to update ping and exit from function.
                slot.SetPing(ping);
                return;
            }
        }
    }
}
