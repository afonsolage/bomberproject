using CommonLib.Messaging.Common;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib.Messaging.Client
{
    public interface ILobbyMessage : IMessage { }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_WELCOME_NFY : IMessage
    {
        public CL_WELCOME_NFY() { MsgType = MessageType.CL_WELCOME_NFY; }
        public MessageType MsgType { get; }
        public string serverName;
        public ulong uid;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_AUTH_REQ : IMessage
    {
        public CL_AUTH_REQ() { MsgType = MessageType.CL_AUTH_REQ; }
        public MessageType MsgType { get; }
        public string login;
        public string pass;
        public string deviceID;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_AUTH_RES : IMessage
    {
        public CL_AUTH_RES() { MsgType = MessageType.CL_AUTH_RES; }
        public MessageType MsgType { get; }
        public MessageError error;
        public string token;
        public bool firstLogin;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_LOGOUT_REQ : IMessage
    {
        public CL_LOGOUT_REQ() { MsgType = MessageType.CL_LOGOUT_REQ; }
        public MessageType MsgType { get; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_FB_AUTH_REQ : IMessage
    {
        public CL_FB_AUTH_REQ() { MsgType = MessageType.CL_FB_AUTH_REQ; }
        public MessageType MsgType { get; }
        public string token;
        public string id;
        public string name;
        public string email;
        public string deviceID;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_FB_AUTH_RES : IMessage
    {
        public CL_FB_AUTH_RES() { MsgType = MessageType.CL_FB_AUTH_RES; }
        public MessageType MsgType { get; }
        public MessageError error;
        public string token;
        public bool firstLogin;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_REGISTER_REQ : IMessage
    {
        public CL_REGISTER_REQ() { MsgType = MessageType.CL_REGISTER_REQ; }
        public MessageType MsgType { get; }
        public string login;
        public string password;
        public string email;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_REGISTER_RES : IMessage
    {
        public CL_REGISTER_RES() { MsgType = MessageType.CL_REGISTER_RES; }
        public MessageType MsgType { get; }
        public MessageError error;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_PLAYER_HEARTBEAT_REQ : IMessage
    {
        public CL_PLAYER_HEARTBEAT_REQ() { MsgType = MessageType.CL_PLAYER_HEARTBEAT_REQ; }
        public MessageType MsgType { get; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_PLAYER_HEARTBEAT_RES : IMessage
    {
        public CL_PLAYER_HEARTBEAT_RES() { MsgType = MessageType.CL_PLAYER_HEARTBEAT_RES; }
        public MessageType MsgType { get; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_PLAYER_HEARTBEAT_NFY : IMessage
    {
        public CL_PLAYER_HEARTBEAT_NFY() { MsgType = MessageType.CL_PLAYER_HEARTBEAT_NFY; }
        public MessageType MsgType { get; }
        public long ping;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_PLAYER_CREATE_REQ : IMessage
    {
        public CL_PLAYER_CREATE_REQ() { MsgType = MessageType.CL_PLAYER_CREATE_REQ; }
        public MessageType MsgType { get; }
        public string nick;
        public PlayerGender gender;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_PLAYER_CREATE_RES : IMessage
    {
        public CL_PLAYER_CREATE_RES() { MsgType = MessageType.CL_PLAYER_CREATE_RES; }
        public MessageType MsgType { get; }
        public MessageError error;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_MAIN_PLAYER_INFO_NFY : IMessage
    {
        public CL_MAIN_PLAYER_INFO_NFY() { MsgType = MessageType.CL_MAIN_PLAYER_INFO_NFY; }
        public MessageType MsgType { get; }
        public PLAYER_INFO player;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_INFO_END_NFY : IMessage
    {
        public CL_INFO_END_NFY() { MsgType = MessageType.CL_INFO_END_NFY; }
        public MessageType MsgType { get; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_PLAYER_LEVEL_RES : IMessage
    {
        public CL_PLAYER_LEVEL_RES() { MsgType = MessageType.CL_PLAYER_LEVEL_RES; }
        public MessageType MsgType { get; }
        public uint newLevel;
        public ulong newExperience;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_PLAYER_EXPERIENCE_RES : IMessage
    {
        public CL_PLAYER_EXPERIENCE_RES() { MsgType = MessageType.CL_PLAYER_EXPERIENCE_RES; }
        public MessageType MsgType { get; }
        public ulong newExperience;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_ROOM_LIST_NFY : IMessage
    {
        public CL_ROOM_LIST_NFY() { MsgType = MessageType.CL_ROOM_LIST_NFY; }
        public MessageType MsgType { get; }
        public List<ROOM_INFO> rooms;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_PLAYER_JOINED_NFY : IMessage
    {
        public CL_PLAYER_JOINED_NFY() { MsgType = MessageType.CL_PLAYER_JOINED_NFY; }
        public MessageType MsgType { get; }
        public uint roomIndex;
        public PLAYER_INFO player;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_PLAYER_LEFT_NFY : IMessage
    {
        public CL_PLAYER_LEFT_NFY() { MsgType = MessageType.CL_PLAYER_LEFT_NFY; }
        public MessageType MsgType { get; }
        public uint roomIndex;
        public ulong playerIndex;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_PLAYER_READY_REQ : IMessage
    {
        public CL_PLAYER_READY_REQ() { MsgType = MessageType.CL_PLAYER_READY_REQ; }
        public MessageType MsgType { get; }
        public bool ready;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_PLAYER_READY_RES : IMessage
    {
        public CL_PLAYER_READY_RES() { MsgType = MessageType.CL_PLAYER_READY_RES; }
        public MessageType MsgType { get; }
        public MessageError error;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_PLAYER_READY_NFY : IMessage
    {
        public CL_PLAYER_READY_NFY() { MsgType = MessageType.CL_PLAYER_READY_NFY; }
        public MessageType MsgType { get; }
        public ulong index;
        public bool ready;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_ROOM_START_REQ : IMessage
    {
        public CL_ROOM_START_REQ() { MsgType = MessageType.CL_ROOM_START_REQ; }
        public MessageType MsgType { get; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_ROOM_START_RES : IMessage
    {
        public CL_ROOM_START_RES() { MsgType = MessageType.CL_ROOM_START_RES; }
        public MessageType MsgType { get; }
        public MessageError error;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_ROOM_START_NFY : IMessage
    {
        public CL_ROOM_START_NFY() { MsgType = MessageType.CL_ROOM_START_NFY; }
        public MessageType MsgType { get; }
        public SERVER_INFO serverInfo;
        public uint roomIndex;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_ROOM_CREATE_REQ : IMessage
    {
        public CL_ROOM_CREATE_REQ() { MsgType = MessageType.CL_ROOM_CREATE_REQ; }
        public MessageType MsgType { get; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_ROOM_CREATE_RES : IMessage
    {
        public CL_ROOM_CREATE_RES() { MsgType = MessageType.CL_ROOM_CREATE_RES; }
        public MessageType MsgType { get; }
        public MessageError error;
        public ROOM_INFO info;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_ROOM_CREATED_NFY : IMessage
    {
        public CL_ROOM_CREATED_NFY() { MsgType = MessageType.CL_ROOM_CREATED_NFY; }
        public MessageType MsgType { get; }
        public ROOM_INFO info;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_ROOM_DESTROYED_NFY : IMessage
    {
        public CL_ROOM_DESTROYED_NFY() { MsgType = MessageType.CL_ROOM_DESTROYED_NFY; }
        public MessageType MsgType { get; }
        public uint index;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_ROOM_UPDATED_NFY : IMessage
    {
        public CL_ROOM_UPDATED_NFY() { MsgType = MessageType.CL_ROOM_UPDATED_NFY; }
        public MessageType MsgType { get; }
        public ROOM_INFO info;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_ROOM_HEARTBEAT_NFY : IMessage
    {
        public CL_ROOM_HEARTBEAT_NFY() { MsgType = MessageType.CL_ROOM_HEARTBEAT_NFY; }
        public MessageType MsgType { get; }
        public ulong playerIdx;
        public long ping;
        public uint roomIdx;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_ROOM_JOIN_REQ : IMessage
    {
        public CL_ROOM_JOIN_REQ() { MsgType = MessageType.CL_ROOM_JOIN_REQ; }
        public MessageType MsgType { get; }
        public uint index;
        public string password;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_ROOM_JOIN_RES : IMessage
    {
        public CL_ROOM_JOIN_RES() { MsgType = MessageType.CL_ROOM_JOIN_RES; }
        public MessageType MsgType { get; }
        public MessageError error;
        public ROOM_INFO info;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_ROOM_LEAVE_REQ : IMessage
    {
        public CL_ROOM_LEAVE_REQ() { MsgType = MessageType.CL_ROOM_LEAVE_REQ; }
        public MessageType MsgType { get; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_ROOM_LEAVE_RES : IMessage
    {
        public CL_ROOM_LEAVE_RES() { MsgType = MessageType.CL_ROOM_LEAVE_RES; }
        public MessageType MsgType { get; }
        public MessageError error;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_ROOM_SETTING_REQ : IMessage
    {
        public CL_ROOM_SETTING_REQ() { MsgType = MessageType.CL_ROOM_SETTING_REQ; }
        public MessageType MsgType { get; }
        public string title;
        public string pw;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_ROOM_SETTING_RES : IMessage
    {
        public CL_ROOM_SETTING_RES() { MsgType = MessageType.CL_ROOM_SETTING_RES; }
        public MessageType MsgType { get; }
        public MessageError error;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_ROOM_CHANGE_SLOT_POS_REQ : IMessage
    {
        public CL_ROOM_CHANGE_SLOT_POS_REQ() { MsgType = MessageType.CL_ROOM_CHANGE_SLOT_POS_REQ; }
        public MessageType MsgType { get; }
        public int currentSlot;
        public int newSlot;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_ROOM_CHANGE_SLOT_POS_RES : IMessage
    {
        public CL_ROOM_CHANGE_SLOT_POS_RES() { MsgType = MessageType.CL_ROOM_CHANGE_SLOT_POS_RES; }
        public MessageType MsgType { get; }
        public MessageError error;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_ROOM_CHANGE_SLOT_POS_NFY : IMessage
    {
        public CL_ROOM_CHANGE_SLOT_POS_NFY() { MsgType = MessageType.CL_ROOM_CHANGE_SLOT_POS_NFY; }
        public MessageType MsgType { get; }
        public ulong playerIndex1;
        public ulong playerIndex2;
        public int oldSlot1;
        public int oldSlot2;
        public int newSlot1;
        public int newSlot2;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_ROOM_CHANGE_SLOT_SINGLE_POS_NFY : IMessage
    {
        public CL_ROOM_CHANGE_SLOT_SINGLE_POS_NFY() { MsgType = MessageType.CL_ROOM_CHANGE_SLOT_SINGLE_POS_NFY; }
        public MessageType MsgType { get; }
        public ulong playerIndex;
        public int oldSlot;
        public int newSlot;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_CHAT_NORMAL_REQ : IMessage
    {
        public CL_CHAT_NORMAL_REQ() { MsgType = MessageType.CL_CHAT_NORMAL_REQ; }
        public MessageType MsgType { get; }
        public ulong uid;
        public string name;
        public string msg;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_CHAT_NORMAL_RES : IMessage
    {
        public CL_CHAT_NORMAL_RES() { MsgType = MessageType.CL_CHAT_NORMAL_RES; }
        public MessageType MsgType { get; }
        public MessageError error;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_CHAT_NORMAL_NFY : IMessage
    {
        public CL_CHAT_NORMAL_NFY() { MsgType = MessageType.CL_CHAT_NORMAL_NFY; }
        public MessageType MsgType { get; }
        public ulong uid;
        public string name;
        public string msg;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_CHAT_WHISPER_REQ : IMessage
    {
        public CL_CHAT_WHISPER_REQ() { MsgType = MessageType.CL_CHAT_WHISPER_REQ; }
        public MessageType MsgType { get; }
        public ulong uid;
        public string toName;
        public string msg;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_CHAT_WHISPER_NFY : IMessage
    {
        public CL_CHAT_WHISPER_NFY() { MsgType = MessageType.CL_CHAT_WHISPER_NFY; }
        public MessageType MsgType { get; }
        public ulong uid;
        public string fromName;
        public string toName;
        public string msg;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_PLAYER_STAGE_NFY : IMessage
    {
        public CL_PLAYER_STAGE_NFY() { MsgType = MessageType.CL_PLAYER_STAGE_NFY; }
        public MessageType MsgType { get; }
        public PLAYER_INFO player;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_PLAYER_LOBBY_LIST_REQ : IMessage
    {
        public CL_PLAYER_LOBBY_LIST_REQ() { MsgType = MessageType.CL_PLAYER_LOBBY_LIST_REQ; }
        public MessageType MsgType { get; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_PLAYER_LOBBY_LIST_RES : IMessage
    {
        public CL_PLAYER_LOBBY_LIST_RES() { MsgType = MessageType.CL_PLAYER_LOBBY_LIST_RES; }
        public MessageType MsgType { get; }
        public List<PLAYER_INFO> players;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_ROOM_KICK_PLAYER_REQ : IMessage
    {
        public CL_ROOM_KICK_PLAYER_REQ() { MsgType = MessageType.CL_ROOM_KICK_PLAYER_REQ; }
        public MessageType MsgType { get; }
        public ulong playerIndex;
        public uint roomIndex;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_ROOM_KICK_PLAYER_RES : IMessage
    {
        public CL_ROOM_KICK_PLAYER_RES() { MsgType = MessageType.CL_ROOM_KICK_PLAYER_RES; }
        public MessageType MsgType { get; }
        public ulong playerIndex;
        public MessageError error;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_ROOM_KICK_PLAYER_NFY : IMessage
    {
        public CL_ROOM_KICK_PLAYER_NFY() { MsgType = MessageType.CL_ROOM_KICK_PLAYER_NFY; }
        public MessageType MsgType { get; }
        public ulong playerIndex;
        public uint roomIndex;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_ROOM_TRANSFER_OWNER_REQ : IMessage
    {
        public CL_ROOM_TRANSFER_OWNER_REQ() { MsgType = MessageType.CL_ROOM_TRANSFER_OWNER_REQ; }
        public MessageType MsgType { get; }
        public ulong playerIndex;
        public uint roomIndex;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_ROOM_TRANSFER_OWNER_RES : IMessage
    {
        public CL_ROOM_TRANSFER_OWNER_RES() { MsgType = MessageType.CL_ROOM_TRANSFER_OWNER_RES; }
        public MessageType MsgType { get; }
        public ulong playerIndex;
        public MessageError error;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_FRIEND_INFO_NFY : IMessage
    {
        public CL_FRIEND_INFO_NFY() { MsgType = MessageType.CL_FRIEND_INFO_NFY; }
        public MessageType MsgType { get; }
        public List<FRIEND_INFO> friends;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_FRIEND_REQUEST_REQ : IMessage
    {
        public CL_FRIEND_REQUEST_REQ() { MsgType = MessageType.CL_FRIEND_REQUEST_REQ; }
        public MessageType MsgType { get; }
        public string nick;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_FRIEND_REQUEST_RES : IMessage
    {
        public CL_FRIEND_REQUEST_RES() { MsgType = MessageType.CL_FRIEND_REQUEST_RES; }
        public MessageType MsgType { get; }
        public MessageError error;
        public string nick;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_FRIEND_RESPONSE_REQ : IMessage
    {
        public CL_FRIEND_RESPONSE_REQ() { MsgType = MessageType.CL_FRIEND_RESPONSE_REQ; }
        public MessageType MsgType { get; }
        public string nick;
        public bool accept;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_FRIEND_RESPONSE_RES : IMessage
    {
        public CL_FRIEND_RESPONSE_RES() { MsgType = MessageType.CL_FRIEND_RESPONSE_RES; }
        public MessageType MsgType { get; }
        public MessageError error;
        public string nick;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_FRIEND_REMOVE_REQ : IMessage
    {
        public CL_FRIEND_REMOVE_REQ() { MsgType = MessageType.CL_FRIEND_REMOVE_REQ; }
        public MessageType MsgType { get; }
        public string nick;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_FRIEND_REMOVE_RES : IMessage
    {
        public CL_FRIEND_REMOVE_RES() { MsgType = MessageType.CL_FRIEND_REMOVE_RES; }
        public MessageType MsgType { get; }
        public MessageError error;
        public string nick;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_FRIEND_ONLINE_NFY : IMessage
    {
        public CL_FRIEND_ONLINE_NFY() { MsgType = MessageType.CL_FRIEND_ONLINE_NFY; }
        public MessageType MsgType { get; }
        public string nick;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CL_FRIEND_OFFLINE_NFY : IMessage
    {
        public CL_FRIEND_OFFLINE_NFY() { MsgType = MessageType.CL_FRIEND_OFFLINE_NFY; }
        public MessageType MsgType { get; }
        public string nick;
    }
}