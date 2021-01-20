using CommonLib.DB;
using CommonLib.Messaging;
using CommonLib.Messaging.Common;
using CommonLib.Messaging.DB;
using DBServer.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBServer.Query
{
    internal abstract class RoomServer
    {
        public static void StartupInfoReq(DR_STARTUP_INFO_REQ req, ClientSession session)
        {
            using (var conn = new DBConnection("RoomServer"))
            {
                //Due to GetBytes only accept column index, always keep data column at first.
                using (var reader = conn.Query("SELECT code, attributes FROM cell_type;"))
                {
                    var res = new DR_LIST_CELL_TYPES_RES()
                    {
                        typeList = new List<Tuple<int, int>>()
                    };

                    while (reader.Read())
                    {
                        res.typeList.Add(new Tuple<int, int>(reader.GetInt32("code"), reader.GetInt32("attributes")));
                    }

                    session.Send(res);
                }

                //Due to GetBytes only accept column index, always keep data column at first.
                using (var reader = conn.Query("SELECT M.data, M.id, M.name, M.width, M.height, M.player_cnt, M.background, MB.behaviour, coalesce(MB.settings, '') as settings FROM  map M INNER JOIN map_behaviour MB ON M.id = MB.map_id ORDER BY  M.id ASC;"))
                {
                    var res = new DR_LIST_MAP_RES()
                    {
                        mapList = new List<MAP_INFO>()
                    };

                    uint currentId = 0;
                    MAP_INFO info = null;
                    while (reader.Read())
                    {
                        currentId = reader.GetUInt32("id");

                        if (info == null || info.index != currentId)
                        {
                            info = new MAP_INFO
                            {
                                index = currentId,
                                width = reader.GetUInt16("width"),
                                height = reader.GetUInt16("height"),
                                playerCnt = reader.GetUInt16("player_cnt"),
                                background = reader.GetUInt16("background")
                            };

                            info.data = new byte[(info.width * info.height) + (info.playerCnt * 2) + 1];
                            var readCount = reader.GetBytes(0, 0, info.data, 0, info.data.Length);

                            Debug.Assert(readCount == info.data.Length);

                            info.behaviour = new Dictionary<string, string>();

                            res.mapList.Add(info);
                        }

                        info.behaviour.Add(reader.GetString("behaviour"), reader.GetString("settings"));
                    }

                    session.Send(res);
                }

                using (var reader = conn.Query("SELECT  P.id, P.name, P.icon, P.rate, PB.behaviour, coalesce(PB.settings, '') as settings FROM powerup P INNER JOIN powerup_behaviour PB ON P.id = PB.powerup_id ORDER BY P.id;"))
                {
                    var res = new DR_LIST_POWERUP_RES()
                    {
                        powerupList = new List<POWERUP>()
                    };

                    uint currentId = 0;
                    POWERUP info = null;
                    while (reader.Read())
                    {
                        currentId = reader.GetUInt32("id");

                        if (info == null || info.index != currentId)
                        {
                            info = new POWERUP
                            {
                                index = currentId,
                                name = reader.GetString("name"),
                                icon = reader.GetUInt32("icon"),
                                rate = reader.GetFloat("rate")
                            };

                            info.behaviour = new Dictionary<string, string>();

                            res.powerupList.Add(info);
                        }

                        info.behaviour.Add(reader.GetString("behaviour"), reader.GetString("settings"));
                    }

                    session.Send(res);
                }
            }
        }
    }
}
