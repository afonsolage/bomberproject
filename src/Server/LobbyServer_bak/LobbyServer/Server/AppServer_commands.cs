using CommonLib.Messaging.Common;
using CommonLib.Messaging.Lobby;
using CommonLib.Server;
using CommonLib.Util;
using LobbyServer.Logic;
using LobbyServer.Logic.OAuth;
using LobbyServer.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyServer.Server
{
    internal partial class AppServer : GameLoopServer
    {
        #region PROCESS COMMAND
        protected override void ProcessCommand(string[] command)
        {
            base.ProcessCommand(command);

            var cmd = command[0];

            switch (cmd)
            {
                case "room":
                    ProcessRoomCommand(command);
                    break;
                case "player":
                    ProcessPlayerCommand(command);
                    break;
                case "dummy":
                    ProcessDummyCommand(command);
                    break;
                case "dev":
                    ProcessDevCommand(command);
                    break;
                case "server":
                    ProcessServerCommand(command);
                    break;
#if _DEBUG
                case "test":
                    FacebookLogin.CheckLoginToken(command[1], (isValid) => CLog.D("IsValid? {0}", isValid));
                    break;
#endif
                default:
                    CLog.W("Unkown command: {0}", cmd);
                    break;
            }
        }
        #endregion

        #region ROOM COMMAND

        protected void ProcessRoomCommand(string[] command)
        {
            if (command.Length < 2)
            {
                PrintRoomCommandUsage();
                return;
            }

            switch (command[1])
            {
                case "list":
                    ProcessRoomListCommand();
                    break;
                case "create":
                    ProcessRoomCreateCommand(command);
                    break;
                case "destroy":
                    ProcessRoomDestroyCommand(command);
                    break;
                case "start":
                    ProcessStartCommand(command);
                    break;
                default:
                    PrintRoomCommandUsage();
                    break;
            }
        }

        private void ProcessRoomDestroyCommand(string[] command)
        {
            if (command.Length < 3 || !uint.TryParse(command[2], out var uid))
            {
                PrintRoomCommandUsage("destroy");
                return;
            }

            var room = RoomController.FindRoom(uid);

            if (room == null)
            {
                CLog.I("Failed to start room. Unable to find a room with id {0}", uid);
                return;
            }

            //Just need to remove all players and the room will be destroyed
            foreach (var player in room.ListAllPlayers())
            {
                //Since we are removing all players, we should skip some checking to avoid sending extra packets.
                room.Leave(player, true);
            }

            RoomController.DestroyRoom(uid);

            CLog.I("Room {0} was destroyed.", uid);
        }

        private void ProcessStartCommand(string[] command)
        {
            if (command.Length < 3 || !uint.TryParse(command[2], out var uid))
            {
                PrintRoomCommandUsage("start");
                return;
            }

            var room = RoomController.FindRoom(uid);

            if (room == null)
            {
                CLog.I("Failed to start room. Unable to find a room with id {0}", uid);
                return;
            }

            var server = RoomServerController.FindBestServer();

            if (server == null)
            {
                CLog.I("Failed to start room. Unable to find a room server.");
                return;
            }

            if (server.Session == null || !server.Session.IsActive)
            {
                CLog.I("Failed to start room. Room Server is offline.");
                return;
            }

            server.Session.Send(new LR_CREATE_ROOM_REQ()
            {
                lobbyRoomIdx = room.Index,
                info = new CREATE_ROOM_INFO()
                {
                    index = room.Index,
                    mapId = room.MapId,
                    name = room.Title,
                    ownerLogin = room.Owner.Session.Login,
                    maxPlayer = room.MaxPlayer,
                    playerCnt = room.PlayerCnt,
                    stage = room.Stage,
                    hasPassword = (!string.IsNullOrEmpty(room.Password)) ? true : false,
                    slotPlayer = room.ListAllSlots(),
                },
            });
        }

        private void ProcessRoomCreateCommand(string[] command)
        {
            if (command.Length < 3)
            {
                PrintRoomCommandUsage("create");
                return;
            }

            Player player;

            if (uint.TryParse(command[2], out var uid))
            {
                player = PlayerController.Find(uid);
            }
            else
            {
                player = PlayerController.Find(command[2]);
            }

            if (player == null)
            {
                CLog.W("Failed to create room. Unable to find player {0}", (uid > 0) ? "with id " + uid : command[2]);
                return;
            }
            else
            {
                var room = RoomController.CreateRoom(player);

                if (room == null)
                {
                    CLog.W("Failed to create room. Unkown error.");
                }
                else
                {
                    CLog.I("Room {0} created.", room.Index);
                }
            }
        }

        private void ProcessRoomListCommand()
        {
            var rooms = RoomController.ListAllRooms();

            if (rooms.Count == 0)
            {
                CLog.I("There are no rooms.");
                return;
            }

            foreach (var room in rooms)
            {
                CLog.I(room.ToString());
            }
        }

        protected void PrintRoomCommandUsage(string subCommand = null)
        {
            switch (subCommand)
            {
                case "create":
                    CLog.W("Room create command syntax: room create <owner name|owner id>");
                    break;
                case "destroy":
                    CLog.W("Room destroy command syntax: room destroy <room id>");
                    break;
                case "start":
                    CLog.W("Room start command syntax: room start <room id>");
                    break;
                default:
                    CLog.W("Room command syntax: room <list|create|destroy|start>");
                    break;
            }
        }

        #endregion

        #region PLAYER COMMAND

        protected void ProcessPlayerCommand(string[] command)
        {
            if (command.Length < 2)
            {
                PrintPlayerCommandUsage();
                return;
            }

            switch (command[1])
            {
                case "list":
                    ProcessPlayerListCommand();
                    break;
                case "disconnect":
                    ProcessPlayerDisconnectCommand(command);
                    break;
                default:
                    PrintPlayerCommandUsage();
                    break;
            }
        }

        private void ProcessPlayerDisconnectCommand(string[] command)
        {
            if (command.Length < 3)
            {
                PrintPlayerCommandUsage("disconnect");
                return;
            }

            Player player;

            if (uint.TryParse(command[2], out var uid))
            {
                player = PlayerController.Find(uid);
            }
            else
            {
                player = PlayerController.Find(command[2]);
            }

            if (player == null)
            {
                CLog.I("Failed to disconnect player. Unable to find player {0}", (uid > 0) ? "with id " + uid : command[2]);
                return;
            }
            else
            {
                player.Disconnect();
                CLog.I("Player {0} was disconnected successfully", (uid > 0) ? "with id " + uid : command[2]);
            }
        }

        private void ProcessPlayerListCommand()
        {
            var players = PlayerController.ListAllPlayers();

            if (players.Count == 0)
            {
                CLog.I("There are no players.");
                return;
            }

            foreach (var player in players)
            {
                CLog.I(player.ToString());
            }
        }

        protected void PrintPlayerCommandUsage(string subCommand = null)
        {
            switch (subCommand)
            {
                case "disconnect":
                    CLog.W("Player disconnect command syntax: player disconnect <login|id>");
                    break;
                default:
                    CLog.W("Player command syntax: room <list|disconnect>");
                    break;
            }
        }

        #endregion

        #region DUMMY COMMAND

        protected void ProcessDummyCommand(string[] command)
        {
            if (command.Length < 2)
            {
                PrintDummyCommandUsage();
                return;
            }

            switch (command[1])
            {
                case "list":
                    ProcessDummyListCommand(command);
                    break;
                case "create":
                    ProcessDummyCreateCommand(command);
                    break;
                case "destroy":
                    ProcessDummyDestroyCommand(command);
                    break;
                case "join":
                    ProcessDummyJoinCommand(command);
                    break;
                case "leave":
                    ProcessDummyLeaveCommand(command);
                    break;
                case "offline":
                    ProcessDummyOfflineCommand(command);
                    break;
                case "online":
                    ProcessDummyOnlineCommand(command);
                    break;
                case "ready":
                    ProcessDummyReadyCommand(command);
                    break;
                case "notready":
                    ProcessDummyNotReadyCommand(command);
                    break;
                default:
                    PrintPlayerCommandUsage();
                    break;
            }
        }

        private void ProcessDummyReadyCommand(string[] command)
        {
            if (command.Length < 3 || !uint.TryParse(command[2], out var uid))
            {
                PrintDummyCommandUsage("notready");
                return;
            }

            var dummy = PlayerController.Find(uid) as Dummy;

            if (dummy == null)
            {
                CLog.I("Unable to find a dummy with id {0}", uid);
            }
            else
            {
                dummy.Ready = true;
            }
        }

        private void ProcessDummyNotReadyCommand(string[] command)
        {
            if (command.Length < 3 || !uint.TryParse(command[2], out var uid))
            {
                PrintDummyCommandUsage("notready");
                return;
            }

            var dummy = PlayerController.Find(uid) as Dummy;

            if (dummy == null)
            {
                CLog.I("Unable to find a dummy with id {0}", uid);
            }
            else
            {
                dummy.Ready = false;
            }
        }

        private void ProcessDummyOnlineCommand(string[] command)
        {
            if (command.Length < 3 || !uint.TryParse(command[2], out var uid))
            {
                PrintDummyCommandUsage("offline");
                return;
            }

            var dummy = PlayerController.Find(uid) as Dummy;

            if (dummy == null)
            {
                CLog.I("Unable to find a dummy with id {0}", uid);
            }
            else
            {
                dummy.Reconnect(new DummySession(this));
            }
        }

        private void ProcessDummyOfflineCommand(string[] command)
        {
            if (command.Length < 3 || !uint.TryParse(command[2], out var uid))
            {
                PrintDummyCommandUsage("offline");
                return;
            }

            if (!(PlayerController.Find(uid) is Dummy dummy))
            {
                CLog.I("Unable to find a dummy with id {0}", uid);
            }
            else
            {
                (dummy.Session as DummySession).RemoteDisconnect = true;
                PlayerController.OnDisconnect(dummy);
            }
        }

        private void ProcessDummyLeaveCommand(string[] command)
        {
            if (command.Length < 3 || !uint.TryParse(command[2], out var uid))
            {
                PrintDummyCommandUsage("leave");
                return;
            }

            var dummy = PlayerController.Find(uid) as Dummy;

            if (dummy == null)
            {
                CLog.I("Unable to find a dummy with id {0}", uid);
            }
            else if (dummy.Room == null)
            {
                CLog.I("Dummy {0} hasn't joined a room.", uid);
            }
            else
            {
                var roomUID = dummy.Room.Index;

                dummy.Room.Leave(dummy);
                CLog.I("Dummy {0} has left room {1}", uid, roomUID);
            }
        }

        private void ProcessDummyJoinCommand(string[] command)
        {
            if (command.Length < 4 || !uint.TryParse(command[2], out var dummyUID) || !uint.TryParse(command[3], out var roomUID))
            {
                PrintDummyCommandUsage("join");
                return;
            }

            var dummy = PlayerController.Find(dummyUID) as Dummy;

            if (dummy == null)
            {
                CLog.I("Unable to find a dummy with id {0}", dummyUID);
                return;
            }

            var room = RoomController.FindRoom(roomUID);

            if (room == null)
            {
                CLog.I("Unable to find a room with id {0}", roomUID);
                return;
            }

            if (dummy.Room == room)
            {
                CLog.I("Dummy {0} is already on froom {1}", dummyUID, roomUID);
                return;
            }

            if (room.IsFull())
            {
                CLog.I("unable to join dummy {0} on room {1}. Room is full.", dummyUID, roomUID);
                return;
            }

            if (room.Join(dummy))
            {
                CLog.I("Dummy {0} joined room {1}.", dummyUID, roomUID);
            }
        }

        private void ProcessDummyDestroyCommand(string[] command)
        {
            if (command.Length < 3 || !uint.TryParse(command[2], out var uid))
            {
                PrintDummyCommandUsage("destroy");
                return;
            }

            var dummy = PlayerController.Find(uid) as Dummy;

            if (dummy == null)
            {
                CLog.I("Unable to find a dummy with id {0}", uid);
            }
            else
            {
                PlayerController.Destroy(dummy);
                CLog.I("Dummy {0} was destroyed successfully.", uid);
            }
        }

        private void ProcessDummyListCommand(string[] command)
        {
            var dummies = PlayerController.ListAllDummies();

            if (dummies.Count == 0)
            {
                CLog.I("There are no dummies.");
                return;
            }

            foreach (var dummy in dummies)
            {
                CLog.I(dummy.ToString());
            }
        }

        private void ProcessDummyCreateCommand(string[] command)
        {
            var dummy = PlayerController.CreateDummy();

            CLog.I("Dummy {0} was created.", dummy.Index);
        }

        protected void PrintDummyCommandUsage(string subCommand = null)
        {
            switch (subCommand)
            {
                case "destroy":
                    CLog.W("Dummy destroy command syntax: dummy destroy <id>");
                    break;
                case "join":
                    CLog.W("Dummy join room command syntax: dummy join <dummy id> <room id>");
                    break;
                case "leave":
                    CLog.W("Dummy leave room command syntax: dummy leave <dummy id>");
                    break;
                case "offline":
                    CLog.W("Dummy offline command syntax: dummy offline <dummy id>");
                    break;
                case "online":
                    CLog.W("Dummy online command syntax: dummy online <dummy id>");
                    break;
                case "ready":
                    CLog.W("Dummy ready command syntax: dummy ready <dummy id>");
                    break;
                case "notready":
                    CLog.W("Dummy not ready command syntax: dummy notready <dummy id>");
                    break;
                default:
                    CLog.W("Dummy command syntax: room <list|create|destroy|join|leave>");
                    break;
            }
        }

        #endregion

        #region SERVER COMMAND

        protected void ProcessServerCommand(string[] command)
        {
            if (command.Length < 2)
            {
                PrintRoomCommandUsage();
                return;
            }

            switch (command[1])
            {
                case "list":
                    ProcessServerListCommand();
                    break;
                default:
                    PrintServerCommandUsage();
                    break;
            }
        }

        protected void PrintServerCommandUsage(string subCommand = null)
        {
            switch (subCommand)
            {
                default:
                    CLog.W("Server command syntax: server <list>");
                    break;
            }
        }

        private void ProcessServerListCommand()
        {
            var servers = RoomServerController.ListAllServers();

            if (servers.Count == 0)
            {
                CLog.I("There are no room servers.");
                return;
            }

            foreach (var server in servers)
            {
                CLog.I(server.ToString());
            }
        }


        #endregion

        //#if _DEBUG
        #region DEV COMMAND
        protected void ProcessDevCommand(string[] command)
        {
            if (command.Length < 2)
            {
                PrintDevCommandUsage();
                return;
            }

            switch (command[1])
            {
                case "dummyrooms":
                    ProcessDummyRoomsCommand(command);
                    break;
                case "loadtest":
                    ProcessLoadTestCommand(command);
                    break;
                default:
                    PrintDevCommandUsage();
                    break;
            }
        }

        private void ProcessDummyRoomsCommand(string[] command)
        {
            var rnd = new Random(Guid.NewGuid().GetHashCode());

            var dummyCnt = rnd.Next(81, 151);
            var dummies = new List<Dummy>();

            for (var i = 0; i < dummyCnt; i++)
                dummies.Add(PlayerController.CreateDummy());

            var dummiesPerRoom = new List<List<Dummy>>();
            var total = 0;
            while (total < dummies.Count)
            {
                var cnt = rnd.Next(1, 6);
                if (cnt + total >= dummies.Count)
                    cnt = dummies.Count - total;

                if (cnt < 1)
                    break;

                dummiesPerRoom.Add(new List<Dummy>(dummies.GetRange(total, cnt)));
                total += cnt;
            }

            foreach (var dummiesToJoin in dummiesPerRoom)
            {
                var room = RoomController.CreateRoom(dummiesToJoin[0]);

                if (room == null)
                    continue;

                for (var i = 1; i < dummiesToJoin.Count; i++)
                    room.Join(dummiesToJoin[i]);
            }

            CLog.I("Created 100 Dummies spread over {0} rooms", dummiesPerRoom.Count);
        }

        private void ProcessLoadTestCommand(string[] command)
        {
            if (command.Length < 3)
            {
                CLog.W("Load test command syntax: dev loadtest <start|stop>");
                return;
            }

            switch (command[2])
            {
                case "start":
                    {
                        var cnt = command.Length > 3 ? int.Parse(command[3]) : 0;

                        var load = LoadTest.Instance;

                        if (load == null)
                        {
                            load = (cnt > 0) ? LoadTest.Create(this, cnt) : LoadTest.Create(this);
                        }

                        load.Start();
                    }
                    break;
                case "stop":
                    LoadTest.Instance?.Stop();
                    break;
                default:
                    CLog.W("Load test command syntax: dev loadtest <start|stop>");
                    break;
            }
        }

        protected void PrintDevCommandUsage(string subCommand = null)
        {
            switch (subCommand)
            {
                default:
                    CLog.W("Dev command syntax: dev <dummyrooms>");
                    break;
            }
        }
        #endregion
        //#endif
    }
}
