#if _SERVER
using CommonLib.Messaging.Common;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib.Messaging.Lobby
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class LR_WELCOME_NFY : IMessage
    {
        public LR_WELCOME_NFY() { MsgType = MessageType.LR_WELCOME_NFY; }
        public MessageType MsgType { get; }
        public uint uid;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class LR_SERVER_INFO_NFY : IMessage
    {
        public LR_SERVER_INFO_NFY() { MsgType = MessageType.LR_SERVER_INFO_NFY; }
        public MessageType MsgType { get; }
        public string ip;
        public int port;
        public int capacity;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class LR_USER_COUNT_NFY : IMessage
    {
        public LR_USER_COUNT_NFY() { MsgType = MessageType.LR_USER_COUNT_NFY; }
        public MessageType MsgType { get; }
        public uint count;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class LR_CREATE_ROOM_REQ : IMessage
    {
        public LR_CREATE_ROOM_REQ() { MsgType = MessageType.LR_CREATE_ROOM_REQ; }
        public MessageType MsgType { get; }
        public uint lobbyRoomIdx;
        public CREATE_ROOM_INFO info;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class LR_CREATE_ROOM_RES : IMessage
    {
        public LR_CREATE_ROOM_RES() { MsgType = MessageType.LR_CREATE_ROOM_RES; }
        public MessageType MsgType { get; }
        public uint lobbyRoomIdx;
        public uint roomIdx;
        public MessageError error;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class LR_ROOM_FINISHED_NFY : IMessage
    {
        public LR_ROOM_FINISHED_NFY() { MsgType = MessageType.LR_ROOM_FINISHED_NFY; }
        public MessageType MsgType { get; }
        public uint index;
        public MATCH_INFO info;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class LR_DUMMY_JOIN_NFY : IMessage
    {
        public LR_DUMMY_JOIN_NFY() { MsgType = MessageType.LR_DUMMY_JOIN_NFY; }
        public MessageType MsgType { get; }
        public string login;
        public uint dummyIndex;
        public uint roomIndex;
    }
}

#endif