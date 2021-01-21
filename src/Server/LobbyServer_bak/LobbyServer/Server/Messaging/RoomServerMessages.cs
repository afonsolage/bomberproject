using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib.Messaging.Lobby;
using LobbyServer.Server;
using CommonLib.Messaging;
using CommonLib.Util;
using CommonLib.Messaging.Client;
using CommonLib.Messaging.Common;

namespace LobbyServer.Server.Messaging
{
    internal static class RoomServerMessages
    {
        internal static void CreateRoomRes(LR_CREATE_ROOM_RES res, RoomServerSession session)
        {
            var room = session.App.RoomController.FindRoom(res.lobbyRoomIdx);

            if (room == null)
            {
                CLog.W("Unable to process create room response. Room id {0} was not found.", res.lobbyRoomIdx);
                //TODO: Send room destroy request.
            }
            else
            {
                if (res.error != MessageError.NONE)
                {
                    room.Owner.Session.Send(new CL_ROOM_CREATE_RES()
                    {
                        error = res.error,
                    });
                }
                else
                {
                    session.App.RoomController.OnRoomStart(room, res.roomIdx, session.Server);
                }
            }
        }

        internal static void RoomFinishedNfy(LR_ROOM_FINISHED_NFY nfy, RoomServerSession session)
        {
            var room = session.App.RoomController.FindRoomByRoomServerIndex(nfy.index);

            if (room == null)
            {
                CLog.W("Unable to process room finished notify. Room server id {0} was not found.", nfy.index);
                return;
            }

            //TODO: Add some exp, itens, and rewards to all players that were in the match based on MatchInfo.

            session.App.RoomController.OnRoomFinish(room);
        }

        internal static void UserCountNfy(LR_USER_COUNT_NFY nfy, RoomServerSession session)
        {
            if (session.Server != null)
            {
                session.Server.UserCount = (int)nfy.count;
            }
        }

        internal static void ServerInfoNfy(LR_SERVER_INFO_NFY nfy, RoomServerSession session)
        {
            var server = session.Server;
            if (server != null)
            {
                //In the future, if those infos grows bigger, we can use a struct to pass all those infos.
                session.App.RoomServerController.UpdateServerInfo(session.Server.UID, nfy.ip, nfy.port, nfy.capacity, true);
            }
        }
    }
}
