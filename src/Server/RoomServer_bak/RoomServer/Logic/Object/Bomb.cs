using CommonLib.GridEngine;
using CommonLib.Util.Math;
using CommonLib.Util;
using RoomServer.Logic.PowerUP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace RoomServer.Logic.Object
{
    public class BombAttributes : BaseObjectAttributes
    {
        public uint area;
        public float timeout;
        public uint moveSpeed;
    }

    public class Bomb : HitableObject
    {
        public const uint DEFAULT_AREA = 3u;

        private static int nextUID = 0;

        private static int NextUID()
        {
            return Interlocked.Increment(ref nextUID);
        }

        private static uint GenUniqueUID()
        {
            return (uint)("Bomb " + NextUID()).GetHashCode();
        }

        // Delay time for the bomb to explode.
        protected float _timeout;
        public float Timeout { get => _timeout; set => _timeout = value; }

        // If bomb needs to be exploded forced.
        protected bool _exploded;

        // If the bomb was kicked.
        protected bool _kicked;
        protected float _timeKick;
        protected GridDir _kickDirection;

        public List<Vec2> ExplosionArea = new List<Vec2>();
        public List<Vec2> ExplosionObject = new List<Vec2>();

        protected uint _owner;
        public uint Owner { get => _owner; }

        protected BombAttributes _attr;
        public BombAttributes Attr { get => _attr; }

        protected override BaseObjectAttributes BaseAttr
        {
            get
            {
                return _attr;
            }
        }

        public uint Area
        {
            get
            {
                return _attr.area;
            }
        }

        private readonly List<GridCell> _brokenCells;

        public Bomb(GridMap map, GridObject owner) : base(GenUniqueUID(), ObjectType.BOMB, false, map)
        {
            _exploded = false;

            _kicked = false;
            _timeKick = 0.0f;

            _brokenCells = new List<GridCell>();

            var bombArea = DEFAULT_AREA;

            if (owner is Player player)
            {
                bombArea = player.Attr.bombArea;
            }

            //TODO: Load those configs from DB.
            _attr = new BombAttributes
            {
                lifePoints = 1,
                defensePoints = 0,
                attackPoints = 1,
                area = bombArea,
                timeout = 2.5f,
                moveSpeed = 8,
            };

            _owner = owner?.UID ?? 0;
            _timeout = _attr.timeout;
        }

        private Vec2f GetDir()
        {
            switch(_kickDirection)
            {
                case GridDir.UP:    return new Vec2f(0, 1f);
                case GridDir.RIGHT: return new Vec2f(1, 0);
                case GridDir.DOWN:  return new Vec2f(0, -1);
                case GridDir.LEFT:  return new Vec2f(-1, 0);
                default:            return new Vec2f(0, 0);
            }
        }

        public override void Tick(float delta)
        {
            base.Tick(delta);

            _timeout -= delta;

            if (_kicked && _timeout > 0.3f)
            {
                var moveForce = GetDir() * _attr.moveSpeed * delta;

                Debug.Assert(moveForce.x == 0 || moveForce.y == 0);

                Move(moveForce.x, moveForce.y);
            }

            // Explode bomb!
            if (_timeout <= 0)
            {
                Die();
            }
        }

        private long _lastSync;
        private const long BOMB_SYNC_FREQ = TimeSpan.TicksPerMillisecond * 33; //33 = 30 fps
        public override void Move(float x, float y)
        {
            var b4 = WorldPos;

            base.Move(x, y);

            //If the bomb move and there is at least 500ms sinc our last pos sync, let's sync again.
            if (b4 != WorldPos && DateTime.Now.Ticks - _lastSync > BOMB_SYNC_FREQ)
            {
                SyncPosNow();
            }
        }

        protected void SyncPosNow()
        {
            _lastSync = DateTime.Now.Ticks;
            var room = _map as Room;
            room.BroadcastWorldBombPos(this);
        }

        public void Kick(GridObject kicker, GridDir dir)
        {
            //Can't kick an already kicked bomb.
            if (_kicked)
                return;

            var distance = new Vec2i(kicker.GridPos.x, kicker.GridPos.y) - new Vec2i(GridPos.x, GridPos.y);
            if (Math.Abs(distance.x) > 1 || Math.Abs(distance.y) > 1)
            {
                CLog.W("Object {0} trying to kick a bomb from high distance {1}.", kicker, distance);
                return;
            }

            // Bomb was kicked and define in which direction the bobm was kicked.
            _kicked = true;
            _kickDirection = dir;
        }

        private void Explode()
        {
            if (_exploded)
                return;

            //Ensure we gonna blow on the right place
            if (_kicked)
                SyncPosNow();

            _exploded = true;
            var area = _attr.area;

            Random random = new Random((int)DateTime.Now.Ticks);

            foreach (var dir in Vec2.ALL_DIRS)
            {
                for (int i = 0; i < area; i++)
                {
                    var pos = _gridPos + (dir * i);

                    var cell = _map[pos.x, pos.y];

                    if (cell.Type == CellType.None)
                    {
                        ExplosionArea.Add(pos);
                    }
                    else if (cell.HasAttribute(CellAttributes.BREAKABLE))
                    {
                        // Inform position from cell for player.
                        ExplosionObject.Add(pos);
                        _brokenCells.Add(cell);

                        break;
                    }
                    else
                    {
                        break;
                    }

                    // Check if has some bomb in this current position on grid, if yes, we need to explode this bomb too.
                    var objs = cell.FindAllByType<HitableObject>();
                    if (objs != null && objs.Count > 0)
                    {
                        foreach (var obj in objs)
                        {
                            var hittable = obj as HitableObject;
                            if (!hittable.Hit(this))
                            {
                                CLog.D("Cant hit object: {0}", obj);
                            }

                            if (hittable is Bomb bomb)
                            {
                                bomb.Explode();
                            }
                        }
                    }
                }
            }

            LeaveMap();
        }

        protected override void OnDead(HitableObject killer)
        {
            Explode();

            foreach (var cell in _brokenCells)
            {
                PowerUpManager.RandomInstanciate(_map as Room, cell.Pos);
                cell.UpdateCell(CellType.None);
            }

            var owner = _map.FindObject(_owner);

            if (owner is Player player)
            {
                player.ReloadBomb();
            }
            else
            {
                Debug.Assert(owner is Player, "Only players can own bombs!");
            }
        }

        protected override bool OnHit(HitableObject hitter) { return true; }

        protected override bool CanHit(HitableObject hitter)
        {
            return !_exploded;
        }
    }
}
