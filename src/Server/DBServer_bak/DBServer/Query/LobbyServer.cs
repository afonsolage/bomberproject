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
    internal abstract class LobbyServer
    {
        public static void StartupInfoReq(DL_STARTUP_INFO_REQ req, ClientSession session)
        {
            using (var conn = new DBConnection("LobbyServer"))
            {
                using (var reader = conn.Query("SELECT id, name, width, height, player_cnt, background FROM map ORDER BY id"))
                {
                    var res = new DL_LIST_MAP_RES()
                    {
                        mapList = new List<MAP_INFO>()
                    };

                    while (reader.Read())
                    {
                        res.mapList.Add(new MAP_INFO
                        {
                            index = reader.GetUInt32("id"),
                            name = reader.GetString("name"),
                            width = reader.GetUInt16("width"),
                            height = reader.GetUInt16("height"),
                            playerCnt = reader.GetUInt16("player_cnt"),
                            background = reader.GetUInt16("background")
                        });
                    }

                    session.Send(res);
                }

                //TODO: Add to send some startup info to room server, like guilds, achivements, etc.
            }
        }

        internal static void FbAuthReq(DL_FB_AUTH_REQ req, ClientSession session)
        {
            using (var conn = new DBConnection("LobbyServer"))
            {
                //Due to GetBytes only accept column index, always keep data column at first.
                using (var reader = conn.Query("CALL fn_fb_auth(@p1, @p2, @p3, @p4)", req.token, req.id, req.name, req.token))
                {
                    if (!reader.Read())
                    {
                        session.Send(new DL_FB_AUTH_RES()
                        {
                            error = MessageError.AUTH_FAIL,
                            login = req.id,
                        });
                    }
                    else
                    {
                        bool first = reader.GetBoolean("first_login");
                        string name = reader.GetString("nick");

                        session.Send(new DL_FB_AUTH_RES()
                        {
                            error = MessageError.NONE,
                            login = req.id,

                            info = new PLAYER_INFO()
                            {
                                index = reader.GetUInt64("id"),
                                nick = name,
                                gender = (PlayerGender)reader.GetByte("sex"),
                                level = reader.GetUInt32("level"),
                                experience = reader.GetUInt64("experience"),
                                firstLogin = first,
                                privilege = reader.GetByte("privilege")
                            }
                        });
                    }
                }
            }
        }

        internal static void PlayerAddInfoReq(DL_PLAYER_ADD_INFO_REQ req, ClientSession session)
        {
            using (var conn = new DBConnection("LobbyServer"))
            {
                //Friends
                var friends = new List<FRIEND_INFO>();

                using (var reader = conn.Query("CALL list_friendship(@p1)", req.index))
                {
                    while (reader.Read())
                    {
                        var state = reader.GetInt32(3);
                        friends.Add(new FRIEND_INFO()
                        {
                            index = reader.GetUInt32(0),
                            login = reader.GetString(1),
                            nick = reader.GetString(2),
                            state = state == 0 ? FriendState.Offline : state == 1 ? FriendState.Requested : FriendState.WaitingApproval,
                        });
                    }
                }

                //TODO: Add more additional info about player, like items, exp, level, guild, etc

                //Send all info
                session.Send(new DL_PLAYER_ADD_INFO_RES()
                {
                    index = req.index,
                    friends = friends,
                });
            }
        }

        internal static void FriendRemoveReq(DL_FRIEND_REMOVE_REQ req, ClientSession session)
        {
            using (var conn = new DBConnection("LobbyServer"))
            {
                using (var reader = conn.Query("CALL remove_friendship(@p1, @p2);", req.nick, req.friend))
                {
                    var error = MessageError.FAIL;

                    if (reader.Read())
                    {
                        switch (reader.GetInt32(0))
                        {
                            case -1:
                                error = MessageError.INVALID_REQUESTER_ID;
                                break;
                            case -2:
                                error = MessageError.INVALID_REQUESTED_ID;
                                break;
                            case -3:
                                error = MessageError.NOT_FOUND;
                                break;
                            case 1:
                                error = MessageError.NONE;
                                break;
                        }
                    }

                    session.Send(new DL_FRIEND_REMOVE_RES()
                    {
                        error = error,
                        nick = req.nick,
                        friend = req.friend,
                    });
                }
            }
        }

        internal static void FriendResponseReq(DL_FRIEND_RESPONSE_REQ req, ClientSession session)
        {
            using (var conn = new DBConnection("LobbyServer"))
            {
                using (var reader = conn.Query("CALL response_friendship(@p1, @p2, @p3);", req.requester, req.requested, req.accept))
                {
                    var msg = new DL_FRIEND_RESPONSE_RES()
                    {
                        error = MessageError.FAIL,
                        requester = req.requester,
                        requested = req.requested,
                        accept = req.accept,
                    };

                    if (reader.Read())
                    {
                        var res = reader.GetInt64(0);
                        switch (res)
                        {
                            case -1:
                                msg.error = MessageError.INVALID_REQUESTER_ID;
                                break;
                            case -2:
                                msg.error = MessageError.INVALID_REQUESTED_ID;
                                break;
                            case -3:
                                msg.error = MessageError.NOT_FOUND;
                                break;
                            case -4:
                                msg.error = MessageError.ALREADY_FRIENDS;
                                break;
                            default:
                                if (res > 0)
                                {
                                    msg.error = MessageError.NONE;
                                    msg.friendIndex = (ulong)res;
                                    msg.friendLogin = reader.GetString(1);
                                }
                                break;
                        }
                    }

                    session.Send(msg);
                }
            }
        }

        internal static void FriendRequestReq(DL_FRIEND_REQUEST_REQ req, ClientSession session)
        {
            using (var conn = new DBConnection("LobbyServer"))
            {
                using (var reader = conn.Query("CALL request_friendship(@p1, @p2);", req.requester, req.requested))
                {
                    var msg = new DL_FRIEND_REQUEST_RES()
                    {
                        error = MessageError.FAIL,
                        requester = req.requester,
                        requested = req.requested,
                    };

                    if (reader.Read())
                    {
                        var res = reader.GetInt64(0);
                        switch (res)
                        {
                            case -1:
                                msg.error = MessageError.INVALID_REQUESTER_ID;
                                break;
                            case -2:
                                msg.error = MessageError.INVALID_REQUESTED_ID;
                                break;
                            case -3:
                                msg.error = MessageError.ALREADY_EXISTS_REQUESTER;
                                break;
                            case -4:
                                msg.error = MessageError.ALREADY_EXISTS_REQUESTED;
                                break;
                            case -5:
                                msg.error = MessageError.ALREADY_FRIENDS;
                                break;
                            default:
                                if (res > 0)
                                {
                                    msg.error = MessageError.NONE;
                                    msg.friendIndex = (ulong)res;
                                    msg.friendLogin = reader.GetString(1);
                                }
                                break;
                        }
                    }

                    session.Send(msg);
                }
            }
        }

        internal static void RegisterReq(DL_REGISTER_REQ req, ClientSession session)
        {
            using (var conn = new DBConnection("LobbyServer"))
            {
                using (var reader = conn.Query("SELECT COUNT(*) FROM member WHERE login = @p1;", req.login))
                {
                    if (reader.Read())
                    {
                        int result = Convert.ToInt32(reader[0].ToString());
                        if (result >= 1)
                        {
                            session.Send(new DL_REGISTER_RES()
                            {
                                sessionId = req.sessionId,
                                error = MessageError.REGISTER_LOGIN_IN_USE
                            });

                            return;
                        }
                    }
                    else
                    {
                        session.Send(new DL_REGISTER_RES()
                        {
                            sessionId = req.sessionId,
                            error = MessageError.REGISTER_FAIL
                        });

                        return;
                    }
                }

                using (var reader = conn.Query("SELECT COUNT(*) FROM member WHERE email = @p1;", req.email))
                {
                    if (reader.Read())
                    {
                        int result = Convert.ToInt32(reader[0].ToString());
                        if (result == 1)
                        {
                            session.Send(new DL_REGISTER_RES()
                            {
                                sessionId = req.sessionId,
                                error = MessageError.REGISTER_EMAIL_IN_USE
                            });

                            return;
                        }
                    }
                    else
                    {
                        session.Send(new DL_REGISTER_RES()
                        {
                            sessionId = req.sessionId,
                            error = MessageError.REGISTER_FAIL
                        });

                        return;
                    }
                }

                // Insert new user. 
                using (var reader = conn.Query("INSERT INTO member (login, pass, email) VALUES(@p1, @p2, @p3);", req.login, req.pass, req.email))
                {
                    if (reader == null)
                    {
                        session.Send(new DL_REGISTER_RES()
                        {
                            sessionId = req.sessionId,
                            error = MessageError.REGISTER_FAIL
                        });
                    }
                    else
                    {
                        session.Send(new DL_REGISTER_RES()
                        {
                            sessionId = req.sessionId,
                            error = MessageError.NONE
                        });
                    }
                }
            }
        }

        internal static void TokenPlayerReq(DX_TOKEN_PLAYER_REQ req, ClientSession session)
        {
            using (var conn = new DBConnection("LobbyServer"))
            {
                //Due to GetBytes only accept column index, always keep data column at first.
                using (var reader = conn.Query("CALL fn_token_auth(@p1)", req.token))
                {
                    if (!reader.Read())
                    {
                        session.Send(new DX_TOKEN_PLAYER_RES()
                        {
                            id = req.id,
                            error = MessageError.AUTH_FAIL,
                        });
                    }
                    else
                    {
                        bool first = reader.GetBoolean("first_login");
                        string name = (!first) ? reader.GetString("nick") : string.Empty;

                        session.Send(new DX_TOKEN_PLAYER_RES()
                        {
                            error = MessageError.NONE,
                            token = req.token,
                            login = reader.GetString("login"),
                            info = new PLAYER_INFO()
                            {
                                index = reader.GetUInt64("id"),
                                nick = name,
                                gender = (PlayerGender)reader.GetByte("sex"),
                                level = reader.GetUInt32("level"),
                                experience = reader.GetUInt64("experience"),
                                firstLogin = first,
                                privilege = reader.GetByte("privilege")
                            }
                        });
                    }
                }
            }
        }

        internal static void AuthPlayerReq(DL_AUTH_PLAYER_REQ req, ClientSession session)
        {
            using (var conn = new DBConnection("LobbyServer"))
            {
                //Due to GetBytes only accept column index, always keep data column at first.
                using (var reader = conn.Query("CALL fn_auth(@p1, @p2, @p3)", req.login, req.pass, req.token))
                {
                    if (!reader.Read())
                    {
                        session.Send(new DL_AUTH_PLAYER_RES()
                        {
                            error = MessageError.AUTH_FAIL,
                            login = req.login,
                        });
                    }
                    else
                    {
                        bool first = reader.GetBoolean("first_login");
                        string name = reader.IsDBNull(1) ? req.login : reader.GetString("nick");

                        session.Send(new DL_AUTH_PLAYER_RES()
                        {
                            error = MessageError.NONE,
                            login = req.login,

                            info = new PLAYER_INFO()
                            {
                                index = reader.GetUInt64("id"),
                                nick = name,
                                gender = (PlayerGender)reader.GetByte("sex"),
                                level = reader.GetUInt32("level"),
                                experience = reader.GetUInt64("experience"),
                                firstLogin = first,
                                privilege = reader.GetByte("privilege")
                            }
                        });
                    }
                }
            }
        }

        internal static void PlayerCreateReq(DL_PLAYER_CREATE_REQ req, ClientSession session)
        {
            using (var conn = new DBConnection("LobbyServer"))
            {
                // Check if nick is already in use.
                using (var reader = conn.Query("SELECT COUNT(*) FROM member WHERE nick = @p1;", req.nick))
                {
                    if (reader.Read())
                    {
                        int result = Convert.ToInt32(reader[0].ToString());
                        if (result >= 1)
                        {
                            session.Send(new DL_PLAYER_CREATE_RES()
                            {
                                error = MessageError.ALREADY_IN_USE_NAME
                            });

                            return;
                        }
                    }
                    else
                    {
                        session.Send(new DL_PLAYER_CREATE_RES()
                        {
                            error = MessageError.CREATE_FAIL
                        });

                        return;
                    }
                }

                // -- //
                using (var reader = conn.Query("UPDATE member SET nick = @p1, sex = @p2, first_login = 0 WHERE id = @p3;", req.nick, req.gender, req.index))
                {
                    if (reader == null)
                    {
                        session.Send(new DL_PLAYER_CREATE_RES()
                        {
                            error = MessageError.CREATE_FAIL
                        });
                    }
                    else
                    {
                        session.Send(new DL_PLAYER_CREATE_RES()
                        {
                            error = MessageError.NONE,

                            sessionId = req.sessionId,
                            nick = req.nick,
                            gender = req.gender,
                            index = req.index
                        });
                    }
                }
            }
        }
    }
}
