using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using CommonLib.Messaging.Base;
using CommonLib.Messaging.Common;
using CommonLib.GridEngine;

namespace CommonLib.Messaging.Client
{
    public interface IRoomMessage : IMessage { }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CR_WELCOME_NFY : IMessage
    {
        public CR_WELCOME_NFY() { MsgType = MessageType.CR_WELCOME_NFY; }
        public MessageType MsgType { get; }
        public string serverName;
        public uint uid;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CR_JOIN_ROOM_REQ : IMessage
    {
        public CR_JOIN_ROOM_REQ() { MsgType = MessageType.CR_JOIN_ROOM_REQ; }
        public MessageType MsgType { get; }
        public uint uid;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CR_JOIN_ROOM_RES : IMessage
    {
        public CR_JOIN_ROOM_RES() { MsgType = MessageType.CR_JOIN_ROOM_RES; }
        public MessageType MsgType { get; }
        public MessageError error;
        public uint mainUID;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CR_JOIN_ROOM_NFY : IMessage
    {
        public CR_JOIN_ROOM_NFY() { MsgType = MessageType.CR_JOIN_ROOM_NFY; }
        public MessageType MsgType { get; }
        public MAP_INFO info;
        public List<Tuple<int, int>> typeList;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CR_PLAYER_ENTER_NFY : IMessage
    {
        public CR_PLAYER_ENTER_NFY() { MsgType = MessageType.CR_PLAYER_ENTER_NFY; }
        public MessageType MsgType { get; }
        public ROOM_PLAYER_INFO info;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CR_PLAYER_LEAVE_NFY : IMessage
    {
        public CR_PLAYER_LEAVE_NFY() { MsgType = MessageType.CR_PLAYER_LEAVE_NFY; }
        public MessageType MsgType { get; }
        public uint uid;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CR_PLAYER_MOVE_SYNC_NFY : IRoomMessage
    {
        public CR_PLAYER_MOVE_SYNC_NFY() { MsgType = MessageType.CR_PLAYER_MOVE_SYNC_NFY; }
        public MessageType MsgType { get; }
        public float moveX;
        public float moveY;
        public VEC2 currentWorldPos;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CR_PLAYER_POS_NFY : IRoomMessage
    {
        public CR_PLAYER_POS_NFY() { MsgType = MessageType.CR_PLAYER_POS_NFY; }
        public MessageType MsgType { get; }
        public uint uid;
        public VEC2 worldPos;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CR_PLAYER_HIT_NFY : IRoomMessage
    {
        public CR_PLAYER_HIT_NFY() { MsgType = MessageType.CR_PLAYER_HIT_NFY; }
        public MessageType MsgType { get; }
        public uint uid;
        public uint hitter;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CR_PLAYER_UPDATE_ATTRIBUTES_RES : IRoomMessage
    {
        public CR_PLAYER_UPDATE_ATTRIBUTES_RES() { MsgType = MessageType.CR_PLAYER_UPDATE_ATTRIBUTES_RES; }
        public MessageType MsgType { get; }
        public uint uid;
        public PLAYER_ATTRIBUTES attributes;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CR_PLAYER_DIED_NFY : IRoomMessage
    {
        public CR_PLAYER_DIED_NFY() { MsgType = MessageType.CR_PLAYER_DIED_NFY; }
        public MessageType MsgType { get; }
        public uint uid;
        public uint killer;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CR_IMMUNITY_NFY : IRoomMessage
    {
        public CR_IMMUNITY_NFY() { MsgType = MessageType.CR_IMMUNITY_NFY; }
        public MessageType MsgType { get; }
        public uint uid;
        public float duration;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CR_SPEED_CHANGE_NFY : IRoomMessage
    {
        public CR_SPEED_CHANGE_NFY() { MsgType = MessageType.CR_SPEED_CHANGE_NFY; }
        public MessageType MsgType { get; }
        public uint uid;
        public uint speed;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CR_PLACE_BOMB_REQ : IRoomMessage
    {
        public CR_PLACE_BOMB_REQ() { MsgType = MessageType.CR_PLACE_BOMB_REQ; }
        public MessageType MsgType { get; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CR_PLACE_BOMB_RES : IRoomMessage
    {
        public CR_PLACE_BOMB_RES() { MsgType = MessageType.CR_PLACE_BOMB_RES; }
        public MessageType MsgType { get; }
        public MessageError error;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CR_BOMB_PLACED_NFY : IRoomMessage
    {
        public CR_BOMB_PLACED_NFY() { MsgType = MessageType.CR_BOMB_PLACED_NFY; }
        public MessageType MsgType { get; }
        public uint uid;
        public ushort gridX;
        public ushort gridY;
        public uint moveSpeed;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CR_BOMB_EXPLODED_NFY : IRoomMessage
    {
        public CR_BOMB_EXPLODED_NFY() { MsgType = MessageType.CR_BOMB_EXPLODED_NFY; }
        public MessageType MsgType { get; }
        public uint uid;
        public List<VEC2> area;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CR_BOMB_EXPLODED_OBJECT_NFY : IRoomMessage
    {
        public CR_BOMB_EXPLODED_OBJECT_NFY() { MsgType = MessageType.CR_BOMB_EXPLODED_OBJECT_NFY; }
        public MessageType MsgType { get; }
        public List<VEC2> area;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CR_BOMB_KICK_REQ : IRoomMessage
    {
        public CR_BOMB_KICK_REQ() { MsgType = MessageType.CR_BOMB_KICK_REQ; }
        public MessageType MsgType { get; }
        public uint uid;
        public uint uidBomb;
        public byte dir;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CR_BOMB_POS_NFY : IRoomMessage
    {
        public CR_BOMB_POS_NFY() { MsgType = MessageType.CR_BOMB_POS_NFY; }
        public MessageType MsgType { get; }
        public uint uid;
        public VEC2 worldPos;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CR_HURRY_UP_CELL_NFY : IRoomMessage
    {
        public CR_HURRY_UP_CELL_NFY() { MsgType = MessageType.CR_HURRY_UP_CELL_NFY; }
        public MessageType MsgType { get; }
        public VEC2 cell;
        public CellType replaceType;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CR_POWERUP_ADD_NFY : IRoomMessage
    {
        public CR_POWERUP_ADD_NFY() { MsgType = MessageType.CR_POWERUP_ADD_NFY; }
        public MessageType MsgType { get; }
        public VEC2 cell;
        public uint uid;
        public uint icon;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CR_POWERUP_REMOVE_NFY : IRoomMessage
    {
        public CR_POWERUP_REMOVE_NFY() { MsgType = MessageType.CR_POWERUP_REMOVE_NFY; }
        public MessageType MsgType { get; }
        public uint uid;
        public bool collected;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CR_MATCH_END_NFY : IRoomMessage
    {
        public CR_MATCH_END_NFY() { MsgType = MessageType.CR_MATCH_END_NFY; }
        public MessageType MsgType { get; }
        public uint winner;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CR_PLAYER_CHAT_NORMAL_REQ : IRoomMessage
    {
        public CR_PLAYER_CHAT_NORMAL_REQ() { MsgType = MessageType.CR_PLAYER_CHAT_NORMAL_REQ; }
        public MessageType MsgType { get; }
        public uint uid;
        public string message;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CR_PLAYER_CHAT_NORMAL_NFY : IRoomMessage
    {
        public CR_PLAYER_CHAT_NORMAL_NFY() { MsgType = MessageType.CR_PLAYER_CHAT_NORMAL_NFY; }
        public MessageType MsgType { get; }
        public uint uid;
        public string name;
        public string message;
    }
}
