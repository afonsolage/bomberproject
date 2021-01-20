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
    public class DR_STARTUP_INFO_REQ : IMessage
    {
        public DR_STARTUP_INFO_REQ() { MsgType = MessageType.DR_STARTUP_INFO_REQ; }
        public MessageType MsgType { get; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DR_LIST_MAP_RES : IMessage
    {
        public DR_LIST_MAP_RES() { MsgType = MessageType.DR_LIST_MAP_RES; }
        public MessageType MsgType { get;}
        public List<MAP_INFO> mapList;
    }
    
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DR_LIST_CELL_TYPES_RES : IMessage
    {
        public DR_LIST_CELL_TYPES_RES() { MsgType = MessageType.DR_LIST_CELL_TYPES_RES; }
        public MessageType MsgType { get; }
        public List<Tuple<int, int>> typeList;
    }
    
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DR_LIST_POWERUP_RES : IMessage
    {
        public DR_LIST_POWERUP_RES() { MsgType = MessageType.DR_LIST_POWERUP_RES; }
        public MessageType MsgType { get; }
        public List<POWERUP> powerupList;
    }
}
#endif