#if _SERVER

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
    public class DX_TOKEN_PLAYER_REQ : IMessage
    {
        public DX_TOKEN_PLAYER_REQ() { MsgType = MessageType.DX_TOKEN_PLAYER_REQ; }
        public MessageType MsgType { get; }
        public uint id;
        public string token;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DX_TOKEN_PLAYER_RES : IMessage
    {
        public DX_TOKEN_PLAYER_RES() { MsgType = MessageType.DX_TOKEN_PLAYER_RES; }
        public MessageType MsgType { get; }
        public MessageError error;
        public uint id;
        public string token;
        public string login;
        public PLAYER_INFO info;
    }
}

#endif