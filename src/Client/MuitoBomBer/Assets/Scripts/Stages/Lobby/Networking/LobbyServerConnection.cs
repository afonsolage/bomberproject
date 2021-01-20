using Assets.Scripts.Stages.Lobby.Logic;
using CommonLib.Messaging;
using CommonLib.Messaging.Base;
using CommonLib.Messaging.Client;
using CommonLib.Networking;
using CommonLib.Util;

public class LobbyServerConnection : ServerConnection
{
    public LobbyStage Stage
    {
        get { return (LobbyStage)_stage; }
    }

    public LobbyServerConnection(LobbyStage stage, string serverHost, int serverPort) : base(stage, serverHost, serverPort)
    {
    }

    public override void HandleOnMainThread(Packet packet)
    {
        var rawMessage = new RawMessage(packet.buffer);

        switch (rawMessage.MsgType)
        {
            case MessageType.CX_TOKEN_RES:
                LobbyServerHandler.TokenRes(rawMessage.To<CX_TOKEN_RES>(), this);
                break;
            case MessageType.CL_FB_AUTH_RES:
                LobbyServerHandler.FbAuthRes(rawMessage.To<CL_FB_AUTH_RES>(), this);
                break;
            case MessageType.CX_DISCONNECTED_NFY:
                LobbyServerHandler.DisconnectedNfy(rawMessage.To<CX_DISCONNECTED_NFY>(), this);
                break;
            case MessageType.CX_PLAYER_OFFLINE_NFY:
                LobbyServerHandler.PlayerOfflineNfy(rawMessage.To<CX_PLAYER_OFFLINE_NFY>(), this);
                break;
            case MessageType.CX_PLAYER_ONLINE_NFY:
                LobbyServerHandler.PlayerOnlineNfy(rawMessage.To<CX_PLAYER_ONLINE_NFY>(), this);
                break;
            case MessageType.CL_AUTH_RES:
                LobbyServerHandler.AuthRes(rawMessage.To<CL_AUTH_RES>(), this);
                break;
            case MessageType.CL_PLAYER_HEARTBEAT_REQ:
                LobbyServerHandler.PlayerHeartBeatReq(rawMessage.To<CL_PLAYER_HEARTBEAT_REQ>(), this);
                break;
            case MessageType.CL_PLAYER_HEARTBEAT_NFY:
                LobbyServerHandler.PlayerHeartBeatNfy(rawMessage.To<CL_PLAYER_HEARTBEAT_NFY>(), this);
                break;
            case MessageType.CL_ROOM_LIST_NFY:
                LobbyServerHandler.RoomListNfy(rawMessage.To<CL_ROOM_LIST_NFY>(), this);
                break;
            case MessageType.CL_ROOM_CREATE_RES:
                LobbyServerHandler.RoomCreateRes(rawMessage.To<CL_ROOM_CREATE_RES>(), this);
                break;
            case MessageType.CL_ROOM_CREATED_NFY:
                LobbyServerHandler.RoomCreatedNfy(rawMessage.To<CL_ROOM_CREATED_NFY>(), this);
                break;
            case MessageType.CL_ROOM_DESTROYED_NFY:
                LobbyServerHandler.RoomDestroyedNfy(rawMessage.To<CL_ROOM_DESTROYED_NFY>(), this);
                break;
            case MessageType.CL_ROOM_UPDATED_NFY:
                LobbyServerHandler.RoomUpdatedNfy(rawMessage.To<CL_ROOM_UPDATED_NFY>(), this);
                break;
            case MessageType.CL_ROOM_JOIN_RES:
                LobbyServerHandler.RoomJoinRes(rawMessage.To<CL_ROOM_JOIN_RES>(), this);
                break;
            case MessageType.CL_ROOM_LEAVE_RES:
                LobbyServerHandler.RoomLeaveRes(rawMessage.To<CL_ROOM_LEAVE_RES>(), this);
                break;
            case MessageType.CL_ROOM_START_NFY:
                LobbyServerHandler.RoomStartNfy(rawMessage.To<CL_ROOM_START_NFY>(), this);
                break;
            case MessageType.CL_ROOM_START_RES:
                LobbyServerHandler.RoomStartRes(rawMessage.To<CL_ROOM_START_RES>(), this);
                break;
            case MessageType.CL_ROOM_SETTING_RES:
                LobbyServerHandler.RoomSettingRes(rawMessage.To<CL_ROOM_SETTING_RES>(), this);
                break;
            case MessageType.CL_ROOM_KICK_PLAYER_RES:
                LobbyServerHandler.RoomKickPlayerRes(rawMessage.To<CL_ROOM_KICK_PLAYER_RES>(), this);
                break;
            case MessageType.CL_ROOM_KICK_PLAYER_NFY:
                LobbyServerHandler.RoomKickPlayerNfy(rawMessage.To<CL_ROOM_KICK_PLAYER_NFY>(), this);
                break;
            case MessageType.CL_ROOM_TRANSFER_OWNER_RES:
                LobbyServerHandler.RoomTransferOwnerRes(rawMessage.To<CL_ROOM_TRANSFER_OWNER_RES>(), this);
                break;
            case MessageType.CL_ROOM_HEARTBEAT_NFY:
                LobbyServerHandler.RoomHeartBeatNfy(rawMessage.To<CL_ROOM_HEARTBEAT_NFY>(), this);
                break;
            case MessageType.CL_ROOM_CHANGE_SLOT_POS_RES:
                LobbyServerHandler.RoomChangeSlotPosRes(rawMessage.To<CL_ROOM_CHANGE_SLOT_POS_RES>(), this);
                break;
            case MessageType.CL_ROOM_CHANGE_SLOT_POS_NFY:
                LobbyServerHandler.RoomChangeSlotPosNfy(rawMessage.To<CL_ROOM_CHANGE_SLOT_POS_NFY>(), this);
                break;
            case MessageType.CL_ROOM_CHANGE_SLOT_SINGLE_POS_NFY:
                LobbyServerHandler.RoomChangeSlotSinglePosNfy(rawMessage.To<CL_ROOM_CHANGE_SLOT_SINGLE_POS_NFY>(), this);
                break;
            case MessageType.CL_PLAYER_JOINED_NFY:
                LobbyServerHandler.PlayerJoinedNfy(rawMessage.To<CL_PLAYER_JOINED_NFY>(), this);
                break;
            case MessageType.CL_PLAYER_LEFT_NFY:
                LobbyServerHandler.PlayerLeftNfy(rawMessage.To<CL_PLAYER_LEFT_NFY>(), this);
                break;
            case MessageType.CL_PLAYER_READY_RES:
                LobbyServerHandler.PlayerReadyRes(rawMessage.To<CL_PLAYER_READY_RES>(), this);
                break;
            case MessageType.CL_PLAYER_READY_NFY:
                LobbyServerHandler.PlayerReadyNfy(rawMessage.To<CL_PLAYER_READY_NFY>(), this);
                break;
            case MessageType.CL_MAIN_PLAYER_INFO_NFY:
                LobbyServerHandler.MainPlayerInfoNfy(rawMessage.To<CL_MAIN_PLAYER_INFO_NFY>(), this);
                break;
            case MessageType.CL_INFO_END_NFY:
                LobbyServerHandler.InfoEndNfy(rawMessage.To<CL_INFO_END_NFY>(), this);
                break;
            case MessageType.CL_PLAYER_STAGE_NFY:
                LobbyServerHandler.PlayerStageNfy(rawMessage.To<CL_PLAYER_STAGE_NFY>(), this);
                break;
            case MessageType.CL_REGISTER_RES:
                LobbyServerHandler.RegisterRes(rawMessage.To<CL_REGISTER_RES>(), this);
                break;
            case MessageType.CL_CHAT_NORMAL_NFY:
                LobbyServerHandler.ChatNormalNfy(rawMessage.To<CL_CHAT_NORMAL_NFY>(), this);
                break;
            case MessageType.CL_CHAT_WHISPER_NFY:
                LobbyServerHandler.ChatWhisperNfy(rawMessage.To<CL_CHAT_WHISPER_NFY>(), this);
                break;
            case MessageType.CL_PLAYER_LOBBY_LIST_RES:
                LobbyServerHandler.LobbyListRes(rawMessage.To<CL_PLAYER_LOBBY_LIST_RES>(), this);
                break;
            case MessageType.CL_PLAYER_CREATE_RES:
                LobbyServerHandler.PlayerCreateRes(rawMessage.To<CL_PLAYER_CREATE_RES>(), this);
                break;
            case MessageType.CL_FRIEND_INFO_NFY:
                LobbyServerHandler.FriendInfoNfy(rawMessage.To<CL_FRIEND_INFO_NFY>(), this);
                break;
            case MessageType.CL_FRIEND_REQUEST_RES:
                LobbyServerHandler.FriendRequestRes(rawMessage.To<CL_FRIEND_REQUEST_RES>(), this);
                break;
            case MessageType.CL_FRIEND_RESPONSE_RES:
                LobbyServerHandler.FriendResponseRes(rawMessage.To<CL_FRIEND_RESPONSE_RES>(), this);
                break;
            case MessageType.CL_FRIEND_REMOVE_RES:
                LobbyServerHandler.FriendRemoveRes(rawMessage.To<CL_FRIEND_REMOVE_RES>(), this);
                break;
            case MessageType.CL_FRIEND_ONLINE_NFY:
                LobbyServerHandler.FriendOnlineNfy(rawMessage.To<CL_FRIEND_ONLINE_NFY>(), this);
                break;
            case MessageType.CL_FRIEND_OFFLINE_NFY:
                LobbyServerHandler.FriendOfflineNfy(rawMessage.To<CL_FRIEND_OFFLINE_NFY>(), this);
                break;
            default:
                CLog.W("Unrecognized message type: {0}.", rawMessage.MsgType);
                break;
        }
    }
}
