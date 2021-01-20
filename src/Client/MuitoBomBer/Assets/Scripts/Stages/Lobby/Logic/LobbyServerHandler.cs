using CommonLib.Messaging.Client;
using CommonLib.Messaging;
using CommonLib.Util;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Stages.Lobby.Logic
{
    class LobbyServerHandler
    {
        internal static void TokenRes(CX_TOKEN_RES res, LobbyServerConnection connection)
        {
            connection.Stage.HideWaiting();

            if (res.error != MessageError.NONE)
            {
                LocalStorage.SetString("TOKEN", null);
                connection.Stage.ShowLoginWindow();
            }
            else
            {
                connection.Stage.AuthSuccess(res.firstLogin);
            }
        }

        internal static void AuthRes(CL_AUTH_RES res, LobbyServerConnection connection)
        {
            connection.Stage.HideWaiting();

            var loginWindow = connection.Stage.UIManager.FindInstance(WindowType.LOGIN) as LoginWindow;
            if (loginWindow == null)
            {
                CLog.W("Failed to find LoginWindow on auth result.");
                return;
            }

            if (res.error != MessageError.NONE)
            {
                loginWindow.AuthFailed(res.error);
            }
            else
            {
                LocalStorage.SetString("TOKEN", res.token);

                // Remove login UI.
                connection.Stage.UIManager.Destroy(WindowType.LOGIN);

                connection.Stage.AuthSuccess(res.firstLogin);
            }
        }

        internal static void FbAuthRes(CL_FB_AUTH_RES res, LobbyServerConnection connection)
        {
            connection.Stage.HideWaiting();

            var loginWindow = connection.Stage.UIManager.FindInstance(WindowType.LOGIN) as LoginWindow;
            if (loginWindow == null)
            {
                CLog.W("Failed to find LoginWindow on auth result.");
                return;
            }

            if (res.error != MessageError.NONE)
            {
                loginWindow.AuthFailed(res.error);
            }
            else
            {
                LocalStorage.SetString("TOKEN", res.token);

                // Remove login UI.
                connection.Stage.UIManager.Destroy(WindowType.LOGIN);

                connection.Stage.AuthSuccess(res.firstLogin);
            }
        }

        internal static void PlayerOnlineNfy(CX_PLAYER_ONLINE_NFY nfy, LobbyServerConnection connection)
        {
            var waitingRoom = connection.Stage.RoomController.WaitingRoom;
            if (waitingRoom == null)
                return;

            waitingRoom.SetPlayerOnline(nfy.index);

            var room = connection.Stage.UIManager.FindInstance(WindowType.ROOM) as RoomWindow;
            if (room == null)
                return;

            room.RefreshPlayers();
        }

        internal static void PlayerHeartBeatReq(CL_PLAYER_HEARTBEAT_REQ req, LobbyServerConnection connection)
        {
            connection.Send(new CL_PLAYER_HEARTBEAT_RES() { });
        }

        internal static void PlayerHeartBeatNfy(CL_PLAYER_HEARTBEAT_NFY nfy, LobbyServerConnection connection)
        {
            var mainPlayer = connection.Stage.MainPlayer;

            if (mainPlayer != null)
            {
                mainPlayer.Ping = nfy.ping;
            }

            var main = connection.Stage.UIManager.FindInstance(WindowType.MAIN) as MainWindow;
            main?.SetPing(mainPlayer.Ping);

            if (mainPlayer?.Stage == PlayerStage.Room)
            {
                var room = connection.Stage.UIManager.FindInstance(WindowType.ROOM) as RoomWindow;
                room?.RefreshPing(mainPlayer.Index, mainPlayer.Ping);
            }
        }

        internal static void PlayerOfflineNfy(CX_PLAYER_OFFLINE_NFY nfy, LobbyServerConnection connection)
        {
            var waitingRoom = connection.Stage.RoomController.WaitingRoom;
            waitingRoom?.SetPlayerOffline(nfy.index);

            var room = connection.Stage.UIManager.FindInstance(WindowType.ROOM) as RoomWindow;
            room?.RefreshPlayers();
        }

        internal static void RoomListNfy(CL_ROOM_LIST_NFY nfy, LobbyServerConnection connection)
        {
            if (nfy.rooms == null || nfy.rooms.Count == 0)
                return;

            foreach (var roomInfo in nfy.rooms)
            {
                var room = connection.Stage.RoomController.AddRoom(roomInfo.index);
                room.Assign(roomInfo);
            }
        }

        internal static void DisconnectedNfy(CX_DISCONNECTED_NFY nfy, LobbyServerConnection connect)
        {
            //TODO: Add better warning and some info about reason.
            connect.Stage.Logout();
        }

        internal static void MainPlayerInfoNfy(CL_MAIN_PLAYER_INFO_NFY nfy, LobbyServerConnection connection)
        {
            CLog.I("Received main player info. Index: {0}, Nick: {1}", nfy.player.index, nfy.player.nick);

            connection.Stage.InitMainPlayer(nfy.player);
        }

        internal static void RoomHeartBeatNfy(CL_ROOM_HEARTBEAT_NFY nfy, LobbyServerConnection connection)
        {
            var mainPlayer = connection.Stage.MainPlayer;
            if (mainPlayer?.Stage != PlayerStage.Room || mainPlayer.RoomIndex != nfy.roomIdx)
                return;

            var room = connection.Stage.UIManager.FindInstance(WindowType.ROOM) as RoomWindow;
            room?.RefreshPing(nfy.playerIdx, nfy.ping);
        }

        internal static void RoomChangeSlotPosRes(CL_ROOM_CHANGE_SLOT_POS_RES res, LobbyServerConnection connection)
        {
            if (res.error == MessageError.NONE)
            {
                // Nothing to do...
            }
            else
            {
                var msgHint = connection.Stage.UIManager.FindInstance(WindowType.MSG_HINT, true) as MessageHint;

                switch (res.error)
                {
                    case MessageError.ROOM_INVALID:
                        msgHint.AddMessageHint("Could not find the room.");
                        break;
                    case MessageError.INVALID_STATE:
                        msgHint.AddMessageHint("Invalid state.");
                        break;
                    case MessageError.NOT_OWNER:
                        msgHint.AddMessageHint("You are not the leader of the room.");
                        break;
                    case MessageError.SLOT_INVALID:
                        msgHint.AddMessageHint("Invalid slot. Try again.");
                        break;
                    default:
                        msgHint.AddMessageHint("Failed to change position in the slot. General error.");
                        break;
                }
            }
        }

        internal static void RoomChangeSlotSinglePosNfy(CL_ROOM_CHANGE_SLOT_SINGLE_POS_NFY nfy, LobbyServerConnection connection)
        {
            var mainPlayer = connection.Stage.MainPlayer;
            if (mainPlayer?.Stage != PlayerStage.Room)
                return;

            var room = connection.Stage.UIManager.FindInstance(WindowType.ROOM) as RoomWindow;
            room?.DragDropSlots(nfy.playerIndex, 0, nfy.oldSlot, nfy.newSlot);
        }

        internal static void RoomChangeSlotPosNfy(CL_ROOM_CHANGE_SLOT_POS_NFY nfy, LobbyServerConnection connection)
        {
            var mainPlayer = connection.Stage.MainPlayer;
            if (mainPlayer?.Stage != PlayerStage.Room)
                return;

            var room = connection.Stage.UIManager.FindInstance(WindowType.ROOM) as RoomWindow;
            room?.DragDropSlots(nfy.playerIndex1, nfy.playerIndex2, nfy.newSlot1, nfy.newSlot2);
        }

        internal static void RoomKickPlayerRes(CL_ROOM_KICK_PLAYER_RES res, LobbyServerConnection connection)
        {
            if (res.error == MessageError.NONE)
            {
                // Nothing to do...
            }
            else
            {
                var msgHint = connection.Stage.UIManager.FindInstance(WindowType.MSG_HINT, true) as MessageHint;
                switch (res.error)
                {
                    case MessageError.INVALID_STATE:
                        msgHint.AddMessageHint("Invalid state.");
                        break;
                    case MessageError.PLAYER_INVALID:
                        msgHint.AddMessageHint("Could not find the player.");
                        break;
                    case MessageError.ROOM_INVALID:
                        msgHint.AddMessageHint("Could not find the room.");
                        break;
                    case MessageError.NOT_OWNER:
                        msgHint.AddMessageHint("You are not the leader of the room.");
                        break;
                    case MessageError.KICK_YOURSELF:
                        msgHint.AddMessageHint("You can't kick yourself.");
                        break;
                    default:
                        msgHint.AddMessageHint("Failed to kick player. General error.");
                        break;
                }
            }
        }

        internal static void FriendOfflineNfy(CL_FRIEND_OFFLINE_NFY nfy, LobbyServerConnection connection)
        {
            connection.Stage.FriendController.UpdateFriendState(nfy.nick, FriendState.Offline);
        }

        internal static void FriendOnlineNfy(CL_FRIEND_ONLINE_NFY nfy, LobbyServerConnection connection)
        {
            connection.Stage.FriendController.UpdateFriendState(nfy.nick, FriendState.Online);
        }

        internal static void FriendRemoveRes(CL_FRIEND_REMOVE_RES res, LobbyServerConnection connection)
        {
            var lobby = connection.Stage;
            lobby.HideWaiting();

            if (res.error == MessageError.NONE)
            {
                lobby.ShowHint(string.Format("{0} was removed from your friend list.", res.nick));
            }
            else if (res.error == MessageError.NOT_FOUND)
            {
                lobby.ShowHint(string.Format("{0} was already removed from your friend list", res.nick));
            }
            else
            {
                lobby.ShowHint(string.Format("Failed to remove {0} from your friend list. Please try again.", res.nick));
            }

            connection.Stage.FriendController.RefreshFriendsUI();
        }

        internal static void FriendResponseRes(CL_FRIEND_RESPONSE_RES res, LobbyServerConnection connection)
        {
            var lobby = connection.Stage;
            lobby.HideWaiting();

            if (res.error == MessageError.NONE)
            {
                lobby.ShowHint(string.Format("{0} was added to your friend list!", res.nick));
            }
            else if (res.error == MessageError.NOT_FOUND)
            {
                lobby.ShowHint(string.Format("Unable to find {0} to accept as your friend.", res.nick));
            }
            else if (res.error == MessageError.ALREADY_FRIENDS)
            {
                lobby.ShowHint("You are already friends!");
            }
            else
            {
                lobby.ShowHint(string.Format("Failed to remove {0} from your friend list. Please try again.", res.nick));
            }

            connection.Stage.FriendController.RefreshFriendsUI();
        }

        internal static void FriendRequestRes(CL_FRIEND_REQUEST_RES res, LobbyServerConnection connection)
        {
            var lobby = connection.Stage;
            lobby.HideWaiting();

            if (res.error == MessageError.NONE)
            {
                lobby.ShowHint(string.Format("Request sent. You must wait for {0} approval.", res.nick));
                lobby.UIManager.Destroy(WindowType.FRIEND_ADD);
            }
            else if (res.error == MessageError.ALREADY_FRIENDS)
            {
                lobby.ShowHint(string.Format("{0} is already on your friend list!", res.nick));
            }
            else if (res.error == MessageError.CANT_SELF)
            {
                lobby.ShowHint("You can't add yourself as friend.");
            }
            else if (res.error == MessageError.NOT_FOUND)
            {
                lobby.ShowHint(string.Format("Unable to find a player with the nick {0}", res.nick));
            }
            else if (res.error == MessageError.ALREADY_EXISTS_REQUESTER)
            {
                lobby.ShowHint(string.Format("{0} is waiting your approval to be your friend", res.nick));
            }
            else if (res.error == MessageError.ALREADY_EXISTS_REQUESTED)
            {
                lobby.ShowHint(string.Format("You've already requested {0} as your friend. You must wait his/her approval", res.nick));
            }
            else
            {
                lobby.ShowHint(string.Format("Failed to add {0} as friend. Please try again.", res.nick));
            }

            connection.Stage.FriendController.RefreshFriendsUI();
        }

        internal static void FriendInfoNfy(CL_FRIEND_INFO_NFY nfy, LobbyServerConnection connection)
        {
            var controller = connection.Stage.FriendController;
            var friends = new List<Friend>();

            if (nfy.friends != null && nfy.friends.Count >= 0)
            {
                foreach (var friend in nfy.friends)
                {
                    friends.Add(new Friend(friend.nick)
                    {
                        State = friend.state,
                    });
                }
            }

            controller.SetFriends(friends);
        }

        internal static void RoomTransferOwnerRes(CL_ROOM_TRANSFER_OWNER_RES res, LobbyServerConnection connection)
        {
            if (res.error == MessageError.NONE)
            {
                // Nothing to do...
            }
            else
            {
                var msgHint = connection.Stage.UIManager.FindInstance(WindowType.MSG_HINT, true) as MessageHint;
                switch (res.error)
                {
                    case MessageError.INVALID_STATE:
                        msgHint.AddMessageHint("Invalid state.");
                        break;
                    case MessageError.PLAYER_INVALID:
                        msgHint.AddMessageHint("Could not find the player.");
                        break;
                    case MessageError.ROOM_INVALID:
                        msgHint.AddMessageHint("Could not find the room.");
                        break;
                    case MessageError.NOT_OWNER:
                        msgHint.AddMessageHint("You are not the leader of the room.");
                        break;
                    case MessageError.TRANSFER_YOURSELF:
                        msgHint.AddMessageHint("You are already owner of the room.");
                        break;
                    default:
                        msgHint.AddMessageHint("Failed to transfer owner for the player. General error.");
                        break;
                }
            }
        }

        internal static void RoomKickPlayerNfy(CL_ROOM_KICK_PLAYER_NFY nfy, LobbyServerConnection connection)
        {
            var mainPlayer = connection.Stage.MainPlayer;

            if (mainPlayer.Index == nfy.playerIndex)
            {
                var msgHint = connection.Stage.UIManager.FindInstance(WindowType.MSG_HINT, true) as MessageHint;
                msgHint?.AddMessageHint("You were kicked out of the room.");

                var controller = connection.Stage.RoomController;
                var waitingRoom = controller.WaitingRoom;

                if (waitingRoom.Info != null && waitingRoom.Info.Index == nfy.roomIndex)
                {
                    controller.LeaveWaitingRoom(true);
                }

                mainPlayer.RoomIndex = 0;
            }
            else
            {
                var waitingRoom = connection.Stage.RoomController.WaitingRoom;
                if (waitingRoom != null)
                {
                    waitingRoom.PlayerLeft(nfy.playerIndex);
                }

                var uiRoom = connection.Stage.UIManager.FindInstance(WindowType.ROOM) as RoomWindow;
                if (uiRoom != null)
                {
                    uiRoom.RefreshPlayers();
                }
            }
        }

        internal static void InfoEndNfy(CL_INFO_END_NFY nfy, LobbyServerConnection connection)
        {
            var mainPlayer = connection.Stage.MainPlayer;

            if (mainPlayer.Stage == PlayerStage.Room)
            {
                var controller = connection.Stage.RoomController;
                var room = controller.FindRoom(mainPlayer.RoomIndex);

                if (room != null)
                {
                    controller.EnterWaitingRoom(room);
                    return;
                }
            }

            var mainWindow = connection.Stage.UIManager.FindInstance(WindowType.MAIN) as MainWindow;
            if (mainWindow)
            {
                mainWindow.SetInfoPlayer(mainPlayer.Nick, mainPlayer.Level);
            }
            else
            {
                CLog.W("Failed to find Main Window.");
            }
        }

        internal static void PlayerReadyNfy(CL_PLAYER_READY_NFY nfy, LobbyServerConnection connection)
        {
            var waitingRoom = connection.Stage.RoomController.WaitingRoom;
            if (waitingRoom == null)
                return;

            waitingRoom.SetPlayerReady(nfy.index, nfy.ready);

            var room = connection.Stage.UIManager.FindInstance(WindowType.ROOM) as RoomWindow;
            if (room == null)
                return;

            room.RefreshPlayers();
            room.RefreshButtons();
        }

        internal static void RoomSettingRes(CL_ROOM_SETTING_RES res, LobbyServerConnection connection)
        {
            if (res.error == MessageError.NONE)
            {
                connection.Stage.UIManager.Destroy(WindowType.ROOM_OPTION);
            }
            else
            {
                var msgHint = connection.Stage.UIManager.FindInstance(WindowType.MSG_HINT, true) as MessageHint;
                switch (res.error)
                {
                    case MessageError.INVALID_STATE:
                        msgHint.AddMessageHint("Invalid state.");
                        break;
                    case MessageError.NOT_FOUND:
                        msgHint.AddMessageHint("Could not find the room.");
                        break;
                    default:
                        msgHint.AddMessageHint("Failed to update room. General error.");
                        break;
                }
            }
        }

        internal static void PlayerCreateRes(CL_PLAYER_CREATE_RES res, LobbyServerConnection connection)
        {
            if (res.error == MessageError.NONE)
            {
                // Let to destroy Player Creation Window.
                connection.Stage.UIManager.Destroy(WindowType.PLAYER_CREATION);

                // Now init Main Window.
                connection.Stage.UIManager.Instanciate(WindowType.MAIN);
            }
            else
            {
                var msgHint = connection.Stage.UIManager.FindInstance(WindowType.MSG_HINT, true) as MessageHint;
                switch (res.error)
                {
                    case MessageError.INVALID_NAME:
                        msgHint.AddMessageHint("Invalid name.");
                        break;
                    case MessageError.ALREADY_IN_USE_NAME:
                        msgHint.AddMessageHint("Name already in use.");
                        break;
                    default:
                        msgHint.AddMessageHint("Failed to create player. General error.");
                        break;
                }
            }
        }

        internal static void PlayerReadyRes(CL_PLAYER_READY_RES res, LobbyServerConnection connection)
        {
            //TODO do something with this res, maybe some messages.
        }

        internal static void LobbyListRes(CL_PLAYER_LOBBY_LIST_RES req, LobbyServerConnection connection)
        {
            var roomWindow = connection.Stage.UIManager.FindInstance(WindowType.LOBBY) as LobbyWindow;
            if (roomWindow != null)
            {
                roomWindow.ClearListPlayer();

                foreach (var p in req.players)
                {
                    roomWindow.AddPlayerList(p.index, p.nick, p.level, p.gender);
                }
            }
        }

        internal static void PlayerStageNfy(CL_PLAYER_STAGE_NFY nfy, LobbyServerConnection connection)
        {
            // Update current stage.
            var player = connection.Stage.MainPlayer;

            if (player == null)
                return;

            if (nfy.player.stage == PlayerStage.Lobby && nfy.player.index == player.Index)
                connection.Stage.RequestLobbyListPlayers();

            var roomWindow = connection.Stage.UIManager.FindInstance(WindowType.LOBBY) as LobbyWindow;
            if (roomWindow != null)
            {
                if (nfy.player.stage == PlayerStage.Lobby)
                    roomWindow.AddPlayerList(nfy.player.index, nfy.player.nick, nfy.player.level, nfy.player.gender);
                else
                    roomWindow.RemovePlayerList(nfy.player.index, nfy.player.nick);
            }
        }

        internal static void RoomStartRes(CL_ROOM_START_RES res, LobbyServerConnection connection)
        {
            if (res.error != MessageError.NONE)
            {
                var msgHint = connection.Stage.UIManager.FindInstance(WindowType.MSG_HINT, true) as MessageHint;
                switch (res.error)
                {
                    case MessageError.NOT_FOUND:
                        msgHint.AddMessageHint("Failed to start game.");
                        break;
                    case MessageError.NOT_ENOUGH:
                        msgHint.AddMessageHint("You must have at least 2 players to start the game.");
                        break;
                    case MessageError.NOT_OWNER:
                        msgHint.AddMessageHint("You are not the owner of the room to start the game.");
                        break;
                    default:
                        msgHint.AddMessageHint("Failed to start game. General failure.");
                        break;
                }
            }
        }

        internal static void RoomStartNfy(CL_ROOM_START_NFY nfy, LobbyServerConnection connection)
        {
            StageManager.ChangeStage(StageType.Room, nfy.roomIndex, nfy.serverInfo.address, nfy.serverInfo.port);
        }

        internal static void PlayerJoinedNfy(CL_PLAYER_JOINED_NFY nfy, LobbyServerConnection connection)
        {
            var waitingRoom = connection.Stage.RoomController.WaitingRoom;
            if (waitingRoom != null)
            {
                var mainPlayer = connection.Stage.MainPlayer;

                if (nfy.player.index == mainPlayer.Index)
                {
                    waitingRoom.PlayerJoined(nfy.player.roomSlotIndex, mainPlayer);
                }
                else
                {
                    waitingRoom.PlayerJoined(nfy.player.roomSlotIndex, new Player(nfy.player.index, nfy.player.nick, nfy.player.gender, nfy.player.level, nfy.player.experience)
                    {
                        Ready = nfy.player.state == PlayerState.Ready,
                        Offline = nfy.player.state == PlayerState.Offline,
                    });
                }
            }

            var room = connection.Stage.UIManager.FindInstance(WindowType.ROOM) as RoomWindow;
            if (room != null)
            {
                room.RefreshPlayers();
            }
        }

        internal static void PlayerLeftNfy(CL_PLAYER_LEFT_NFY nfy, LobbyServerConnection connection)
        {
            var waitingRoom = connection.Stage.RoomController.WaitingRoom;
            if (waitingRoom != null)
            {
                waitingRoom.PlayerLeft(nfy.playerIndex);
            }

            var uiRoom = connection.Stage.UIManager.FindInstance(WindowType.ROOM) as RoomWindow;
            if (uiRoom != null)
            {
                uiRoom.RefreshPlayers();
            }
        }

        internal static void RoomJoinRes(CL_ROOM_JOIN_RES res, LobbyServerConnection connection)
        {
            if (res.error != MessageError.NONE)
            {
                var msgHint = connection.Stage.UIManager.FindInstance(WindowType.MSG_HINT, true) as MessageHint;
                switch (res.error)
                {
                    case MessageError.NOT_FOUND:
                        msgHint.AddMessageHint("Failed to join into the room. Room not found.");
                        break;
                    case MessageError.FULL:
                        msgHint.AddMessageHint("Failed to join into the room. Room is already full.");
                        break;
                    case MessageError.JOIN_WRONG_PASSWORD:
                        msgHint.AddMessageHint("Wrong password.");
                        break;
                    default:
                        msgHint.AddMessageHint("Failed to join into the room. Geral failure.");
                        break;
                }
            }
            else
            {
                var controller = connection.Stage.RoomController;
                var room = controller.FindRoom(res.info.index);

                if (room == null)
                {
                    room = controller.AddRoom(res.info.index);
                }

                room.Assign(res.info);

                connection.Stage.MainPlayer.RoomIndex = room.Index;
                connection.Stage.MainPlayer.Stage = PlayerStage.Room;

                if (room.IsPublic || connection.Stage.UIManager.FindInstance(WindowType.ROOM_PASSWORD))
                    connection.Stage.UIManager.Destroy(WindowType.ROOM_PASSWORD);

                connection.Stage.RoomController.EnterWaitingRoom(room);
            }
        }

        internal static void RoomLeaveRes(CL_ROOM_LEAVE_RES res, LobbyServerConnection connection)
        {
            if (res.error == MessageError.NONE)
            {
                // Update current room index & stage of Player.
                // As the player left the room, then the room index will be 0 and stage will be back to Lobby.
                connection.Stage.MainPlayer.RoomIndex = 0;
                connection.Stage.MainPlayer.Stage = PlayerStage.Lobby;
            }
        }

        internal static void RoomDestroyedNfy(CL_ROOM_DESTROYED_NFY nfy, LobbyServerConnection connection)
        {
            var controller = connection.Stage.RoomController;
            controller.RemoveRoom(nfy.index);

            var waitingRoom = controller.WaitingRoom;

            if (waitingRoom.Info != null && waitingRoom.Info.Index == nfy.index)
            {
                controller.LeaveWaitingRoom();
            }

            var roomWindow = connection.Stage.UIManager.FindInstance(WindowType.LOBBY) as LobbyWindow;
            if (roomWindow != null)
                roomWindow.RemoveRoom(nfy.index);
        }

        internal static void RoomCreatedNfy(CL_ROOM_CREATED_NFY nfy, LobbyServerConnection connection)
        {
            var room = connection.Stage.RoomController.AddRoom(nfy.info.index);
            room.Assign(nfy.info);

            var roomWindow = connection.Stage.UIManager.FindInstance(WindowType.LOBBY) as LobbyWindow;
            if (roomWindow != null)
                roomWindow.AddRoom(room);
        }

        internal static void RoomUpdatedNfy(CL_ROOM_UPDATED_NFY nfy, LobbyServerConnection connection)
        {
            var controller = connection.Stage.RoomController;
            var room = controller.FindRoom(nfy.info.index);
            if (room != null)
            {
                room.Assign(nfy.info);

                var roomWindow = connection.Stage.UIManager.FindInstance(WindowType.LOBBY) as LobbyWindow;
                if (roomWindow != null)
                {
                    roomWindow.UpdateRoom(room);
                }
                else
                {
                    var inRoomWindow = connection.Stage.UIManager.FindInstance(WindowType.ROOM) as RoomWindow;
                    if (inRoomWindow != null)
                    {
                        inRoomWindow.RefreshAll();
                    }
                }
            }
        }

        internal static void RoomCreateRes(CL_ROOM_CREATE_RES res, LobbyServerConnection connection)
        {
            if (res.error != MessageError.NONE)
            {
                var msgHint = connection.Stage.UIManager.FindInstance(WindowType.MSG_HINT, true) as MessageHint;
                msgHint.AddMessageHint("Failed to create room. Please try again.");
            }
            else
            {
                var controller = connection.Stage.RoomController;
                var room = controller.FindRoom(res.info.index);

                if (room == null)
                {
                    room = controller.AddRoom(res.info.index);
                }

                room.Assign(res.info);

                connection.Stage.MainPlayer.RoomIndex = room.Index;
                connection.Stage.MainPlayer.Stage = PlayerStage.Room;

                controller.EnterWaitingRoom(room);
            }
        }

        internal static void RegisterRes(CL_REGISTER_RES res, LobbyServerConnection connection)
        {
            var loginWindow = connection.Stage.UIManager.FindInstance(WindowType.LOGIN, true) as LoginWindow;
            var msgHint = connection.Stage.UIManager.FindInstance(WindowType.MSG_HINT, true) as MessageHint;

            switch (res.error)
            {
                case MessageError.NONE:
                    {
                        msgHint.AddMessageHint("Registed with successfuly.");

                        loginWindow._loginInput.value = loginWindow._regLoginInput.value;
                        loginWindow._passInput.value = loginWindow._regPasswordInput.value;

                        loginWindow.OnClickedBackBtn();

                        loginWindow._backBtn.isEnabled = true;
                        loginWindow._toRegisterBtn.isEnabled = true;

                        loginWindow.OnClickedLoginBtn();
                    }
                    break;
                case MessageError.REGISTER_LOGIN_IN_USE:
                    {
                        msgHint.AddMessageHint("This login is already in use.");

                        loginWindow._regLoginInput.value = "";
                        if (!loginWindow._regLoginInput.isSelected)
                            loginWindow._regLoginInput.isSelected = true;

                        loginWindow._backBtn.isEnabled = true;
                        loginWindow._toRegisterBtn.isEnabled = true;
                    }
                    break;
                case MessageError.REGISTER_EMAIL_IN_USE:
                    {
                        msgHint.AddMessageHint("This email is already in use.");

                        loginWindow._regEmailInput.value = "";
                        if (!loginWindow._regEmailInput.isSelected)
                            loginWindow._regEmailInput.isSelected = true;

                        loginWindow._backBtn.isEnabled = true;
                        loginWindow._toRegisterBtn.isEnabled = true;
                    }
                    break;
                default:
                    {
                        msgHint.AddMessageHint("Failed to register.");

                        loginWindow._backBtn.isEnabled = true;
                        loginWindow._toRegisterBtn.isEnabled = true;
                    }
                    break;
            }
        }

        internal static void ChatNormalNfy(CL_CHAT_NORMAL_NFY nfy, LobbyServerConnection connection)
        {
            connection.Stage.ChatController.AddMessage(ChatType.NORMAL, nfy.uid, nfy.name, nfy.msg, connection.Stage.MainPlayer.Index == nfy.uid);
        }

        internal static void ChatWhisperNfy(CL_CHAT_WHISPER_NFY nfy, LobbyServerConnection connection)
        {
            // Add new message on chat window.
            //chatWindow.AddWhisperText(nfy.toName, nfy.fromName, nfy.msg);
        }
    }
}
