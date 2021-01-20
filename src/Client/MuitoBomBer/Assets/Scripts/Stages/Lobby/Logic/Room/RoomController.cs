using CommonLib.Messaging.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class RoomController
{
    private LobbyStage _stage;
    public LobbyStage Stage { get { return _stage; } }

    private readonly List<RoomInfo> _rooms;

    public WaitingRoom WaitingRoom;

    public RoomController(LobbyStage stage)
    {
        _stage = stage;
        _rooms = new List<RoomInfo>();
        WaitingRoom = new WaitingRoom();
    }

    public RoomInfo AddRoom(uint index)
    {
        var res = new RoomInfo(index);

        lock (_rooms)
        {
            _rooms.Add(res);
        }

        return res;
    }

    public void RemoveRoom(uint index)
    {
        lock (_rooms)
        {
            _rooms.RemoveAll((r) => r.Index == index);
        }
    }

    public void RemoveAllRooms()
    {
        lock (_rooms)
        {
            _rooms.Clear();
        }
    }

    public RoomInfo FindRoom(uint index)
    {
        lock (_rooms)
        {
            return _rooms.Find((r) => r.Index == index);
        }
    }

    public RoomInfo[] AllRooms()
    {
        lock (_rooms)
        {
            var res = new RoomInfo[_rooms.Count];
            _rooms.CopyTo(res);

            return res;
        }
    }

    public RoomInfo[] FilterRooms(string match)
    {
        lock (_rooms)
        {
            var filtered = _rooms.FindAll((r) => r.Name.Contains(match));
            var res = new RoomInfo[filtered.Count];
            filtered.CopyTo(res);

            return res;
        }
    }

    public void EnterWaitingRoom(RoomInfo roomInfo)
    {
        WaitingRoom.Update(roomInfo);

        Stage.UIManager.Destroy(WindowType.LOBBY);

        var menuWindow = Stage.UIManager.FindInstance(WindowType.MAIN) as MainWindow;
        menuWindow?.DisableComponents(true);

        var room = Stage.UIManager.Instanciate(WindowType.ROOM) as RoomWindow;
        room?.RefreshAll();
    }

    public void LeaveWaitingRoom(bool kicked = false)
    {
        Stage.UIManager.Destroy(WindowType.ROOM);
        Stage.UIManager.Instanciate(WindowType.LOBBY);
        
        WaitingRoom.Clear();

        // If the player has been kicked out of the room, no need to send a packet to the server that he left the room.
        if (!kicked)
        {
            Stage.ServerConnection.Send(new CL_ROOM_LEAVE_REQ() { });
        }
    }

    public void Clear()
    {
        RemoveAllRooms();
        _stage = null;
    }
}
