using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CommonLib.Messaging;
using CommonLib.Messaging.Client;
using CommonLib.Messaging.DB;
using CommonLib.Util;
using LobbyServer.Logic;

namespace LobbyServer.Server.Messaging
{
    abstract class DatabaseMessages
    {
        internal static void TokenPlayerRes(DX_TOKEN_PLAYER_RES res, DatabaseClient dbClient)
        {
            var session = dbClient.App.FindSessionByToken(res.token);
            if (session == null)
            {
                CLog.W("Session not found for login {0}", res.login);

                // Don't found session by token, then try to get session by ID.
                session = dbClient.App.FindSession(res.id);
                if (session == null)
                {
                    CLog.F("Session not found for login {0}, id {1}, token {2}", res.login, res.id, res.token);
                    return;
                }
            }

            session.Send(new CX_TOKEN_RES()
            {
                error = res.error,
                firstLogin = (res.error == MessageError.NONE) ? res.info.firstLogin : false
            });

            if (res.error == MessageError.NONE)
            {
                session.Login = res.login;

                //Try to find a player that already exists on the given login.
                var player = dbClient.App.PlayerController.Find(res.info.index);

                if (player == null && res.info.firstLogin)
                {
                    dbClient.App.PlayerController.Create(res.info.index, string.Empty, PlayerGender.None, res.info.level, res.info.experience, session, PlayerStage.Creating);
                }
                else if (player == null)
                {
                    dbClient.App.PlayerController.Create(res.info.index, res.info.nick, res.info.gender, res.info.level, res.info.experience, session, PlayerStage.Lobby);
                }
                else
                {
                    Debug.Assert(player.Session == null);
                    CLog.D("Player {0} already as an object, reconnecting session...", player.Nick);
                    player.Reconnect(session);
                }
            }
            else
            {
                session.Login = null;
                session.DeviceID = null;

                session.Token = null;
            }
        }

        internal static void PlayerCreateRes(DL_PLAYER_CREATE_RES res, DatabaseClient dbClient)
        {
            var session = dbClient.App.FindSession(res.sessionId);
            if (session == null)
            {
                CLog.D("Session not found for login {0}", res.sessionId);
                return;
            }

            if (res.error == MessageError.NONE)
            {
                var player = dbClient.App.PlayerController.Find(res.index);
                if (player == null)
                {
                    // TODO : !!!!!!!!!
                    return;
                }
                else
                {
                    player.Nick = res.nick;
                    player.Gender = res.gender;

                    player.Stage = PlayerStage.Lobby;

                    player.SendInitialData();
                    player.InfoSendEnd();
                }

            }

            session.Send(new CL_PLAYER_CREATE_RES()
            {
                error = res.error,
            });
        }

        internal static void FriendRemoveRes(DL_FRIEND_REMOVE_RES res, DatabaseClient dbClient)
        {
            var player = dbClient.App.PlayerController.Find(res.nick);

            if (player == null)
            {
                return;
            }

            var error = MessageError.FAIL;

            if (res.error == MessageError.NONE)
            {
                dbClient.App.PlayerController.RemoveFriend(player, res.friend);
                error = MessageError.NONE;
            }
            else if (res.error == MessageError.INVALID_REQUESTED_ID)
            {
                error = MessageError.NOT_FOUND;
            }
            else
            {
                CLog.W("General failure when player {0} tried to remove {1} as friend.", res.nick, res.friend);
                error = MessageError.FAIL;
            }

            player.Session.Send(new CL_FRIEND_REMOVE_RES()
            {
                error = error,
                nick = res.friend,
            });
        }

        internal static void FriendResposeRes(DL_FRIEND_RESPONSE_RES res, DatabaseClient dbClient)
        {
            var player = dbClient.App.PlayerController.Find(res.requested);

            if (player == null)
            {
                return;
            }

            var error = MessageError.FAIL;

            if (res.error == MessageError.NONE)
            {
                var friend = player.Friends.Find(f => f.nick == res.requester);

                if (friend?.state == FriendState.WaitingApproval)
                {
                    error = MessageError.NONE;

                    if (res.accept)
                    {
                        dbClient.App.PlayerController.AcceptFriend(player, res.requester);
                    }
                    else
                    {
                        dbClient.App.PlayerController.RemoveFriend(player, res.requester);
                    }
                }
            }
            else if (res.error == MessageError.INVALID_REQUESTED_ID)
            {
                error = MessageError.NOT_FOUND;
            }
            else if (res.error == MessageError.ALREADY_FRIENDS)
            {
                error = MessageError.ALREADY_FRIENDS;
            }
            else
            {
                CLog.W("General failure when player {0} tried to accept {1} as friend.", res.requested, res.requester);
                error = MessageError.FAIL;
            }

            player.Session.Send(new CL_FRIEND_RESPONSE_RES()
            {
                error = error,
                nick = res.requester,
            });
        }

        internal static void FriendRequestRes(DL_FRIEND_REQUEST_RES res, DatabaseClient dbClient)
        {
            var player = dbClient.App.PlayerController.Find(res.requester);

            if (player == null)
            {
                return;
            }

            var error = MessageError.FAIL;

            if (res.error == MessageError.NONE)
            {
                dbClient.App.PlayerController.AddFriend(player, res.friendIndex, res.friendLogin, res.requested);
                error = MessageError.NONE;
            }
            else if (res.error == MessageError.INVALID_REQUESTED_ID)
            {
                error = MessageError.NOT_FOUND;
            }
            else if (res.error == MessageError.ALREADY_FRIENDS)
            {
                error = MessageError.ALREADY_FRIENDS;
            }
            else if (res.error == MessageError.ALREADY_EXISTS_REQUESTER)
            {
                //Means there is already a request to this same friend.
                error = MessageError.ALREADY_EXISTS_REQUESTER;
            }
            else if (res.error == MessageError.ALREADY_EXISTS_REQUESTED)
            {
                //Means there is a pending request from this friend to us.
                error = MessageError.ALREADY_EXISTS_REQUESTED;
            }
            else
            {
                CLog.W("General failure when player {0} tried to add {1} as friend.", res.requester, res.requested);
                error = MessageError.FAIL;
            }

            if (player.IsOffline)
            {
                int b = 0;
            }

            player.Session.Send(new CL_FRIEND_REQUEST_RES()
            {
                error = error,
                nick = res.requested,
            });
        }

        internal static void PlayerAddInfoRes(DL_PLAYER_ADD_INFO_RES res, DatabaseClient dbClient)
        {
            var player = dbClient.App.PlayerController.Find(res.index);

            if (player != null && !player.IsOffline)
            {
                player.SetFriends(res.friends == null ? new List<Friend>() : res.friends.Select(f => new Friend() { index = f.index, login = f.login, nick = f.nick, state = f.state }).ToList());
            }

            dbClient.App.PlayerController.OnConnect(player);
        }

        internal static void AuthPlayerRes(DL_AUTH_PLAYER_RES res, DatabaseClient dbClient)
        {
            var session = dbClient.App.FindSession(res.login);
            if (session == null)
            {
                CLog.D("Session not found for login {0}", res.login);
                return;
            }

            if (res.error == MessageError.NONE)
            {
                if (res.info.firstLogin)
                {
                    dbClient.App.PlayerController.Create(res.info.index, string.Empty, PlayerGender.None, res.info.level, res.info.experience, session, PlayerStage.Creating);
                }
                else
                {
                    var existing = dbClient.App.PlayerController.Find(res.info.index);

                    if (existing != null)
                    {
                        dbClient.App.PlayerController.Destroy(existing);
                    }

                    dbClient.App.PlayerController.Create(res.info.index, res.info.nick, res.info.gender, res.info.level, res.info.experience, session, PlayerStage.Lobby);
                }
            }
            else
            {
                session.Login = null;
                session.DeviceID = null;

                session.Token = null;
            }

            session.Send(new CL_AUTH_RES()
            {
                error = res.error,
                token = session.Token,
                firstLogin = res.info?.firstLogin ?? false
            });
        }

        internal static void FbAuthRes(DL_FB_AUTH_RES res, DatabaseClient dbClient)
        {
            var session = dbClient.App.FindSession(res.login);
            if (session == null)
            {
                CLog.D("Session not found for login {0}", res.login);
                return;
            }

            if (res.error == MessageError.NONE)
            {
                if (res.info.firstLogin)
                {
                    dbClient.App.PlayerController.Create(res.info.index, string.Empty, PlayerGender.None, res.info.level, res.info.experience, session, PlayerStage.Creating);
                }
                else
                {
                    dbClient.App.PlayerController.Create(res.info.index, res.info.nick, res.info.gender, res.info.level, res.info.experience, session, PlayerStage.Lobby);
                }
            }
            else
            {
                session.Login = null;
                session.DeviceID = null;

                session.Token = null;
            }

            session.Send(new CL_FB_AUTH_RES()
            {
                error = res.error,
                token = session.Token,
                firstLogin = res.info?.firstLogin ?? false
            });
        }

        internal static void RegisterRes(DL_REGISTER_RES res, DatabaseClient dbClient)
        {
            var session = dbClient.App.FindSession(res.sessionId);

            if (session == null)
            {
                CLog.D("Session not found for login {0}", res.sessionId);
                return;
            }

            session.Send(new CL_REGISTER_RES()
            {
                error = res.error
            });
        }

        internal static void ListMapRes(DL_LIST_MAP_RES res, DatabaseClient dbClient)
        {
            foreach (var info in res.mapList)
            {
                dbClient.App.RoomController.AddMap(info.index, info.name, info.width, info.height, info.playerCnt);
            }

            CLog.I("Loaded {0} maps from DB", res.mapList.Count);
        }
    }
}
