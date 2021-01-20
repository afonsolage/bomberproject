using CommonLib.Messaging;
using CommonLib.Messaging.Client;
using CommonLib.Messaging.DB;
using CommonLib.Util;
using RoomServer.Logic.PowerUP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomServer.Server.Messaging
{
    static class DatabaseMessages
    {
        internal static void TokenPlayerRes(DX_TOKEN_PLAYER_RES res, DatabaseClient dbClient)
        {
            var session = dbClient.App.FindSessionByToken(res.token);
            if (session == null)
            {
                CLog.D("Session not found for login {0}", res.login);
                return;
            }

            if (res.error == MessageError.NONE)
            {
                //TODO: Save nick somewhere.
                session.DBID = res.info.index;
                session.Login = res.login;
                session.PlayerInfo = new PlayerInfo()
                {
                    index = res.info.index,
                    nick = res.info.nick,
                    gender = res.info.gender,
                    stage = res.info.stage,
                    state = res.info.state,
                    privilege = res.info.privilege,
                    firstLogin = res.info.firstLogin,
                    roomIndex = res.info.roomIndex,
                    roomSlotIndex = res.info.roomSlotIndex,
                    ping = res.info.ping,
                };

                session.Authenticated = true;

                session.Send(new CX_TOKEN_RES()
                {
                    error = res.error,
                });
            }
            else
            {
                //Failed to authenticate, force finish session.
                session.Stop();
            }

        }

        public static void ListCellTypeRes(DR_LIST_CELL_TYPES_RES res, DatabaseClient client)
        {
            var list = res.typeList;
            var app = client.App;

            if (list == null || list.Count == 0)
            {
                CLog.F("Unable to load cell types. No types returned from db request.");
                app.Quit();
            }

            app.RoomManager.Init(list);
        }

        internal static void ListMapRes(DR_LIST_MAP_RES res, DatabaseClient client)
        {
            var app = client.App;

            if (res.mapList == null)
            {
                CLog.F("Unable to load maps. No maps returned from db request.");
                app.Quit();
            }

            app.RoomManager.LoadMaps(res.mapList);
        }

        internal static void ListPowerUpRes(DR_LIST_POWERUP_RES res, DatabaseClient client)
        {
            var app = client.App;

            if (res.powerupList == null)
            {
                CLog.F("Unable to load powerups. No powerup returned from db request.");
                app.Quit();
            }

            PowerUpManager.Setup(res.powerupList);
        }
    }
}
