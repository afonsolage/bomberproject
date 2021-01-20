using CommonLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class SlotPlayer
{
    public ulong _playerIndex;
    private readonly int _slotIndex;
    public int SlotIndex { get { return _slotIndex; } }

    private bool _isSlotOpen;
    public bool IsSlotOpen { get { return _isSlotOpen; } }

    public SlotPlayer(int slotId, bool slotOpenned)
    {
        _slotIndex = slotId;
        _isSlotOpen = slotOpenned;
    }

    public void Reset()
    {
        _playerIndex = 0;
        _isSlotOpen = true;
    }

    public void OpenSlot()
    {
        _isSlotOpen = true;
    }

    public void CloseSlot()
    {
        _isSlotOpen = false;
    }
}

class WaitingRoom
{
    /// <summary>
    /// Maximum number of slots in room.
    /// </summary>
    private readonly int MAX_NUM_USER_SLOT = 6;

    private RoomInfo _info;
    public RoomInfo Info { get { return _info; } }

    private SlotPlayer[] _slotPlayer;

    private List<Player> _players;
    public List<Player> Players { get { return _players; } }

    public WaitingRoom()
    {
        _slotPlayer = new SlotPlayer[MAX_NUM_USER_SLOT];
        for (var i = 0; i < MAX_NUM_USER_SLOT; i++)
        {
            _slotPlayer[i] = new SlotPlayer(i, true);
        }

        _players = new List<Player>();
    }

    public void Update(RoomInfo info)
    {
        _info = info;
    }

    public void PlayerJoined(int slotIndex, Player player)
    {
        var slot = FindSlotByIndex(slotIndex);
        if(slot != null)
        {
            slot._playerIndex = player.Index;
        }
        else
        {
            CLog.E("Failed to find slot index {0}.", slotIndex);
        }

        _players.Add(player);
    }

    public void PlayerLeft(ulong playerIndex)
    {
        var slot = FindPlayerSlot(playerIndex);
        if (slot != null)
        {
            slot._playerIndex = 0;
        }
        else
        {
            CLog.E("Failed to find slot with player index in slot{0}.", playerIndex);
        }

        _players.RemoveAll(p => p.Index == playerIndex);
    }

    internal void Clear()
    {
        _info = null;

        for (var i = 0; i < MAX_NUM_USER_SLOT; i++)
        {
            if (_slotPlayer[i] == null)
                continue;

            _slotPlayer[i]._playerIndex = 0;
            _slotPlayer[i].OpenSlot();
        }

        _players.Clear();
    }

    internal void SetPlayerOnline(ulong index)
    {
        var player = _players.Find(p => p.Index == index);
        if (player == null)
            return;

        player.Offline = false;
    }

    internal void SetPlayerOffline(ulong index)
    {
        var player = _players.Find(p => p.Index == index);
        if (player == null)
            return;

        player.Offline = true;
    }

    internal void SetPlayerReady(ulong index, bool ready)
    {
        var player = _players.Find(p => p.Index == index);
        if (player == null)
            return;

        player.Ready = ready;
    }

    private void ClearSlot()
    {
        foreach (var slot in _slotPlayer)
        {
            if (slot == null)
                continue;

            slot._playerIndex = 0;
            slot.OpenSlot();
        }
    }

    /// <summary>
    /// Find the slot by slot index.
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <returns></returns>
    internal SlotPlayer FindSlotByIndex(int index)
    {
        foreach (var slot in _slotPlayer)
        {
            if (slot == null)
                continue;

            if (slot.SlotIndex == index)
                return slot;
        }

        return null;
    }

    /// <summary>
    /// Find the player slot from the player index.
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <returns></returns>
    internal SlotPlayer FindPlayerSlot(ulong playerIndex)
    {
        foreach (var slot in _slotPlayer)
        {
            if (slot == null)
                continue;

            if (slot._playerIndex == playerIndex)
                return slot;
        }

        return null;
    }
}
