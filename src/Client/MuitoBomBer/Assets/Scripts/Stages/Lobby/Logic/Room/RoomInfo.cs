using CommonLib.Messaging.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class RoomInfo
{
    private readonly uint _index;
    public uint Index { get { return _index; } }

    private uint _mapId;
    public uint MapId
    {
        get
        {
            return _mapId;
        }

        set
        {
            _mapId = value;
        }
    }

    private uint _maxPlayer;
    public uint MaxPlayer
    {
        get
        {
            return _maxPlayer;
        }

        set
        {
            _maxPlayer = value;
        }
    }

    private string _name;
    public string Name
    {
        get
        {
            return _name;
        }

        set
        {
            _name = value;
        }
    }

    private string _owner;
    public string Owner
    {
        get
        {
            return _owner;
        }

        set
        {
            _owner = value;
        }
    }

    private uint _playerCnt;
    public uint PlayerCnt
    {
        get
        {
            return _playerCnt;
        }

        set
        {
            _playerCnt = value;
        }
    }

    private string _password;
    public string Password
    {
        get
        {
            return _password;
        }

        set
        {
            _password = value;
        }
    }

    private bool _isPublic;
    public bool IsPublic
    {
        get
        {
            return _isPublic;
        }

        set
        {
            _isPublic = value;
        }
    }

    private CommonLib.Messaging.RoomStage _stage;
    public CommonLib.Messaging.RoomStage RoomStage
    {
        get
        {
            return _stage;
        }

        set
        {
            _stage = value;
        }
    }

    public RoomInfo(uint index)
    {
        _index = index;
    }

    public void Assign(ROOM_INFO info)
    {
        MapId = info.mapId;
        MaxPlayer = info.maxPlayer;
        Name = info.name;
        Owner = info.owner;
        PlayerCnt = info.playerCnt;
        RoomStage = info.stage;
        Password = info.password;
        IsPublic = info.isPublic;
    }
}
