#if _SERVER
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib.Messaging.Common
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class POWERUP
    {
        public uint index;
        public string name;
        public uint icon;
        public float rate;
        public Dictionary<string, string> behaviour;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CREATE_ROOM_INFO
    {
        public uint index;
        public string name;
        public string ownerLogin;
        public uint maxPlayer;
        public uint playerCnt;
        public uint mapId;
        public bool hasPassword;
        public RoomStage stage;
        public List<SLOT_PLAYER> slotPlayer;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class MATCH_INFO
    {
        //TODO: Add more info about the match result here, like number of enemies killed, time of death, etc
        public uint winner;
    }
}
#endif