using System;
using CommonLib.Messaging.Base;
using CommonLib.Networking;
using CommonLib.Util;
using CommonLib.Messaging.Client;
using CommonLib.Messaging;

public class RoomServerConnection : ServerConnection
{
    public RoomStage Stage
    {
        get { return (RoomStage)_stage; }
    }

    public RoomServerConnection(RoomStage stage, string serverHost, int serverPort) : base(stage, serverHost, serverPort)
    {
    }

    public override void HandleOnMainThread(Packet packet)
    {
        var rawMessage = new RawMessage(packet.buffer);

        switch (rawMessage.MsgType)
        {
            case MessageType.CX_TOKEN_RES:
                RoomServerHandler.TokenRes(rawMessage.To<CX_TOKEN_RES>(), this);
                break;
            case MessageType.CR_JOIN_ROOM_RES:
                RoomServerHandler.JoinRoomRes(rawMessage.To<CR_JOIN_ROOM_RES>(), this);
                break;
            case MessageType.CR_JOIN_ROOM_NFY:
                RoomServerHandler.JoinRoomNfy(rawMessage.To<CR_JOIN_ROOM_NFY>(), this);
                break;
            case MessageType.CR_WELCOME_NFY:
                RoomServerHandler.WelcomeNfy(rawMessage.To<CR_WELCOME_NFY>(), this);
                break;
            case MessageType.CR_PLAYER_ENTER_NFY:
                RoomServerHandler.PlayerEnterNfy(rawMessage.To<CR_PLAYER_ENTER_NFY>(), this);
                break;
            case MessageType.CR_PLAYER_LEAVE_NFY:
                RoomServerHandler.PlayerLeaveNfy(rawMessage.To<CR_PLAYER_LEAVE_NFY>(), this);
                break;
            case MessageType.CR_PLAYER_POS_NFY:
                RoomServerHandler.PlayerPosNfy(rawMessage.To<CR_PLAYER_POS_NFY>(), this);
                break;
            case MessageType.CR_PLAYER_UPDATE_ATTRIBUTES_RES:
                RoomServerHandler.PlayerUpdateAttributesRes(rawMessage.To<CR_PLAYER_UPDATE_ATTRIBUTES_RES>(), this);
                break;
            case MessageType.CR_PLAYER_HIT_NFY:
                RoomServerHandler.PlayerHitNfy(rawMessage.To<CR_PLAYER_HIT_NFY>(), this);
                break;
            case MessageType.CR_PLAYER_DIED_NFY:
                RoomServerHandler.PlayerDiedNfy(rawMessage.To<CR_PLAYER_DIED_NFY>(), this);
                break;
            case MessageType.CR_IMMUNITY_NFY:
                RoomServerHandler.ImmunityNfy(rawMessage.To<CR_IMMUNITY_NFY>(), this);
                break;
            case MessageType.CR_SPEED_CHANGE_NFY:
                RoomServerHandler.SpeedChangeNfy(rawMessage.To<CR_SPEED_CHANGE_NFY>(), this);
                break;
            case MessageType.CR_PLACE_BOMB_RES:
                RoomServerHandler.PlaceBombRes(rawMessage.To<CR_PLACE_BOMB_RES>(), this);
                break;
            case MessageType.CR_BOMB_PLACED_NFY:
                RoomServerHandler.BombPlacedNfy(rawMessage.To<CR_BOMB_PLACED_NFY>(), this);
                break;
            case MessageType.CR_BOMB_EXPLODED_NFY:
                RoomServerHandler.BombExplodedNfy(rawMessage.To<CR_BOMB_EXPLODED_NFY>(), this);
                break;
            case MessageType.CR_BOMB_EXPLODED_OBJECT_NFY:
                RoomServerHandler.BombExplodedObjectNfy(rawMessage.To<CR_BOMB_EXPLODED_OBJECT_NFY>(), this);
                break;
            case MessageType.CR_BOMB_POS_NFY:
                RoomServerHandler.BombPosNfy(rawMessage.To<CR_BOMB_POS_NFY>(), this);
                break;
            case MessageType.CR_HURRY_UP_CELL_NFY:
                RoomServerHandler.HurryUpCellNfy(rawMessage.To<CR_HURRY_UP_CELL_NFY>(), this);
                break;
            case MessageType.CR_POWERUP_ADD_NFY:
                RoomServerHandler.PowerUpAddNfy(rawMessage.To<CR_POWERUP_ADD_NFY>(), this);
                break;
            case MessageType.CR_POWERUP_REMOVE_NFY:
                RoomServerHandler.PowerUpRemoveNfy(rawMessage.To<CR_POWERUP_REMOVE_NFY>(), this);
                break;
            case MessageType.CR_MATCH_END_NFY:
                RoomServerHandler.MatchEnd(rawMessage.To<CR_MATCH_END_NFY>(), this);
                break;
            default:
                CLog.W("Unrecognized message type: {0}.", rawMessage.MsgType);
                break;
        }
    }
}
