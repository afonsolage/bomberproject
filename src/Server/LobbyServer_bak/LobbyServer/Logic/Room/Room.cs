using CommonLib.Messaging;
using CommonLib.Messaging.Client;
using CommonLib.Messaging.Common;
using CommonLib.Util;
using LobbyServer.Logic.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LobbyServer.Logic
{
    internal class SlotPlayer
    {
        /// <summary>
        /// Index of player in current slot.
        /// </summary>
        public ulong _playerIndex;

        /// <summary>
        /// Index of the slot.
        /// </summary>
        private readonly int _slotIndex;
        public int SlotIndex { get { return _slotIndex; } }

        /// <summary>
        /// If slot is openned or not.
        /// </summary>
        private bool _isSlotOpen;
        public bool IsSlotOpen { get { return _isSlotOpen; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="slotId"></param>
        /// <param name="slotOpenned"></param>
        public SlotPlayer(int slotId, bool slotOpenned)
        {
            _slotIndex = slotId;
            _isSlotOpen = slotOpenned;
        }

        /// <summary>
        /// Open slot.
        /// </summary>
        public void OpenSlot()
        {
            _isSlotOpen = true;
        }

        /// <summary>
        /// Close slot.
        /// </summary>
        public void CloseSlot()
        {
            _isSlotOpen = false;
        }
    }

    class Room : IEquatable<Room>
    {
        /// <summary>
        /// Maximum number of slots in room.
        /// </summary>
        public readonly int MAX_NUM_USER_SLOT = 6;

        /// <summary>
        /// Index of this room on Lobby Server. It is unique and can't be changed.
        /// </summary>
        private readonly uint _index;
        public uint Index => _index;

        /// <summary>
        /// Index of the room on Room Server. 0 If this room isn't on play mode.
        /// </summary>
        public uint RoomIndex { get; set; }

        /// <summary>
        /// Room is already playing.
        /// </summary>
        public bool IsPlaying { get => Stage == RoomStage.Playing; }

        /// <summary>
        /// Current map ID of the room.
        /// </summary>
        private readonly uint _mapId;
        public uint MapId { get => _mapId; }

        /// <summary>
        /// Current title of the room.
        /// </summary>
        private string _title;
        public string Title { get => _title; set => _title = value; }

        /// <summary>
        /// Current password of the room, if password is empty then room don't to use password.
        /// </summary>
        private string _password;
        public string Password { get => _password; set => _password = value; }

        /// <summary>
        /// Current owner in the room.
        /// </summary>
        private Player _owner;
        public Player Owner { get => _owner; set => _owner = value; }

        /// <summary>
        /// Max of players in the room.
        /// </summary>
        private readonly uint _maxPlayer;
        public uint MaxPlayer { get => _maxPlayer; }

        /// <summary>
        /// Slot players in the room.
        /// </summary>
        private SlotPlayer[] _slotPlayer;
        public SlotPlayer[] SlotPlayers { get => _slotPlayer; }

        /// <summary>
        /// All players in the room.
        /// </summary>
        private readonly List<Player> _players;

        /// <summary>
        /// Count of the players in the room.
        /// </summary>
        public uint PlayerCnt { get => (uint)_players.Count; }

        /// <summary>
        /// Current stage of the room.
        /// </summary>
        private RoomStage _stage;
        public RoomStage Stage {  get => _stage;  set => _stage = value; }

        // We need to keep it as a weak reference to avoid circular references (Controller -> Room and Room -> Controller)
        private WeakReference<RoomController> _controller;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="index"></param>
        /// <param name="owner"></param>
        /// <param name="mapId"></param>
        /// <param name="maxPlayer"></param>
        internal Room(RoomController controller, uint index, Player owner, uint mapId, uint maxPlayer)
        {
            _index = index;
            _mapId = mapId;
            _maxPlayer = maxPlayer;
            _owner = owner;
            _password = string.Empty;
            _title = string.Format("{0}'s room", owner.Nick);
            _controller = new WeakReference<RoomController>(controller);

            _stage = RoomStage.Waiting;

            _slotPlayer = new SlotPlayer[MAX_NUM_USER_SLOT];
            for(var i = 0; i < MAX_NUM_USER_SLOT; i++)
            {
                _slotPlayer[i] = new SlotPlayer(i, true);
            }

            _players = new List<Player>();
        }

        /// <summary>
        /// Get main informations about of the room in string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[{0}] {1} ({2}) - {3} - {4}/{5}", Index, Title, Owner?.Nick, MapId, PlayerCnt, MaxPlayer);
        }

        /// <summary>
        /// Compare with another room if they are the equals.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is Room room && Equals(room);
        }

        /// <summary>
        /// Get hash code.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            var hashCode = 503727959;
            hashCode = hashCode * -1521134295 + _index.GetHashCode();
            hashCode = hashCode * -1521134295 + _mapId.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Compare with another room if they are the equals.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Room other)
        {
            return _index == other._index &&
                   _mapId == other._mapId;
        }

        /// <summary>
        /// Clone current the room and return room cloned.
        /// </summary>
        /// <returns></returns>
        internal Room Clone()
        {
            _controller.TryGetTarget(out var controller);

            var res = new Room(controller, _index, _owner, _mapId, _maxPlayer)
            {
                _stage = _stage,
                _title = _title,
                _password = _password,
                RoomIndex = RoomIndex,
            };

            res._slotPlayer = (SlotPlayer[])_slotPlayer.Clone();

            res._players.AddRange(_players);

            return res;
        }

        /// <summary>
        /// Join in the current room.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        internal bool Join(Player player)
        {
            lock (_players)
            {
#if DEBUG
                Debug.Assert(!_players.Contains(player), string.Format("Player {0} was already joined on room {1}", player, this));
#endif
                var slot = FindNextFreeSlot();
                if (slot != null)
                {
                    slot._playerIndex = player.Index;
                }
                else
                {
                    CLog.W("Failed to find some slot empty.");
                    return false;
                }

                // Send info about newcome to existing players.
                Broadcast(new CL_PLAYER_JOINED_NFY()
                {
                    roomIndex = _index,
                    player = new PLAYER_INFO()
                    {
                        index = player.Index,
                        nick = player.Nick,
                        level = player.Level,
                        experience = player.Experience,
                        state = player.IsOffline ? PlayerState.Offline : player.Ready ? PlayerState.Ready : PlayerState.NotReady,
                        ping = player.Ping,
                        roomIndex = Index,
                        roomSlotIndex = slot?.SlotIndex ?? -1
                    }
                });

                _players.Add(player);

                // Send info about existing players to newcome, including him self.
                foreach (var existingPlayer in _players)
                {
                    player.Session.Send(new CL_PLAYER_JOINED_NFY()
                    {
                        roomIndex = _index,
                        player = new PLAYER_INFO()
                        {
                            index = existingPlayer.Index,
                            nick = existingPlayer.Nick,
                            level = existingPlayer.Level,
                            experience = existingPlayer.Experience,
                            state = existingPlayer.IsOffline ? PlayerState.Offline : existingPlayer.Ready ? PlayerState.Ready : PlayerState.NotReady,
                            ping = existingPlayer.Ping,
                            roomIndex = Index,
                            roomSlotIndex = FindPlayerSlot(existingPlayer.Index)?.SlotIndex ?? -1,
                        },
                    });
                }
            }

            player.Room = this;
            player.Stage = PlayerStage.Room;
            player.Ready = false;

            // The room has reached the maximum players will no longer be allowed the entry of new players.
            if (PlayerCnt >= MaxPlayer)
            {
                Stage = RoomStage.Full;
            }

            if (_controller.TryGetTarget(out var controller))
            {
                controller.RoomUpdated(this);
            }

#if _DEBUG
            // If the owner is a dummy, transfer ownership to the first player (non-dummy) on room
            if (Owner is Dummy)
            {
                Player firstDummyPlayer = null;
                lock (_players)
                {
                    foreach (var p in _players)
                    {
                        if (p is Dummy)
                            continue;

                        firstDummyPlayer = p;
                        break;
                    }
                }

                if (firstDummyPlayer != null)
                    TransferOwnership(firstDummyPlayer);
            }
#endif

            return true;
        }

        /// <summary>
        /// Leave from current room.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="batchRemoval"></param>
        internal void Leave(Player player, bool batchRemoval = false)
        {
            var slot = FindPlayerSlot(player.Index);
            if(slot != null)
            {
                slot._playerIndex = 0;
            }
            else
            {
                CLog.W("Failed to find player in slot.");
            }

            lock (_players)
            {
                _players.RemoveAll((p) => p.Index == player.Index);
            }

            Broadcast(new CL_PLAYER_LEFT_NFY()
            {
                roomIndex = _index,
                playerIndex = player.Index,
            });

            player.Room = null;
            player.Stage = PlayerStage.Lobby;
            player.Ready = false;

            // If the stage is full and the player left, then update the stage for waiting.
            if (Stage == RoomStage.Full && PlayerCnt < MaxPlayer)
            {
                Stage = RoomStage.Waiting;
            }

            if (!batchRemoval)
            {
                if (!_controller.TryGetTarget(out var controller))
                {
                    return;
                }

                // Check for new owner
                if (player == Owner)
                {
                    Player newOwner = FindFirstPlayer();
                    if (newOwner == null)
                    {
                        // If room will be destroyed, we don't need to broadcast update
                        controller.DestroyRoom(_index);
                    }
                    else
                    {
                        // Transfer Ownership already broadcast update info
                        TransferOwnership(newOwner);
                    }
                }
                else
                {
                    controller.RoomUpdated(this);
                }
            }
        }

        /// <summary>
        /// Force player to leave of the room.
        /// </summary>
        /// <param name="player"></param>
        internal void Kick(Player player)
        {
            var slot = FindPlayerSlot(player.Index);
            if (slot != null)
            {
                slot._playerIndex = 0;
            }
            else
            {
                CLog.W("Failed to find player in slot.");
            }

            // Send packet for all players in the room.
            Broadcast(new CL_ROOM_KICK_PLAYER_NFY()
            {
                roomIndex = _index,
                playerIndex = player.Index,
            });

            // Remove player kicked in the list of the room.
            lock (_players)
            {
                _players.RemoveAll((p) => p.Index == player.Index);
            }

            // Update infos from player kicked.
            player.Room = null;
            player.Stage = PlayerStage.Lobby;
            player.Ready = false;

            // Update info for players on lobby.
            if (!_controller.TryGetTarget(out var controller))
            {
                return;
            }

            controller.RoomUpdated(this);
        }

        /// <summary>
        /// Start match of the room.
        /// </summary>
        /// <param name="roomIndex"></param>
        internal void StartMatch(uint roomIndex, RoomServer server)
        {
            // Update current stage of the players for playing.
            lock (_players)
            {
                _players.ForEach(p => p.Stage = PlayerStage.Playing);
            }

            if (!_controller.TryGetTarget(out var controller))
                return;

            // Update index of the room server.
            RoomIndex = roomIndex;

            // Update current stage of the room.
            Stage = RoomStage.Playing;

            // Send packet for all players in the room with information of the RoomServer.
            Broadcast(new CL_ROOM_START_NFY()
            {
                roomIndex = roomIndex,
                serverInfo = new SERVER_INFO()
                {
                    address = server.IP,
                    port = server.Port,
                }
            });
        }

        /// <summary>
        /// Finish match, remove all players of ingame and change stage.
        /// </summary>
        internal void FinishMatch()
        {
            lock (_players)
            {
                _players.ForEach(p => p.OnFinishMatch());
            }

            // Reset server index.
            RoomIndex = 0;

            // Change stage for waiting.
            Stage = RoomStage.Waiting;
        }

        /// <summary>
        /// Transfer current ownership of the room for another player.
        /// </summary>
        /// <param name="newOwner"></param>
        internal void TransferOwnership(Player newOwner)
        {
            // Update new owner.
            _owner = newOwner;

            if (!_controller.TryGetTarget(out var controller))
            {
                return;
            }

            // Send new update for all players in the room.
            controller.RoomUpdated(this);
        }

        /// <summary>
        /// Change the position of the player in the room.
        /// </summary>
        /// <param name="currentSlot"></param>
        /// <param name="newSlot"></param>
        internal void ChangeSlotPosition(int currentSlot, int newSlot)
        {
            var slot1 = FindSlotByIndex(currentSlot);
            var slot2 = FindSlotByIndex(newSlot);

            // Check if has some player in this slot.
            if (slot2._playerIndex != 0)
            {
                // Keep player index of slot 1.
                var tmpPlayerIndex = slot1._playerIndex;

                // Let to update in slot 1 and put player index of slot 2.
                slot1._playerIndex = slot2._playerIndex;

                // Now to use player index of slot 1.
                slot2._playerIndex = tmpPlayerIndex;

                // Send for all players in the room.
                Broadcast(new CL_ROOM_CHANGE_SLOT_POS_NFY()
                {
                    playerIndex1 = slot2._playerIndex,
                    newSlot1 = slot1.SlotIndex,
                    oldSlot1 = slot2.SlotIndex,

                    playerIndex2 = slot1._playerIndex,
                    newSlot2 = slot2.SlotIndex,
                    oldSlot2 = slot1.SlotIndex,
                });
            }
            else
            {
                // Check if this current slot is available.
                if(!slot2.IsSlotOpen)
                {
                    Owner.Session.Send(new CL_ROOM_CHANGE_SLOT_POS_RES()
                    {
                        error = MessageError.SLOT_CLOSED 
                    });
                }
                else
                {
                    // Slot 2 will receive index of player and Slot 1 will be zero, because this slot don't has any player.
                    slot2._playerIndex = slot1._playerIndex;
                    slot1._playerIndex = 0;

                    // Send for all players in the room.
                    Broadcast(new CL_ROOM_CHANGE_SLOT_SINGLE_POS_NFY()
                    {
                        playerIndex = slot2._playerIndex,
                        newSlot = slot2.SlotIndex,
                        oldSlot = slot1.SlotIndex,
                    });
                }
            }
        }

        /// <summary>
        /// Check that all players are "ready" to start the game
        /// </summary>
        /// <returns></returns>
        internal bool AllReady()
        {
            lock (_players)
            {
                foreach (var player in _players)
                {
                    // Owner will always by not ready. Because when he is ready, the match will start
                    if (!player.Ready && _owner != player) 
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check if the player is the owner of the room
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        internal bool IsOwner(Player player)
        {
            return _owner == player;
        }

        /// <summary>
        /// Check if the player is the owner of the room
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <returns></returns>
        internal bool IsOwner(ulong playerIndex)
        {
            return _owner.Index == playerIndex;
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
                if (slot?._playerIndex == playerIndex)
                    return slot;
            }

            return null;
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
                if (slot?.SlotIndex == index)
                    return slot;
            }

            return null;
        }

        /// <summary>
        /// Find an available slot.
        /// </summary>
        /// <returns></returns>
        private SlotPlayer FindNextFreeSlot()
        {
            foreach (var slot in _slotPlayer)
            {
                if (slot == null)
                    continue;

                // Check if this slot is openned.
                if (!slot.IsSlotOpen)
                    continue;

                // If playerIndex is zero, then slot is available.
                if (slot._playerIndex == 0)
                    return slot;
            }

            return null;
        }

        /// <summary>
        /// Find first player in list.
        /// </summary>
        /// <returns></returns>
        private Player FindFirstPlayer()
        {
            lock (_players)
            {
                return _players.Count > 0 ? _players[0] : null;
            }
        }

        /// <summary>
        /// Send packet for all players in the room.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="msg"></param>
        internal void Broadcast<T>(T msg) where T : IMessage
        {
            lock (_players)
            {
                foreach (var player in _players)
                {
                    // If player is offline, let to ignore.
                    if (player.IsOffline)
                        continue;

                    // Send packet for player.
                    player.Session.Send(msg);
                }
            }
        }

        /// <summary>
        /// Check if all slots have already been occupied.
        /// </summary>
        /// <returns></returns>
        internal bool IsFull()
        {
            lock (_players)
            {
                return _players.Count >= _maxPlayer /*&& FindNextFreeSlot() == null*/;
            }
        }

        /// <summary>
        /// Check if current room has password.
        /// If yes, check if the password matches the password of the room.
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        internal bool CheckPassword(string password)
        {
            // If password in the room is empty, then let to ignore.
            if (string.IsNullOrEmpty(Password))
            {
                return true;
            }
            // If the room has password, let to check if password sended by player to match with password of the room.
            else if (Password == password)
            {
                return true;
            }

            // If this case got here, then has some cases happened, like wrong password or other something.
            return false;
        }

        /// <summary>
        /// Send packet to all players in the room with current ping from determined player index.
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <param name="ping"></param>
        internal void PingNfy(ulong playerIndex, long ping)
        {
            lock (_players)
            {
                foreach (var player in _players)
                {
                    // Check if player is offline.
                    if (player.IsOffline /*|| player.Index == playerIndex*/)
                        continue;

                    player.Session.Send(new CL_ROOM_HEARTBEAT_NFY()
                    {
                        playerIdx = playerIndex,
                        ping = ping,
                        roomIdx = Index
                    });
                }
            }
        }

        /// <summary>
        /// Get all players in the room.
        /// </summary>
        /// <returns></returns>
        internal List<Player> ListAllPlayers()
        {
            var res = new List<Player>();

            lock (_players)
            {
                res.AddRange(_players);
            }

            return res;
        }

        /// <summary>
        /// Get all slots players and return in list.
        /// </summary>
        /// <returns></returns>
        internal List<SLOT_PLAYER> ListAllSlots()
        {
            var res = new List<SLOT_PLAYER>();

            foreach (var slot in SlotPlayers)
            {
                SLOT_PLAYER s = new SLOT_PLAYER
                {
                    slotIndex = slot.SlotIndex,
                    playerIndex = slot._playerIndex,
                };

                res.Add(s);
            }

            return res;
        }
    }
}
