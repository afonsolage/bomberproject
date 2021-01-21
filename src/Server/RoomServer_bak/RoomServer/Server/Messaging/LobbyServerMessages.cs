using CommonLib.Messaging.DB;
using CommonLib.Util;
using RoomServer.Logic.PowerUP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib.Messaging.Lobby;
using CommonLib.Messaging.Client;
using CommonLib.Messaging;
using RoomServer.Logic.Object;
using System.Diagnostics;

namespace RoomServer.Server.Messaging
{
    static class LobbyServerMessages
    {
        internal static void CreateRoomReq(LR_CREATE_ROOM_REQ req, LobbyServerClient client)
        {
            var app = client.App;
            var roomManager = app.RoomManager;

            var room = roomManager.CreateRoom(req.info.mapId, req.info.ownerLogin);
            room.InitSlotPlayers(req.info.slotPlayer);
            room.InitialPlayerCount = (int) req.info.playerCnt;

            if (room == null)
            {
                CLog.D("Failed to create room.");
                client.Send(new LR_CREATE_ROOM_RES()
                {
                    lobbyRoomIdx = req.lobbyRoomIdx,
                    error = MessageError.CREATE_FAIL
                });
            }
            else
            {
                //TODO: Change this to add only when configured by owner
                //room.BotCount = req.info.maxPlayer - req.info.playerCnt;
                room.BotCount = 0;

                //Send room creating success.
                client.Send(new LR_CREATE_ROOM_RES()
                {
                    error = MessageError.NONE,
                    lobbyRoomIdx = req.lobbyRoomIdx,
                    roomIdx = room.UID,
                });
            }
        }

        internal static void WelcomeNfy(LR_WELCOME_NFY nfy, LobbyServerClient client)
        {
            var baseId = nfy.uid;

            client.App.RoomManager.SetBaseID(baseId);

            client.Send(new LR_SERVER_INFO_NFY()
            {
                ip = client.App.PublicIP,
                port = client.App.PublicPort,
                capacity = client.App.Capacity,
            });
        }

        internal static void DummyJoinNfy(LR_DUMMY_JOIN_NFY nfy, LobbyServerClient client)
        {
            var app = client.App;

            var roomManager = app.RoomManager;

            var room = roomManager.Find(nfy.roomIndex);

            var dummy = new Dummy(app, room);
            dummy.SetLogin(nfy.login);

            if (!room.AllocSlot(dummy, room.GetPlayerSlot(nfy.dummyIndex)))
            {
                CLog.W("The room is full!");
                return;
            }

            var spawn = room.GetSpawnPos(dummy);
            dummy.Wrap(room.GridToWorld(spawn));
            dummy.EnterMap();
        }
    }
}
