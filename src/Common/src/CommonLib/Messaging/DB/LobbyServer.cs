#if _SERVER
using CommonLib.Messaging.Base;
using CommonLib.Messaging.Common;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib.Messaging.DB
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DL_STARTUP_INFO_REQ : IMessage
    {
        public DL_STARTUP_INFO_REQ() { MsgType = MessageType.DL_STARTUP_INFO_REQ; }
        public MessageType MsgType { get; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DL_LIST_MAP_RES : IMessage
    {
        public DL_LIST_MAP_RES() { MsgType = MessageType.DL_LIST_MAP_RES; }
        public MessageType MsgType { get; }
        public List<MAP_INFO> mapList;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DL_FB_AUTH_REQ : IMessage
    {
        public DL_FB_AUTH_REQ() { MsgType = MessageType.DL_FB_AUTH_REQ; }
        public MessageType MsgType { get; }
        public string token;
        public string id;
        public string name;
        public string email;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DL_FB_AUTH_RES : IMessage
    {
        public DL_FB_AUTH_RES() { MsgType = MessageType.DL_FB_AUTH_RES; }
        public MessageType MsgType { get; }
        public string login;
        public MessageError error;
        public PLAYER_INFO info;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DL_AUTH_PLAYER_REQ : IMessage
    {
        public DL_AUTH_PLAYER_REQ() { MsgType = MessageType.DL_AUTH_PLAYER_REQ; }
        public MessageType MsgType { get; }
        public string login;
        public string pass;
        public string token;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DL_AUTH_PLAYER_RES : IMessage
    {
        public DL_AUTH_PLAYER_RES() { MsgType = MessageType.DL_AUTH_PLAYER_RES; }
        public MessageType MsgType { get; }
        public string login;
        public MessageError error;
        public PLAYER_INFO info;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DL_REGISTER_REQ : IMessage
    {
        public DL_REGISTER_REQ() { MsgType = MessageType.DL_REGISTER_REQ; }
        public MessageType MsgType { get; }
        public uint sessionId;
        public string login;
        public string pass;
        public string email;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DL_REGISTER_RES : IMessage
    {
        public DL_REGISTER_RES() { MsgType = MessageType.DL_REGISTER_RES; }
        public MessageType MsgType { get; }
        public uint sessionId;
        public MessageError error;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DL_PLAYER_ADD_INFO_REQ : IMessage
    {
        public DL_PLAYER_ADD_INFO_REQ() { MsgType = MessageType.DL_PLAYER_ADD_INFO_REQ; }
        public MessageType MsgType { get; }
        public ulong index;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DL_PLAYER_ADD_INFO_RES : IMessage
    {
        public DL_PLAYER_ADD_INFO_RES() { MsgType = MessageType.DL_PLAYER_ADD_INFO_RES; }
        public MessageType MsgType { get; }
        public ulong index;
        public List<FRIEND_INFO> friends;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DL_PLAYER_CREATE_REQ : IMessage
    {
        public DL_PLAYER_CREATE_REQ() { MsgType = MessageType.DL_PLAYER_CREATE_REQ; }
        public MessageType MsgType { get; }
        public uint sessionId;
        public ulong index;
        public string nick;
        public PlayerGender gender;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DL_PLAYER_CREATE_RES : IMessage
    {
        public DL_PLAYER_CREATE_RES() { MsgType = MessageType.DL_PLAYER_CREATE_RES; }
        public MessageType MsgType { get; }
        public MessageError error;
        public uint sessionId;
        public ulong index;
        public string nick;
        public PlayerGender gender;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DL_PLAYER_LEVEL_REQ : IMessage
    {
        public DL_PLAYER_LEVEL_REQ() { MsgType = MessageType.DL_PLAYER_LEVEL_REQ; }
        public MessageType MsgType { get; }
        public uint sessionId;
        public ulong index;
        public uint level;
        public ulong experience;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DL_PLAYER_LEVEL_RES : IMessage
    {
        public DL_PLAYER_LEVEL_RES() { MsgType = MessageType.DL_PLAYER_LEVEL_RES; }
        public MessageType MsgType { get; }
        public MessageError error;
        public uint sessionId;
        public ulong index;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DL_PLAYER_EXPERIENCE_REQ : IMessage
    {
        public DL_PLAYER_EXPERIENCE_REQ() { MsgType = MessageType.DL_PLAYER_EXPERIENCE_REQ; }
        public MessageType MsgType { get; }
        public uint sessionId;
        public ulong index;
        public ulong experience;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DL_PLAYER_EXPERIENCE_RES : IMessage
    {
        public DL_PLAYER_EXPERIENCE_RES() { MsgType = MessageType.DL_PLAYER_EXPERIENCE_RES; }
        public MessageType MsgType { get; }
        public MessageError error;
        public uint sessionId;
        public ulong index;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DL_FRIEND_REQUEST_REQ : IMessage
    {
        public DL_FRIEND_REQUEST_REQ() { MsgType = MessageType.DL_FRIEND_REQUEST_REQ; }
        public MessageType MsgType { get; }
        public string requester;
        public string requested;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DL_FRIEND_REQUEST_RES : IMessage
    {
        public DL_FRIEND_REQUEST_RES() { MsgType = MessageType.DL_FRIEND_REQUEST_RES; }
        public MessageType MsgType { get; }
        public MessageError error;
        public string requester;
        public string requested;
        public ulong friendIndex;
        public string friendLogin;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DL_FRIEND_RESPONSE_REQ : IMessage
    {
        public DL_FRIEND_RESPONSE_REQ() { MsgType = MessageType.DL_FRIEND_RESPONSE_REQ; }
        public MessageType MsgType { get; }
        public string requester;
        public string requested;
        public bool accept;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DL_FRIEND_RESPONSE_RES : IMessage
    {
        public DL_FRIEND_RESPONSE_RES() { MsgType = MessageType.DL_FRIEND_RESPONSE_RES; }
        public MessageType MsgType { get; }
        public MessageError error;
        public string requester;
        public string requested;
        public bool accept;
        public ulong friendIndex;
        public string friendLogin;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DL_FRIEND_REMOVE_REQ : IMessage
    {
        public DL_FRIEND_REMOVE_REQ() { MsgType = MessageType.DL_FRIEND_REMOVE_REQ; }
        public MessageType MsgType { get; }
        public string nick;
        public string friend;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DL_FRIEND_REMOVE_RES : IMessage
    {
        public DL_FRIEND_REMOVE_RES() { MsgType = MessageType.DL_FRIEND_REMOVE_RES; }
        public MessageType MsgType { get; }
        public MessageError error;
        public string nick;
        public string friend;
    }
}
#endif