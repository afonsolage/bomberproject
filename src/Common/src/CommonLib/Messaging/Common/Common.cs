using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib.Messaging.Common
{
    public interface IMessage
    {
        MessageType MsgType { get; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class TypeOnlyMessage : IMessage
    {
        public MessageType MsgType { get; set; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class MAP_INFO
    {
        public uint index;
        public string name;
        public ushort width;
        public ushort height;
        public ushort playerCnt;
        public byte[] data;
        public uint background;
        public Dictionary<string, string> behaviour;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class JOIN_MAP_INFO
    {
        public uint index;
        public uint owner;
        public ushort width;
        public ushort height;
        public byte[] data;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class VEC2
    {
        public float x;
        public float y;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class BASE_OBJECT_ATTRIBUTES
    {
        public uint lifePoints;
        public uint attackPoints;
        public uint defensePoints;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PLAYER_ATTRIBUTES : BASE_OBJECT_ATTRIBUTES
    {
        public uint bombCount;
        public uint bombArea;
        public float immunityTime;
        public bool kickBomb;
        public COMMON_PLAYER_ATTRIBUTES common;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class COMMON_PLAYER_ATTRIBUTES : BASE_OBJECT_ATTRIBUTES
    {
        public uint moveSpeed;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PLAYER_INFO
    {
        public ulong index;
        public string nick;
        public PlayerGender gender;
        public PlayerStage stage;
        public PlayerState state;
        public uint level;
        public ulong experience;
        public byte privilege;
        public bool firstLogin;
        public uint roomIndex;
        public int roomSlotIndex;
        public long ping;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class SERVER_INFO
    {
        public string address;
        public int port;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class ROOM_INFO
    {
        public uint index;
        public string name;
        public string owner;
        public uint maxPlayer;
        public uint playerCnt;
        public uint mapId;
        public string password;
        public bool isPublic;
        public RoomStage stage;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class ROOM_PLAYER_INFO
    {
        public uint uid;
        public bool alive;
        public VEC2 gridPos;
        public string nick;
        public PlayerGender gender;
        public PLAYER_ATTRIBUTES attr;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class SLOT_PLAYER
    {
        /// <summary>
        /// Index of player in current slot.
        /// </summary>
        public ulong playerIndex;

        /// <summary>
        /// Index of the slot.
        /// </summary>
        public int slotIndex;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class FRIEND_INFO
    {
        public ulong index;
        public string login;
        public string nick;
        public FriendState state;
    }
}
