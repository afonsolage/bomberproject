using CommonLib.Messaging.Common;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib.Messaging.Client
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CX_TOKEN_REQ : IMessage
    {
        public CX_TOKEN_REQ() { MsgType = MessageType.CX_TOKEN_REQ; }
        public MessageType MsgType { get; }
        public string token;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CX_TOKEN_RES : IMessage
    {
        public CX_TOKEN_RES() { MsgType = MessageType.CX_TOKEN_RES; }
        public MessageType MsgType { get; }
        public MessageError error;
        public bool firstLogin;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CX_DISCONNECTED_NFY : IMessage
    {
        public CX_DISCONNECTED_NFY() { MsgType = MessageType.CX_DISCONNECTED_NFY; }
        public MessageType MsgType { get; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CX_PLAYER_OFFLINE_NFY : IMessage
    {
        public CX_PLAYER_OFFLINE_NFY() { MsgType = MessageType.CX_PLAYER_OFFLINE_NFY; }
        public MessageType MsgType { get; }
        public ulong index;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CX_PLAYER_ONLINE_NFY : IMessage
    {
        public CX_PLAYER_ONLINE_NFY() { MsgType = MessageType.CX_PLAYER_ONLINE_NFY; }
        public MessageType MsgType { get; }
        public ulong index;
    }
}
