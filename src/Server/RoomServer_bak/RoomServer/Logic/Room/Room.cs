using CommonLib.GridEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using CommonLib.Messaging.Common;
using RoomServer.Logic.Object;
using CommonLib.Util;
using CommonLib.Messaging.Base;
using CommonLib.Messaging.Client;
using CommonLib.Server;
using CommonLib.Messaging;
using CommonLib.Util.Math;
using System.Diagnostics;
using RoomServer.Logic.Behaviour;

namespace RoomServer.Logic
{
    public enum RoomStage
    {
        Waiting,
        Playing,
        Finished
    }

    struct MatchEndInfo
    {
        //TODO: Add more match end info, like number of players killed, time of death, etc
        public uint winner;
    }

    class SessionMessage : RawMessage
    {
        public readonly uint uid;
        public SessionMessage(uint sessionUID, byte[] buffer) : base(buffer)
        {
            uid = sessionUID;
        }
        public SessionMessage(uint sessionUID, RawMessage raw) : base(raw)
        {
            uid = sessionUID;
        }
    }

    internal class Room : GridMap, ITickable
    {
        public static readonly int TICKS_PER_SECOND = 64;
        private static readonly float MAP_CELL_SIZE = 1f;

        private uint _ownerId;
        public uint Owner
        {
            get
            {
                return _ownerId;
            }
            set
            {
                _ownerId = value;
            }
        }

        private string _ownerLogin;
        public string OwnerLogin
        {
            get
            {
                return _ownerLogin;
            }
            set
            {
                _ownerLogin = value;
            }
        }

        private bool _initialized;
        public bool IsInitialized
        {
            get
            {
                return _initialized;
            }
        }

        public string Name
        {
            get
            {
                return "Room" + _uid;
            }
        }

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

        private SLOT_PLAYER[] _slotPlayers;
        public SLOT_PLAYER[] SlotPlayers
        {
            get
            {
                return _slotPlayers;
            }
        }

        public RoomStage Stage { get; private set; }

        private readonly Queue<SessionMessage> _messageQueue;
        private readonly RoomManager _roomManager;

        private readonly BehaviourController<RoomBehaviour> _behaviourController;
        private bool _shutdownRequested;

        /// <summary>
        /// This property is used to check how many players is expected to connect on the room. Only when this expected quantity is met the room is initialized.
        /// </summary>
        public int InitialPlayerCount { get; set; }

        internal T GetOwner<T>() where T : GridObject
        {
            return ActiveObjects.Where(o => o is T t && t.UID == Owner) as T;
        }

        private uint _botCount;
        public uint BotCount
        {
            get => _botCount;
            set => _botCount = (uint)Math.Min(value, MaxPlayer);
        }

        public BehaviourController<RoomBehaviour> Behaviour
        {
            get
            {
                return _behaviourController;
            }
        }

        public Room(RoomManager manager, int width, int height, uint uid, int botCount = 0) : base(MAP_CELL_SIZE, width, height, uid)
        {
            _messageQueue = new Queue<SessionMessage>();
            _roomManager = manager;
            _behaviourController = new BehaviourController<RoomBehaviour>(this);
            _shutdownRequested = false;

            OnObjectAdded += NotifyObjectAdded;
            OnObjectRemoved += NotifyObjectRemoved;

            Stage = RoomStage.Waiting;
        }

        internal void InitSlotPlayers(List<SLOT_PLAYER> slotsPlayers)
        {
            _slotPlayers = new SLOT_PLAYER[slotsPlayers.Count];
            for (var i = 0; i < slotsPlayers.Count; i++)
            {
                var slot = slotsPlayers[i];
                if (slot == null)
                    continue;

                _slotPlayers[i] = slot;
            }
        }

        internal MAP_INFO GetMapInfo()
        {
            MAP_INFO result = null;

            var spawnCnt = MaxPlayer;
            var data = new byte[_cells.Length + (spawnCnt * 2) + 1];

            result = new MAP_INFO
            {
                width = _mapSize.x,
                height = _mapSize.y,
                playerCnt = (ushort)spawnCnt,
                background = _roomManager.GetBackground(MapId),
                data = data,
                index = _uid
            };

            int i = 0;

            data[i++] = (byte)spawnCnt;

            foreach (var v in SpawnPlaces)
            {
                data[i++] = (byte)v.x;
                data[i++] = (byte)v.y;
            }

            foreach (var cell in _cells)
            {
                data[i++] = cell.Serialize();
            }

            return result;
        }

        internal List<Tuple<int, int>> GetMapTypeList()
        {
            var result = new List<Tuple<int, int>>();
            var typesDict = GridCell.TypesAttr;

            foreach (var pair in typesDict)
            {
                result.Add(new Tuple<int, int>((int)pair.Key, (int)pair.Value));
            }

            return result;
        }


        public int ActiveObjectsCount { get; private set; }
        public int ActivePlayersCount { get; private set; }

        internal List<GridObject> GetActiveObjects()
        {
            var result = new List<GridObject>(ActiveObjects.Count);
            result.AddRange(ActiveObjects);
            return result;
        }

        internal void ListActiveObjectsAsync(Action<List<GridObject>> callback)
        {
            GetActiveObjectsAsync((objs) => callback(objs));
        }

        public Vec2 FindRandFreeGrid()
        {
            var rand = new Random((int)DateTime.Now.Ticks);

            Vec2 res = Vec2.INVALID;

            int tryCount = 20;
            GridCell cell;
            do
            {
                if (tryCount-- <= 0)
                {
                    res = Vec2.INVALID;
                    break;
                }

                res = new Vec2(rand.Next(0, MapSize.x), rand.Next(0, MapSize.y));
                cell = this[res.x, res.y];
            } while (cell != null && !cell.IsEmpty());

            return res;
        }

        public override string ToString()
        {
            return string.Format("[uid: {0}, owner: {1}, width: {2}, height: {3}]", _uid, _ownerId, _mapSize.x, _mapSize.y);
        }

        private void NotifyObjectAdded(GridObject obj)
        {
            var type = obj.Type;

            switch (type)
            {
                case ObjectType.PLAYER:
                    {
                        PlayerJoined(obj as Player);
                    }
                    break;
                case ObjectType.BOMB:
                    {
                        BroadcastMessage(new CR_BOMB_PLACED_NFY()
                        {
                            uid = obj.UID,
                            gridX = obj.GridPos.x,
                            gridY = obj.GridPos.y,
                            moveSpeed = (obj as Bomb).Attr.moveSpeed,
                        });
                    }
                    break;
                case ObjectType.POWERUP:
                    {
                        BroadcastMessage(new CR_POWERUP_ADD_NFY()
                        {
                            uid = obj.UID,
                            icon = (obj as PowerUp).Icon,
                            cell = new VEC2()
                            {
                                x = obj.GridPos.x,
                                y = obj.GridPos.y
                            },
                        });
                    }
                    break;
                default:
                    CLog.E("Invalid object type entered on map: " + type);
                    break;
            }
        }

        private void PlayerJoined(Player player)
        {
            if (player.Session.Login == _ownerLogin)
                _ownerId = player.UID;

            SendInitialInfo(player);
            NotifyPlayerJoined(player);

            // A room is initialized only when the owner has entered. Only after that, the others can join.
            if (!_initialized && Count(ObjectType.PLAYER) == InitialPlayerCount)
            {
                _initialized = true;
                OnInitialize();
            }
        }

        private void NotifyPlayerJoined(Player newPlayer)
        {
            var players = ActiveObjects.Where(o => o.Type == ObjectType.PLAYER);
            foreach (var obj in players)
            {
                if (newPlayer.UID == obj.UID)
                    continue;

                var player = obj as Player;
                player.Session?.Send(new CR_PLAYER_ENTER_NFY()
                {
                    info = new ROOM_PLAYER_INFO()
                    {
                        uid = newPlayer.UID,
                        gridPos = new VEC2()
                        {
                            x = newPlayer.GridPos.x,
                            y = newPlayer.GridPos.y
                        },
                        nick = player.Info.nick,
                        gender = player.Info.gender,
                        alive = newPlayer.IsLive,
                        attr = new PLAYER_ATTRIBUTES()
                        {
                            // We need to send only common attributes to other players.
                            common = new COMMON_PLAYER_ATTRIBUTES()
                            {
                                moveSpeed = newPlayer.Attr.moveSpeed,
                            },
                        },
                    }
                });
            }
        }

        internal int Count(ObjectType type)
        {
            return ActiveObjects.Where(o => o.Type == type).Count();
        }

        internal void SendInitialInfo(Player player)
        {
            // Send player that are already on map.
            var players = ActiveObjects.Where(o => o.Type == ObjectType.PLAYER);
            foreach (var otherObj in players)
            {
                var otherPlayer = otherObj as Player;

                player.Session?.Send(new CR_PLAYER_ENTER_NFY()
                {
                    info = new ROOM_PLAYER_INFO()
                    {
                        uid = otherPlayer.UID,
                        gridPos = new VEC2()
                        {
                            x = otherPlayer.GridPos.x,
                            y = otherPlayer.GridPos.y
                        },
                        alive = otherPlayer.IsLive,
                        nick = otherPlayer.Info.nick,
                        gender = otherPlayer.Info.gender,
                        attr = new PLAYER_ATTRIBUTES()
                        {
                            lifePoints = player.Attr.lifePoints,
                            attackPoints = player.Attr.attackPoints,
                            defensePoints = player.Attr.defensePoints,
                            bombCount = player.Attr.bombCount,
                            bombArea = player.Attr.bombArea,
                            immunityTime = player.Attr.immunityTime,
                            kickBomb = player.Attr.kickBomb,

                            common = new COMMON_PLAYER_ATTRIBUTES()
                            {
                                moveSpeed = player.Attr.moveSpeed,
                            },
                        }
                    }
                });
            }

            // Send bombs that are already on map.
            var bombs = ActiveObjects.Where(o => o.Type == ObjectType.BOMB);
            foreach (var bomb in bombs)
            {
                var b = bomb as Bomb;
                player.Session?.Send(new CR_BOMB_PLACED_NFY()
                {
                    uid = bomb.UID,
                    gridX = bomb.GridPos.x,
                    gridY = bomb.GridPos.y,
                    moveSpeed = b.Attr.moveSpeed,
                });
            }

            //TODO: Add sent to other objects.
        }

        private void OnInitialize()
        {
            Stage = RoomStage.Playing;

            for (var i = 0; i < BotCount; i++)
            {
                var dummy = new Dummy(_roomManager.App, this);

                if (!AllocSlot(dummy))
                {
                    CLog.W("The room is full!");
                    return;
                }

                var spawn = GetSpawnPos(dummy);
                dummy.Wrap(GridToWorld(spawn));
                dummy.EnterMap();
            }
        }

        private void NotifyObjectRemoved(GridObject obj)
        {
            var type = obj.Type;

            switch (type)
            {
                case ObjectType.PLAYER:
                    {
                        PlayerLeft(obj as Player);
                    }
                    break;
                case ObjectType.BOMB:
                    {
                        BombExploded(obj as Bomb);
                    }
                    break;
                case ObjectType.POWERUP:
                    {
                        BroadcastMessage(new CR_POWERUP_REMOVE_NFY()
                        {
                            uid = obj.UID,
                            collected = (obj as PowerUp).Collected,
                        });
                    }
                    break;
                default:
                    CLog.E("Invalid object type leave on map: " + type);
                    break;
            }
        }

        private void PlayerLeft(Player player)
        {
            BroadcastMessage(new CR_PLAYER_LEAVE_NFY()
            {
                uid = player.UID
            });

            if (player.UID == Owner)
            {
                var nextOwner = FindFirstByType(ObjectType.PLAYER);

                if (nextOwner == null)
                {
                    Shutdown();
                }
                else
                {
                    Owner = nextOwner.UID;
                    //NotifyNewOwner();
                }
            }
        }

        public void RequestShutdown()
        {
            _shutdownRequested = true;
        }

        public override void Shutdown()
        {
            base.Shutdown();

            var tmp = new GridObject[ActiveObjects.Count];
            ActiveObjects.CopyTo(tmp);

            foreach (var obj in tmp)
            {
                obj.LeaveMap();
            }

            _behaviourController.Destroy();
            _messageQueue.Clear();
            _roomManager.DestroyRoom(this);
        }

        private void BombExploded(Bomb bomb)
        {
            var area = new List<VEC2>(bomb.ExplosionArea.Count);

            foreach (var pos in bomb.ExplosionArea)
            {
                area.Add(new VEC2()
                {
                    x = pos.x,
                    y = pos.y
                });
            }

            BroadcastMessage(new CR_BOMB_EXPLODED_NFY()
            {
                uid = bomb.UID,
                area = area
            });

            // Object explosion's.
            if (bomb.ExplosionObject.Count > 0)
            {
                var objectExplosion = new List<VEC2>(bomb.ExplosionObject.Count);
                foreach (var pos in bomb.ExplosionObject)
                {
                    objectExplosion.Add(new VEC2()
                    {
                        x = pos.x,
                        y = pos.y
                    });
                }

                BroadcastMessage(new CR_BOMB_EXPLODED_OBJECT_NFY()
                {
                    area = objectExplosion
                });
            }
        }

        public void BroadcastMessage<T>(T message) where T : IMessage
        {
            SendMessageTo(ActiveObjects, message);
        }

        public void BroadcastMessageAsync<T>(T message) where T : IMessage
        {
            GetActiveObjectsAsync((objs) =>
            {
                SendMessageTo(objs, message);
            });
        }

        public void SendMessageTo<T>(List<GridObject> destList, T message) where T : IMessage
        {
            foreach (var activeObj in destList)
            {
                if (activeObj.Type == (int)ObjectType.PLAYER)
                {
                    var player = activeObj as Player;

                    if (!player.Offline)
                        player.Session?.Send(message);
                }
            }
        }

        public void AddMessage(SessionMessage message)
        {
            lock (_messageQueue)
            {
                _messageQueue.Enqueue(message);
            }
        }

#if _DEBUG
        private volatile bool _runningTick = false;
#endif
        public override void Tick(float delta)
        {
            if (_shutdownRequested)
            {
                Shutdown();
                _shutdownRequested = false;
                return;
            }

#if _DEBUG
            Debug.Assert(_runningTick == false);
            _runningTick = true;
#endif

            base.Tick(delta);

            ProcessMessages();
            _behaviourController.Tick(delta);

#if _DEBUG
            _runningTick = false;
#endif
            ActiveObjectsCount = ActiveObjects.Count;
            ActivePlayersCount = ActiveObjects.Where(o => o != null && o.Type == ObjectType.PLAYER).Count();
        }

        private void ProcessMessages()
        {
            if (!_initialized)
                return;

            var messages = new List<SessionMessage>();

            lock (_messageQueue)
            {
                while (_messageQueue.Count > 0)
                {
                    messages.Add(_messageQueue.Dequeue());
                }
            }

            if (messages.Count == 0)
                return;

            foreach (var msg in messages)
            {
                Process(msg);
            }
        }

        private void Process(SessionMessage msg)
        {
            var sender = FindObject(msg.uid);

            switch (msg.MsgType)
            {
                case MessageType.CR_PLAYER_MOVE_SYNC_NFY:
                    {
                        PlayerMoveSyncNfy(msg.To<CR_PLAYER_MOVE_SYNC_NFY>(), sender);
                    }
                    break;
                case MessageType.CR_PLACE_BOMB_REQ:
                    {
                        PlaceBombReq(msg.To<CR_PLACE_BOMB_REQ>(), sender);
                    }
                    break;
                case MessageType.CR_BOMB_KICK_REQ:
                    {
                        KickBombReq(msg.To<CR_BOMB_KICK_REQ>(), sender);
                    }
                    break;
                default:
                    CLog.W("Unrecognized message received on room {0}: {1}", _uid, msg.MsgType);
                    break;
            }
        }

        private void KickBombReq(CR_BOMB_KICK_REQ req, GridObject sender)
        {
            if (FindObject(req.uid) is Player player && player.Attr.kickBomb)
            {
                if (FindObject(req.uidBomb) is Bomb bomb)
                {
                    bomb.Kick(player, (GridDir)req.dir);
                }
            }
        }

        private void PlaceBombReq(CR_PLACE_BOMB_REQ req, GridObject sender)
        {
            //TODO: Add check.
            var gridPos = sender.GridPos;

            // Check if already has another bomb in same position on grid.
            if (sender.Map[gridPos.x, gridPos.y].FindFirstByType(ObjectType.BOMB) != null)
                return;

            if (sender is Player player)
            {
                player.PlaceBomb();
            }
        }

        private void PlayerMoveSyncNfy(CR_PLAYER_MOVE_SYNC_NFY nfy, GridObject sender)
        {
            var previousPos = sender.WorldPos;
            sender.Move(nfy.moveX, nfy.moveY);

            if (previousPos == sender.WorldPos)
            {
                CLog.W("Can't move obj {0} [{1}, {2}] units", sender.UID, nfy.moveX, nfy.moveY);

                // Notify sender to reset its position to server one.
                SendMessageTo(new List<GridObject>() { sender }, new CR_PLAYER_POS_NFY()
                {
                    uid = sender.UID,
                    worldPos = new VEC2()
                    {
                        x = sender.WorldPos.x,
                        y = sender.WorldPos.y
                    },
                });
            }
            else
            {
                // Since floating point calculation can vary depending on hardware, we need to keep this check to sync position across clients.
                var senderOriginalPos = new Vec2f(nfy.currentWorldPos.x, nfy.currentWorldPos.y);
                var distance = senderOriginalPos - sender.WorldPos;
                if (distance.Magnitude() > 0.5)
                {
                    // The distance is greater than minimum threshold, let's notify client to fix it's position.
                    SendMessageTo(new List<GridObject>() { sender }, new CR_PLAYER_POS_NFY()
                    {
                        uid = sender.UID,
                        worldPos = new VEC2()
                        {
                            x = sender.WorldPos.x,
                            y = sender.WorldPos.y
                        },
                    });
                }

                BroadcastWorldPos(sender);
            }
        }

        public void BroadcastWorldPos(GridObject obj)
        {
            var others = ActiveObjects.Where(o => o.UID != obj.UID).ToList();

            SendMessageTo(others, new CR_PLAYER_POS_NFY()
            {
                uid = obj.UID,
                worldPos = new VEC2()
                {
                    x = obj.WorldPos.x,
                    y = obj.WorldPos.y
                },
            });
        }

        public void BroadcastWorldBombPos(GridObject obj)
        {
            var others = ActiveObjects.Where(o => o.UID != obj.UID).ToList();

            SendMessageTo(others, new CR_BOMB_POS_NFY()
            {
                uid = obj.UID,
                worldPos = new VEC2()
                {
                    x = obj.WorldPos.x,
                    y = obj.WorldPos.y
                },
            });
        }

        internal void EndMatch(MatchEndInfo matchEndInfo)
        {
            Stage = RoomStage.Finished;

            BroadcastMessage(new CR_MATCH_END_NFY()
            {
                winner = matchEndInfo.winner
            });

            _roomManager.MatchEnded(this, matchEndInfo);

            RequestShutdown();
        }

        internal int GetPlayerSlot(ulong playerIndex)
        {
            foreach (var slot in _slotPlayers)
            {
                if (slot.playerIndex == playerIndex)
                    return slot.slotIndex;
            }

            return -1;
        }
    }
}
