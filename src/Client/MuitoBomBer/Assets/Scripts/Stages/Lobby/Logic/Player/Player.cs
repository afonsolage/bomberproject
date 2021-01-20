using CommonLib.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    protected readonly ulong _index;
    public ulong Index { get { return _index; } }

    protected readonly string _nick;
    public string Nick { get { return _nick; } }

    private readonly PlayerGender _gender;
    public PlayerGender Gender { get { return _gender; } }

    private readonly uint _level;
    public uint Level { get { return _level; } }

    private readonly ulong _experience;
    public ulong Experience { get { return _experience; } }

    public bool Offline { get; set; }
    public bool Ready { get; set; }

    public long Ping { get; set; }

    public Player(ulong index, string nick, PlayerGender gender, uint level, ulong experience)
    {
        _index = index;
        _nick = nick;
        _gender = gender;

        _level = level;
        _experience = experience;
    }
}

public class MainPlayer : Player
{
    public PlayerStage Stage { get; set; }
    public uint RoomIndex { get; set; }

    public MainPlayer(ulong index, string nick, PlayerGender gender, uint level, ulong experience) : base(index, nick, gender, level, experience)
    {
    }
}
