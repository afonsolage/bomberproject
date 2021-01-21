using CommonLib.GridEngine;
using CommonLib.Util.Math;
using CommonLib.Messaging.Client;
using CommonLib.Util;
using RoomServer.Logic.AI;
using RoomServer.Logic.AI.Behaviour;
using RoomServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomServer.Logic.Object
{
    class Player : HitableObject
    {
        protected PlayerAttributes _attr;
        protected override BaseObjectAttributes BaseAttr
        {
            get
            {
                return _attr;
            }
        }
        public PlayerAttributes Attr
        {
            get
            {
                return _attr;
            }
        }

        protected static uint GenUniqueUID(uint sessionID)
        {
            return (uint)("Player " + sessionID).GetHashCode();
        }

        protected ClientSession _session;

        public long SessionDestroyedTick { get; private set; }

        public virtual string Login { get; protected set; }
        public ulong DBID { get; private set; }

        public PlayerInfo Info => Session?.PlayerInfo ?? default(PlayerInfo);

        public bool Offline { get => _session == null; }
        public bool IsAIControlled { get => _behaviour != null; }

        protected uint _canSkipBomb = uint.MaxValue;

        public ClientSession Session
        {
            get
            {
                return _session;
            }
        }

        protected float _immunityTime;
        protected PlayerBehaviour _behaviour;

        public Player(ClientSession session, GridMap map) : base(GenUniqueUID(session.ID), (int)ObjectType.PLAYER, true, map)
        {
            _session = session;
            Login = _session.Login;
            DBID = _session.DBID;

            _attr = new PlayerAttributes()
            {
                //TODO: Load those configs from DB.
                attackPoints = 0,
                defensePoints = 0,
                lifePoints = 1000000u, //(this is Dummy) ? 1u : 1000000u,
                bombCount = 20,
                immunityTime = 2,
                moveSpeed = 5u,
                bombArea = 3,
                kickBomb = false,
            };
        }

        public bool CanPassThrough(Vec2 pos)
        {
            return CanPassThrough(_map[pos.x, pos.y]);
        }

        public bool CanPassThrough(GridCell cell, bool ignoreSkipBomb = false)
        {
            return Surroundings.CanPassThrough(cell, o =>
            {
                if (!ignoreSkipBomb && o is Bomb b && b.IsLive && b.UID == _canSkipBomb) //If this is a bomb and we can skip it, we should not collide with it
                    return false;
                else
                    return WALL_TYPES.Contains(o.Type); //Else just check if it is a collider type
            });
        }

        protected bool DoMove(float x, float y)
        {
            if (!_live)
                return false;

            var gridB4 = GridPos;

            base.Move(x, y);

            if (gridB4 != GridPos)
            {
                _canSkipBomb = uint.MaxValue;
            }

            return true;
        }

        public override void Move(float x, float y)
        {
            var b4 = WorldPos;
            
            DoMove(x, y);

            if (b4 != WorldPos && IsAIControlled)
            {
                var room = _map as Room;
                room.BroadcastWorldPos(this);
                _lazyWorldPos = WorldPos;
                _lastLazySync = long.MaxValue;
            }
        }

        protected Vec2f _lazyWorldPos;
        protected long _lastLazySync;

        public bool LazyMove(float x, float y)
        {
            var b4 = WorldPos;

            DoMove(x, y);

            if (b4 != WorldPos && IsAIControlled)
            {
                _lastLazySync = DateTime.Now.Ticks;
            }

            return b4 != WorldPos;
        }

        protected void CheckLazyMove()
        {
            if (_lazyWorldPos == _worldPos)
                return;

            var dist = _worldPos - _lazyWorldPos;

            if (dist.Magnitude() > _map.CellSize / 8f  //If the accumulated walk is higher than 1/8 of a cell size
                || _lastLazySync + (TimeSpan.TicksPerSecond / 2) < DateTime.Now.Ticks) //Or 500ms has elapsed since the last _lazySync;
            {
                (_map as Room).BroadcastWorldPos(this);
                _lazyWorldPos = _worldPos;
                _lastLazySync = long.MaxValue;
            }
        }

        public override void Tick(float delta)
        {
            base.Tick(delta);

            if (_immunityTime > 0)
            {
                _immunityTime -= delta;
            }

            _behaviour?.Tick(delta);

            CheckLazyMove();
        }

        protected override void OnDead(HitableObject killer)
        {
            var room = _map as Room;

            room.BroadcastMessage(new CR_PLAYER_DIED_NFY()
            {
                uid = _uid,
                killer = killer?.UID ?? 0
            });
        }

        protected override bool CanHit(HitableObject hitter)
        {
            return _immunityTime <= 0;
        }

        public void SetImmune(float seconds)
        {
            _immunityTime = seconds;

            var room = _map as Room;
            room.BroadcastMessage(new CR_IMMUNITY_NFY()
            {
                uid = _uid,
                duration = seconds,
            });
        }

        internal virtual void Reconnect(ClientSession session)
        {
            _session = session;
            _session.Player = this;
            SessionDestroyedTick = 0;

            _behaviour = null;

            var room = _map as Room;

            //Send current room info
            session.Send(new CR_JOIN_ROOM_NFY()
            {
                info = room.GetMapInfo(),
                typeList = room.GetMapTypeList(),
            });

            room.SendInitialInfo(this);

            //Tell others this player is online
            room.BroadcastMessage(new CX_PLAYER_ONLINE_NFY()
            {
                index = _uid,
            });

            //TODO: Send state packages, like if the player is on some room or if it was on some UI like shop.
        }

        internal virtual void DestroySessionOnly()
        {
            _session.Player = null;
            _session = null;
            SessionDestroyedTick = DateTime.Now.Ticks;

            var room = _map as Room;
            room.BroadcastMessageAsync(new CX_PLAYER_OFFLINE_NFY()
            {
                index = _uid,
            });

            _behaviour = new PlayerBehaviour(this);
        }

        protected override bool OnHit(HitableObject hitter)
        {
            var room = _map as Room;
            room.BroadcastMessage(new CR_PLAYER_HIT_NFY()
            {
                uid = _uid,
                hitter = hitter.UID
            });

            SetImmune(_attr.immunityTime);

            return true;
        }

        internal bool PlaceBomb()
        {
            // Check if player is still alive.
            if (!_live)
                return false;

            // Check if player can to place bomb.
            if (_attr.bombCount == 0)
                return false;

            // Check if current grid position don't already has some bomb.
            if (Map[GridPos.x, GridPos.y].FindFirstByType(ObjectType.BOMB) != null)
                return false;

            // Initialize bomb.
            var bomb = new Bomb(_map, this);
            bomb.Wrap(_map.GridToWorld(GridPos));
            bomb.EnterMap();

            _attr.bombCount--;
            _canSkipBomb = bomb.UID;

            return true;
        }

        internal void ReloadBomb()
        {
            _attr.bombCount++;
        }

        internal void ChangeMoveSpeed(int value)
        {
            var newSpeed = _attr.moveSpeed + value;
            if (newSpeed <= 0)
                newSpeed = 1;

            _attr.moveSpeed = (uint)newSpeed;

            var room = _map as Room;
            room.BroadcastMessage(new CR_SPEED_CHANGE_NFY()
            {
                uid = _uid,
                speed = _attr.moveSpeed,
            });
        }
    }
}
