using CommonLib.Messaging;
using CommonLib.Messaging.Client;
using CommonLib.Util;
using System.Collections.Generic;
using UnityEngine;

public class LobbyWindow : UIComponent
{
    // Prefab of the Room Window.
    public GameObject _roomObject;

    private List<GameObject> _lstRoomObjects = new List<GameObject>();

    // Components
    public UIGrid _gridRoom;
    public UIScrollView _scrollViewRoom;

    public UIInput _searchInput;

    public GameObject _playerListObject;
    public UIGrid _gridListPlayer;
    public UIScrollView _scrollViewPlayerList;

    private List<GameObject> _lstPlayerList = new List<GameObject>();

    // Use this for initialization
    void Start()
    {
        ListAllRooms();

        _gridRoom.Reposition();
        _scrollViewRoom.ResetPosition();
    }

    private void ListAllRooms()
    {
        var lobbyStage = _parent.Stage as LobbyStage;
        var rooms = lobbyStage.RoomController.AllRooms();

        foreach (var room in rooms)
        {
            AddRoom(room);
        }
    }

    private void ListRoomsMatching(string text)
    {
        var lobbyStage = _parent.Stage as LobbyStage;
        var rooms = lobbyStage.RoomController.FilterRooms(text);

        foreach (var room in rooms)
        {
            AddRoom(room);
        }
    }

    private void ClearRooms()
    {
        foreach (var go in _lstRoomObjects)
        {
            NGUITools.Destroy(go);
        }

        _lstRoomObjects.Clear();

        _gridRoom.Reposition();
        _scrollViewRoom.ResetPosition();
    }

    internal void AddRoom(RoomInfo room)
    {
        // Get grid from the scroll view.
        var gridObject = _gridRoom.gameObject;
        if (gridObject)
        {
            // Create new room.
            var go = NGUITools.AddChild(gridObject, _roomObject);
            var btn = go.GetComponent<UIButton>();
            EventDelegate.Set(btn.onClick, () => OnClickedJoinBtn(room.Index, room.IsPublic));

            go.name = "Room " + room.Index;

            // Add data from data.
            var data = go.AddComponent<RoomComponent.RoomData>();
            data.index = room.Index;
            data.title = room.Name;
            data.playerCnt = room.PlayerCnt;
            data.maxPlayer = room.MaxPlayer;
            data.isPublic = room.IsPublic;
            data.stage = room.RoomStage;

            var roomComponent = go.GetComponent<RoomComponent>();

            //Set index of the room.
            roomComponent.SetID(room.Index);

            // Set title of the room.
            roomComponent.SetTitle(room.Name);

            // Set quantity of the room.
            roomComponent.SetQuantity(room.PlayerCnt, room.MaxPlayer);

            // Enable lock sprite if room has password or disable if not has.
            roomComponent._password.gameObject.SetActive(room.IsPublic);

            roomComponent.SetStatusRoom(room.RoomStage);

            _lstRoomObjects.Add(go);

            _gridRoom.Reposition();
        }
    }

    internal void AddPlayerList(ulong playerIdx, string playerName, uint level, PlayerGender gender)
    {
        if (CheckPlayerList(playerIdx, playerName))
            return;

        // Get grid from the scroll view.
        var gridListObject = _gridListPlayer.gameObject;
        if (gridListObject)
        {
            var go = NGUITools.AddChild(gridListObject, _playerListObject);

            go.name = "Player " + playerIdx;

            var component = go.GetComponent<PlayerListComponent>();
            if(component)
            {
                component.SetBackgroundAlpha((_lstPlayerList.Count + 1) % 2 == 0);
                component.SetInformations(playerName, level, gender);
            }

            _lstPlayerList.Add(go);

            _gridListPlayer.Reposition();
        }
    }

    internal bool CheckPlayerList(ulong playerIdx, string playerName)
    {
        var go = _lstPlayerList.Find(g => g.name == "Player " + playerIdx);
        return (go != null) ? true : false;
    }

    internal void RemovePlayerList(ulong playerIdx, string playerName)
    {
        var go = _lstPlayerList.Find(g => g.name == "Player " + playerIdx);

        if (go != null)
        {
            _lstPlayerList.Remove(go);

            // We need to set parent to null, becayse Destroy won't destroy it immediately
            // Else UIGrid reposition won't notice it was gone.
            go.transform.parent = null;
            Destroy(go);

            _gridListPlayer.Reposition();
            _scrollViewPlayerList.ResetPosition();
        }
    }

    internal void ClearListPlayer()
    {
        foreach (var go in _lstPlayerList)
        {
            NGUITools.Destroy(go);
        }

        _lstPlayerList.Clear();
        _gridListPlayer.Reposition();
    }

    public void OnClickedCloseBtn()
    {
        ClearRooms();

        _parent.Destroy(WindowType.LOBBY);

        var main = _parent.FindInstance(WindowType.MAIN) as MainWindow;
        main?.DisableComponents(false);
    }

    public void OnClickedCreateBtn()
    {
        var lobbyStage = _parent.Stage as LobbyStage;
        lobbyStage?.ServerConnection.Send(new CL_ROOM_CREATE_REQ() { });
    }

    public void OnClickedQuickBtn()
    {
        CLog.D("QuickBtn!");
    }

    public void OnClickedJoinBtn(uint index, bool hasPassword)
    {
        if (hasPassword)
        {
            var roomPasswordWindow = _parent.Instanciate(WindowType.ROOM_PASSWORD) as RoomPasswordWindow;
            if (roomPasswordWindow)
                roomPasswordWindow.RoomIndex = index;
        }
        else
        {
            CurrentStage<LobbyStage>().ServerConnection.Send(new CL_ROOM_JOIN_REQ()
            {
                index = index,
                password = string.Empty
            });
        }
    }

    public void OnClickedSearchBtn()
    {
        ClearRooms();

        ListRoomsMatching(_searchInput.value);
    }

    public void OnClickedRefreshBtn()
    {
        _searchInput.value = "";
        ClearRooms();

        ListAllRooms();
    }

    internal void RemoveRoom(uint index)
    {
        var go = _lstRoomObjects.Find(g => g.name == "Room " + index);

        if (go != null)
        {
            _lstRoomObjects.Remove(go);

            // We need to set parent to null, becayse Destroy won't destroy it immediately
            // Else UIGrid reposition won't notice it was gone.
            go.transform.parent = null;
            Destroy(go);

            _gridRoom.Reposition();
        }
    }

    internal void UpdateRoom(RoomInfo room)
    {
        var go = _lstRoomObjects.Find(g => g.name == "Room " + room.Index);

        if (go != null)
        {
            // Add data from data.
            var data = go.GetComponent<RoomComponent.RoomData>();
            data.index = room.Index;
            data.title = room.Name;
            data.playerCnt = room.PlayerCnt;
            data.maxPlayer = room.MaxPlayer;
            data.isPublic = room.IsPublic;
            data.stage = room.RoomStage;

            var roomComponent = go.GetComponent<RoomComponent>();

            // Set title of the room.
            roomComponent.SetID(room.Index);

            // Set title of the room.
            roomComponent.SetTitle(room.Name);

            // Set quantity of the room.
            roomComponent.SetQuantity(room.PlayerCnt, room.MaxPlayer);

            // Set lock sprite if room has password.
            roomComponent._password.gameObject.SetActive(room.IsPublic);

            roomComponent.SetStatusRoom(room.RoomStage);
        }
    }
}
