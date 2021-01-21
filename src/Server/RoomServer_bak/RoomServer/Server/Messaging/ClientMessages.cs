using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib.Messaging.Client;
using CommonLib.Messaging;
using RoomServer.Logic.Object;
using CommonLib.Util.Math;
using CommonLib.Messaging.Base;
using RoomServer.Logic;
using CommonLib.Util;
using CommonLib.Messaging.Lobby;
using CommonLib.Messaging.DB;

namespace RoomServer.Server.Messaging
{
    internal static class ClientMessages
    {
        internal static void RoomMessage(RawMessage message, ClientSession session)
        {
            var uid = session?.Player?.Map?.UID ?? 0;

            if (uid == 0)
            {
                CLog.D("Can't send a message to room from a session that didn't joined any room.");
                return;
            }

            session.App.RoomManager.AddRoomMessage(session.Player.Map.UID, new SessionMessage(session.Player.UID, message));
        }

        internal static void TokenReq(CX_TOKEN_REQ req, ClientSession session)
        {
            if (session.App.FindSessionByToken(req.token) != null)
            {
                session.Send(new CX_TOKEN_RES()
                {
                    error = MessageError.ALREADY_CONNECTED,
                });
                return;
            }

            session.Token = req.token;

            session.App.DBClient.Send(new DX_TOKEN_PLAYER_REQ()
            {
                token = req.token,
            });
        }

        internal static void JoinRoomReq(CR_JOIN_ROOM_REQ req, ClientSession session)
        {
            var app = session.App;
            var uid = req.uid;

            var room = app.RoomManager.Find(uid);

            if (room == null)
            {
                CLog.D("Trying to join a inexistent room: {0}", uid);
                session.Send(new CR_JOIN_ROOM_RES()
                {
                    error = MessageError.NOT_FOUND
                });
            }
            else
            {
                var existingPlayer = room.FindAllByType<Player>().Find(p => p.DBID == session.DBID);

                if (existingPlayer != null)
                {
                    // Send room joinning success.
                    session.Send(new CR_JOIN_ROOM_RES()
                    {
                        error = MessageError.NONE,
                        mainUID = existingPlayer.UID
                    });

                    existingPlayer.Reconnect(session);
                }
                else
                {
                    if (!room.HasFreeSlot())
                    {
                        CLog.D("Trying to join a full room: {0}", uid);
                        session.Send(new CR_JOIN_ROOM_RES()
                        {
                            error = MessageError.FULL
                        });

                        return;
                    }

                    var player = CreatePlayer(session, room);

                    // Send room joinning success.
                    session.Send(new CR_JOIN_ROOM_RES()
                    {
                        error = MessageError.NONE,
                        mainUID = player.UID
                    });

                    JoinPlayerOnRoom(player, room);
                }
            }
        }

        private static void JoinPlayerOnRoom(Player player, Room room)
        {
            // Add the player in the slot that was in the room.
            // If it is not possible to add to the current slot, try adding to a random slot
            if (!room.AllocSlot(player, room.GetPlayerSlot(player.Info.index), true))
            {
                CLog.F("This shouldn't happen. You changed something, fix it!");
                return;
            }

            //Send room joining info
            player.Session?.Send(new CR_JOIN_ROOM_NFY()
            {
                info = room.GetMapInfo(),
                typeList = room.GetMapTypeList(),
            });

            var world = room.GridToWorld(room.GetSpawnPos(player));
            player.Wrap(world); //TODO: Add spawn point on each map.
            player.EnterMap();
        }

        private static Player CreatePlayer(ClientSession session, Room room)
        {
            var player = new Player(session, room);
            session.Player = player;

            return player;
        }
    }
}
