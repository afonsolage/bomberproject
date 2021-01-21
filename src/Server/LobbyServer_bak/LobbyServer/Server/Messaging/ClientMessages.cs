using CommonLib.Messaging.Client;
using CommonLib.Messaging;
using CommonLib.Util;
using CommonLib.Messaging.DB;
using CommonLib.Messaging.Common;
using System;
using CommonLib.Messaging.Lobby;
using System.Linq;
using System.Text;
using LobbyServer.Logic;
using System.Collections.Generic;
using System.Diagnostics;
using LobbyServer.Logic.OAuth;

namespace LobbyServer.Server.Messaging
{
    internal abstract class ClientMessages
    {
        public static byte[] UTF8 { get; private set; }

        internal static void Auth(CL_AUTH_REQ req, ClientSession session)
        {
            if (session.App.FindSession(req.login) != null)
            {
                session.Send(new CL_AUTH_RES()
                {
                    error = MessageError.ALREADY_CONNECTED,
                });
                return;
            }

            session.Login = req.login;
            session.DeviceID = req.deviceID;

            //TODO: Add password on config later on
            var tokenRaw = AesHelper.AesEncryptBytes(Encoding.UTF8.GetBytes(session.DeviceID + new Guid().ToString()), "s0M3p4ss", new string(session.Login.PadLeft(15, '@').Reverse().ToArray()));
            session.Token = StringUtils.ByteArrayToHexString(tokenRaw);

            session.App.DBClient.Send(new DL_AUTH_PLAYER_REQ()
            {
                login = req.login,
                pass = req.pass,
                token = session.Token,
            });
        }

        internal static void Token(CX_TOKEN_REQ req, ClientSession session)
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
                id = session.ID,
                token = session.Token,
            });
        }

        internal static void RegisterReq(CL_REGISTER_REQ req, ClientSession session)
        {
            if (string.IsNullOrEmpty(req.password) || string.IsNullOrEmpty(req.login) || string.IsNullOrEmpty(req.email))
                return;

            session.App.DBClient.Send(new DL_REGISTER_REQ()
            {
                sessionId = session.ID,
                login = req.login,
                pass = req.password,
                email = req.email
            });
        }

        internal static void LogOut(CL_LOGOUT_REQ req, ClientSession session)
        {
            session.Stop();
        }

        internal static void FbAuthReq(CL_FB_AUTH_REQ req, ClientSession session)
        {
            var res = FacebookLogin.CheckLoginToken(req.token, (success) =>
                {
                    if (!success)
                    {
                        session.Send(new CL_AUTH_RES()
                        {
                            error = MessageError.AUTH_FAIL,
                        });
                    }

                    if (session.App.FindSession(req.id) != null)
                    {
                        session.Send(new CL_AUTH_RES()
                        {
                            error = MessageError.ALREADY_CONNECTED,
                        });
                        return;
                    }

                    session.Login = req.id;
                    session.DeviceID = req.deviceID;

                    //TODO: Add password on config later on
                    var tokenRaw = AesHelper.AesEncryptBytes(Encoding.UTF8.GetBytes(session.DeviceID + new Guid().ToString()), "s0M3p4ss", new string(session.Login.PadLeft(15, '@').Reverse().ToArray()));
                    session.Token = StringUtils.ByteArrayToHexString(tokenRaw);

                    session.App.DBClient.Send(new DL_FB_AUTH_REQ()
                    {
                        id = req.id,
                        name = req.name,
                        email = req.email,
                        token = session.Token,
                    });
                });

            if (!res)
            {
                //This means we were unable to send a request to facebook due to some error.
                session.Send(new CL_AUTH_RES()
                {
                    error = MessageError.FAIL,
                });
            }
        }

        internal static void ChatNormalReq(CL_CHAT_NORMAL_REQ req, ClientSession session)
        {
            if (string.IsNullOrEmpty(req.msg))
                return;

            session.App.PlayerController.Broadcast(new CL_CHAT_NORMAL_NFY()
            {
                uid = session.Player.Index,
                name = session.Player.Nick,
                msg = req.msg
            });
        }

        internal static void PlayerHeartBeatRes(CL_PLAYER_HEARTBEAT_RES res, ClientSession session)
        {
            var diffTicks = DateTime.UtcNow.Ticks - session.Player.CurrentPingTicks;
            session.Player.Ping = diffTicks / TimeSpan.TicksPerMillisecond;

            session.Send(new CL_PLAYER_HEARTBEAT_NFY() { ping = session.Player.Ping });

            // If the player is in a room, let to inform for anothers players in room your current ping.
            if (session.Player.Stage == PlayerStage.Room && session.Player.Room?.Index != 0)
            {
                var player = session.Player;
                session.Player.Room.PingNfy(player.Index, player.Ping);
            }
        }

        internal static void PlayerCreateReq(CL_PLAYER_CREATE_REQ req, ClientSession session)
        {
            if (session.Player.Stage != PlayerStage.Creating)
            {
                session.Send(new CL_PLAYER_CREATE_RES()
                {
                    error = MessageError.CREATE_FAIL,
                });
            }
            else if (string.IsNullOrEmpty(req.nick))
            {
                session.Send(new CL_PLAYER_CREATE_RES()
                {
                    error = MessageError.INVALID_NAME,
                });
            }
            else
            {
                session.App.DBClient.Send(new DL_PLAYER_CREATE_REQ()
                {
                    sessionId = session.ID,
                    index = session.Player.Index,
                    nick = req.nick,
                    gender = req.gender
                });
            }
        }

        internal static void RoomChangeSlotPosReq(CL_ROOM_CHANGE_SLOT_POS_REQ req, ClientSession session)
        {
            var room = session.Player.Room;

            if (room == null)
            {
                session.Send(new CL_ROOM_CHANGE_SLOT_POS_RES()
                {
                    error = MessageError.ROOM_INVALID
                });
            }
            else if (session.Player.Stage != PlayerStage.Room)
            {
                session.Send(new CL_ROOM_CHANGE_SLOT_POS_RES()
                {
                    error = MessageError.INVALID_STATE
                });
            }
            else if (!room.IsOwner(session.Player))
            {
                session.Send(new CL_ROOM_CHANGE_SLOT_POS_RES()
                {
                    error = MessageError.NOT_OWNER,
                });
            }
            else if (req.newSlot < 0 || req.newSlot >= room.MAX_NUM_USER_SLOT)
            {
                session.Send(new CL_ROOM_CHANGE_SLOT_POS_RES()
                {
                    error = MessageError.SLOT_INVALID,
                });
            }
            else
            {
                room.ChangeSlotPosition(req.currentSlot, req.newSlot);
            }
        }

        internal static void FriendRemoveReq(CL_FRIEND_REMOVE_REQ req, ClientSession session)
        {
            var friend = session.Player.Friends.Find(f => f.nick == req.nick);
            if (friend == null)
            {
                session.Send(new CL_FRIEND_REMOVE_RES()
                {
                    error = MessageError.NOT_FOUND,
                    nick = req.nick,
                });
            }
            else
            {
                session.App.DBClient.Send(new DL_FRIEND_REMOVE_REQ()
                {
                    nick = session.Player.Nick,
                    friend = req.nick,
                });
            }
        }

        internal static void FriendResponseReq(CL_FRIEND_RESPONSE_REQ req, ClientSession session)
        {
            var friend = session.Player.Friends.Find(f => f.nick == req.nick);
            if (friend == null)
            {
                session.Send(new CL_FRIEND_RESPONSE_RES()
                {
                    error = MessageError.NOT_FOUND,
                    nick = req.nick,
                });
            }
            else if (friend.state == FriendState.Offline || friend.state == FriendState.Online)
            {
                session.Send(new CL_FRIEND_RESPONSE_RES()
                {
                    error = MessageError.ALREADY_FRIENDS,
                    nick = req.nick,
                });
            }
            else
            {
                Debug.Assert(friend.state == FriendState.WaitingApproval, "Can only approve friend request if the friend is waiting our approval");

                session.App.DBClient.Send(new DL_FRIEND_RESPONSE_REQ()
                {
                    requester = req.nick,
                    requested = session.Player.Nick,
                    accept = req.accept,
                });
            }
        }

        internal static void FriendRequestReq(CL_FRIEND_REQUEST_REQ req, ClientSession session)
        {
            if (session.Player.Friends.Find(f => f.nick == req.nick) != null)
            {
                session.Send(new CL_FRIEND_REQUEST_RES()
                {
                    error = MessageError.ALREADY_FRIENDS,
                    nick = req.nick,
                });
            }
            else if (session.Player.Nick == req.nick)
            {
                session.Send(new CL_FRIEND_REQUEST_RES()
                {
                    error = MessageError.CANT_SELF,
                    nick = req.nick,
                });
            }
            else
            {
                //TODO: Add more checks, like max friend count?

                session.App.DBClient.Send(new DL_FRIEND_REQUEST_REQ()
                {
                    requester = session.Player.Nick,
                    requested = req.nick,
                });
            }
        }

        internal static void RoomTransferOwnerReq(CL_ROOM_TRANSFER_OWNER_REQ req, ClientSession session)
        {
            var player = session.App.PlayerController.Find(req.playerIndex);
            var room = session.Player.Room;

            if (player == null)
            {
                session.Send(new CL_ROOM_TRANSFER_OWNER_RES()
                {
                    error = MessageError.PLAYER_INVALID
                });
            }
            else if (session.Player.Room == null || player.Room == null)
            {
                session.Send(new CL_ROOM_TRANSFER_OWNER_RES()
                {
                    error = MessageError.ROOM_INVALID
                });
            }
            else if (room == null)
            {
                session.Send(new CL_ROOM_TRANSFER_OWNER_RES()
                {
                    error = MessageError.ROOM_INVALID
                });
            }
            else if (session.Player.Stage != PlayerStage.Room || player.Stage != PlayerStage.Room)
            {
                session.Send(new CL_ROOM_TRANSFER_OWNER_RES()
                {
                    error = MessageError.INVALID_STATE
                });
            }
            else if (!room.IsOwner(session.Player))
            {
                session.Send(new CL_ROOM_TRANSFER_OWNER_RES()
                {
                    error = MessageError.NOT_OWNER,
                });
            }
            else if (room.IsOwner(player))
            {
                session.Send(new CL_ROOM_TRANSFER_OWNER_RES()
                {
                    error = MessageError.TRANSFER_YOURSELF,
                });
            }
            else
            {
                session.Player.Room.TransferOwnership(player);

                session.Send(new CL_ROOM_TRANSFER_OWNER_RES()
                {
                    error = MessageError.NONE,
                });
            }
        }

        internal static void RoomKickPlayerReq(CL_ROOM_KICK_PLAYER_REQ req, ClientSession session)
        {
            var player = session.App.PlayerController.Find(req.playerIndex);
            var room = session.Player.Room;

            if (player == null)
            {
                session.Send(new CL_ROOM_KICK_PLAYER_RES()
                {
                    error = MessageError.PLAYER_INVALID
                });
            }
            else if (session.Player.Room == null || player.Room == null)
            {
                session.Send(new CL_ROOM_KICK_PLAYER_RES()
                {
                    error = MessageError.ROOM_INVALID
                });
            }
            else if (room == null)
            {
                session.Send(new CL_ROOM_KICK_PLAYER_RES()
                {
                    error = MessageError.ROOM_INVALID
                });
            }
            else if (session.Player.Stage != PlayerStage.Room || player.Stage != PlayerStage.Room)
            {
                session.Send(new CL_ROOM_KICK_PLAYER_RES()
                {
                    error = MessageError.INVALID_STATE
                });
            }
            else if (!room.IsOwner(session.Player))
            {
                session.Send(new CL_ROOM_KICK_PLAYER_RES()
                {
                    error = MessageError.NOT_OWNER,
                });
            }
            else if (room.IsOwner(player))
            {
                session.Send(new CL_ROOM_KICK_PLAYER_RES()
                {
                    error = MessageError.KICK_YOURSELF,
                });
            }
            else
            {
                session.Player.Room.Kick(player);

                session.Send(new CL_ROOM_KICK_PLAYER_RES()
                {
                    error = MessageError.NONE,
                });
            }
        }

        internal static void RoomLeaveReq(CL_ROOM_LEAVE_REQ req, ClientSession session)
        {
            if (session.Player.Stage == PlayerStage.Lobby || session.Player.Room == null)
            {
                session.Send(new CL_ROOM_LEAVE_RES()
                {
                    error = MessageError.NOT_FOUND,
                });
            }
            else if (session.Player.Stage == PlayerStage.Playing)
            {
                session.Send(new CL_ROOM_LEAVE_RES()
                {
                    error = MessageError.ALREADY_PLAYING,
                });
            }
            else
            {
                session.Player.Room.Leave(session.Player);

                session.Send(new CL_ROOM_LEAVE_RES()
                {
                    error = MessageError.NONE,
                });
            }
        }

        internal static void RoomSettingReq(CL_ROOM_SETTING_REQ req, ClientSession session)
        {
            if (session.Player.Stage != PlayerStage.Room)
            {
                session.Send(new CL_ROOM_SETTING_RES() { error = MessageError.INVALID_STATE });
            }
            else
            {
                var room = session.Player.Room;
                if (room != null)
                {
                    // Title updated.
                    room.Title = req.title;
                    room.Password = (!string.IsNullOrEmpty(req.pw)) ? req.pw : string.Empty;

                    session.App.RoomController.RoomUpdated(room);

                    session.Send(new CL_ROOM_SETTING_RES() { error = MessageError.NONE });
                }
                else
                {
                    session.Send(new CL_ROOM_SETTING_RES() { error = MessageError.NOT_FOUND });
                }
            }
        }

        internal static void PlayerReadyReq(CL_PLAYER_READY_REQ req, ClientSession session)
        {
            if (session.Player.Stage == PlayerStage.Playing)
            {
                session.Send(new CL_PLAYER_READY_RES() { error = MessageError.ALREADY_PLAYING });
            }
            else if (session.Player.Ready == req.ready)
            {
                session.Send(new CL_PLAYER_READY_RES() { error = MessageError.ALREADY_READY });
            }
            else if (session.Player.Stage != PlayerStage.Room)
            {
                session.Send(new CL_PLAYER_READY_RES() { error = MessageError.INVALID_STATE });
            }
            else
            {
                session.Player.Ready = req.ready;
                session.Send(new CL_PLAYER_READY_RES() { error = MessageError.NONE });
            }
        }

        internal static void LobbyListReq(CL_PLAYER_LOBBY_LIST_REQ req, ClientSession session)
        {
            if (session.Player.Stage != PlayerStage.Lobby)
                return;

            var allPlayers = session.App.PlayerController.ListAllPlayers();
            if (allPlayers?.Count > 0)
            {
                List<PLAYER_INFO> lstPlayer = new List<PLAYER_INFO>();

                foreach (var p in allPlayers)
                {
                    if (p.Stage != PlayerStage.Lobby)
                        continue;

                    PLAYER_INFO player = new PLAYER_INFO
                    {
                        nick = p.Nick,
                        index = p.Index,
                        stage = p.Stage,
                        level = p.Level,
                        experience = p.Experience,
                        ping = p.Ping,
                        roomIndex = p.Room?.Index ?? 0,
                        roomSlotIndex = p.Room?.FindPlayerSlot(p.Index).SlotIndex ?? -1,
                    };

                    lstPlayer.Add(player);
                }

                session.Send(new CL_PLAYER_LOBBY_LIST_RES() { players = lstPlayer });
            }
        }

        internal static void RoomStartReq(CL_ROOM_START_REQ req, ClientSession session)
        {
            var room = session.App.RoomController.FindRoomByOwner(session.Player);

            if (room == null)
            {
                session.Send(new CL_ROOM_START_RES()
                {
                    error = MessageError.NOT_FOUND,
                });
            }
            else if (room.PlayerCnt < 2) //TODO: Add minimum player info on maps
            {
                session.Send(new CL_ROOM_START_RES()
                {
                    error = MessageError.NOT_ENOUGH,
                });
            }
            else if (!room.AllReady())
            {
                session.Send(new CL_ROOM_START_RES()
                {
                    error = MessageError.NOT_READY,
                });
            }
            else if (!room.IsOwner(session.Player))
            {
                session.Send(new CL_ROOM_START_RES()
                {
                    error = MessageError.NOT_OWNER,
                });
            }
            else
            {
                var roomServer = session.App.RoomServerController.FindBestServer();
#if _DEBUG
                Debug.Assert(roomServer != null);
#endif
                if (roomServer == null || roomServer.Session == null || !roomServer.Session.IsActive)
                {
                    session.Send(new CL_ROOM_START_RES()
                    {
                        error = MessageError.SERVER_OFF,
                    });
                }
                else
                {
                    roomServer.Session.Send(new LR_CREATE_ROOM_REQ()
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
            }
        }

        internal static void RoomJoinReq(CL_ROOM_JOIN_REQ req, ClientSession session)
        {
            var room = session.App.RoomController.FindRoom(req.index);

            if (room == null)
            {
                session.Send(new CL_ROOM_JOIN_RES()
                {
                    error = MessageError.NOT_FOUND,
                });
            }
            else if (session.Player.Stage != PlayerStage.Lobby)
            {
                session.Send(new CL_ROOM_JOIN_RES()
                {
                    error = MessageError.JOIN_FAIL,
                });
            }
            else if (room.IsFull())
            {
                session.Send(new CL_ROOM_JOIN_RES()
                {
                    error = MessageError.FULL,
                });
            }
            else if (!room.CheckPassword(req.password))
            {
                session.Send(new CL_ROOM_JOIN_RES()
                {
                    error = MessageError.JOIN_WRONG_PASSWORD,
                });
            }
            else
            {
                if (!room.Join(session.Player))
                {
                    session.Send(new CL_ROOM_JOIN_RES()
                    {
                        error = MessageError.JOIN_FAIL,
                    });
                }
                else
                {
                    session.Send(new CL_ROOM_JOIN_RES()
                    {
                        error = MessageError.NONE,
                        info = new ROOM_INFO()
                        {
                            index = room.Index,
                            mapId = room.MapId,
                            maxPlayer = room.MaxPlayer,
                            name = room.Title,
                            owner = room.Owner.Nick,
                            playerCnt = room.PlayerCnt,
                            stage = room.Stage,
                            password = room.Password,
                            isPublic = (!string.IsNullOrEmpty(room.Password)) ? true : false,
                        },
                    });
                }
            }
        }

        internal static void ChatWhisperReq(CL_CHAT_WHISPER_REQ req, ClientSession session)
        {
            // Get player.
            var player = session.App.PlayerController.Find(req.toName);
            if (player != null)
            {
                player.Session.Send(new CL_CHAT_WHISPER_NFY()
                {
                    uid = session.Player.Index,
                    toName = session.Player.Nick,
                    fromName = player.Nick,
                    msg = req.msg
                });

                session.Send(new CL_CHAT_WHISPER_NFY()
                {
                    uid = player.Index,
                    toName = session.Player.Nick,
                    fromName = player.Nick,
                    msg = req.msg
                });
            }
        }

        internal static void RoomCreateReq(CL_ROOM_CREATE_REQ req, ClientSession session)
        {
            var room = session.App.RoomController.CreateRoom(session.Player);

            if (room == null)
            {
                session.Send(new CL_ROOM_CREATE_RES()
                {
                    error = MessageError.CREATE_FAIL
                });
            }
            else
            {
                session.Send(new CL_ROOM_CREATE_RES()
                {
                    error = MessageError.NONE,
                    info = new ROOM_INFO()
                    {
                        index = room.Index,
                        name = room.Title,
                        mapId = room.MapId,
                        maxPlayer = room.MaxPlayer,
                        playerCnt = room.PlayerCnt,
                        owner = room.Owner.Nick,
                        stage = room.Stage,
                        password = room.Password,
                        isPublic = (!string.IsNullOrEmpty(room.Password)) ? true : false,
                    }
                });
            }

        }
    }
}
