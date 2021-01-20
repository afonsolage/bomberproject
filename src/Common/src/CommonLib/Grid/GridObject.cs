using CommonLib.Util.Math;
using CommonLib.Util;
using System.Collections.Generic;
using System;
using System.Diagnostics;

namespace CommonLib.GridEngine
{
    public abstract class GridObjectPool<T> where T : GridObject
    {
        protected readonly Queue<T> _pool;
        protected uint _nextUid;

        public GridObjectPool()
        {
            _pool = new Queue<T>();
            _nextUid = 1;
        }

        public T Get()
        {
            lock (this)
            {
                if (_pool.Count == 0)
                {
                    return CreateObject();
                }
                else
                {
                    var g = _pool.Dequeue();
                    g.Reset();
                    return g;
                }
            }
        }

        public void Release(T obj)
        {
            lock (this)
            {
                _pool.Enqueue(obj);
            }
        }

        protected abstract T CreateObject();

        protected uint NextUID()
        {
            lock (this)
            {
                return _nextUid++;
            }
        }
    }

    public class GridObjectSurrounding
    {
        public readonly GridCell[] left;
        public readonly GridCell[] right;
        public readonly GridCell[] bottom;
        public readonly GridCell[] up;

        private readonly GridObject _obj;

        public GridObjectSurrounding(GridObject obj)
        {
            Debug.Assert(obj != null);

            /*
                     UP
                    +---+
                    |   |
            LEFT    |   |   RIGHT
                    +---+
                    BOTTOM
            */


            left = new GridCell[1 + 2]; //We need to add 2 on each side to cover the outbound of object
            right = new GridCell[1 + 2];
            bottom = new GridCell[1 + 2];
            up = new GridCell[1 + 2];

            _obj = obj;
        }

        public void Update(GridMap map)
        {
            UpdateSide(left, LeftRange());
            UpdateSide(right, RightRange());
            UpdateSide(bottom, BottomRange());
            UpdateSide(up, UpRange());
        }

        private void UpdateSide(GridCell[] side, Rang2 rang)
        {
            int i = 0;
            for (int x = rang.beg.x; x <= rang.end.x; x++)
            {
                for (int y = rang.beg.y; y <= rang.end.y; y++)
                {
                    side[i++] = _obj.Map[x, y];
                }
            }
        }

        public Rang2 LeftRange(bool addExtraBounds = true)
        {
            var extra = (addExtraBounds) ? 1 : 0;
            return new Rang2(
                new Vec2(_obj.GridPos.x - 1, _obj.GridPos.y - extra),
                new Vec2(_obj.GridPos.x - 1, _obj.GridPos.y + extra)
            );
        }
        public Rang2 RightRange(bool addExtraBounds = true)
        {
            var extra = (addExtraBounds) ? 1 : 0;
            return new Rang2(
                new Vec2(_obj.GridPos.x + 1, _obj.GridPos.y - extra),
                new Vec2(_obj.GridPos.x + 1, _obj.GridPos.y + extra)
            );
        }
        public Rang2 BottomRange(bool addExtraBounds = true)
        {
            var extra = (addExtraBounds) ? 1 : 0;
            return new Rang2(
                new Vec2(_obj.GridPos.x - extra, _obj.GridPos.y - 1),
                new Vec2(_obj.GridPos.x + extra, _obj.GridPos.y - 1)
            );
        }
        public Rang2 UpRange(bool addExtraBounds = true)
        {
            var extra = (addExtraBounds) ? 1 : 0;
            return new Rang2(
                new Vec2(_obj.GridPos.x - extra, _obj.GridPos.y + 1),
                new Vec2(_obj.GridPos.x + extra, _obj.GridPos.y + 1)
            );
        }

        public bool CantPassThrough(GridCell cell)
        {
            return cell == null || cell.HasAttribute(CellAttributes.WALL) || cell.FindFirstByType(GridObject.WALL_TYPES) != null;
        }

        public bool CanPassThrough(GridCell cell)
        {
            return !CantPassThrough(cell);
        }

        public bool CantPassThrough(GridCell cell, Predicate<GridObject> collideWith)
        {
            return cell == null || cell.HasAttribute(CellAttributes.WALL) || cell.Find(collideWith) != null;
        }

        public bool CanPassThrough(GridCell cell, Predicate<GridObject> collideWith)
        {
            return !CantPassThrough(cell, collideWith);
        }

        public Vec2f CheckMove(Vec2f dir)
        {
            if (dir.x > 0)
            {
                if (CantPassThrough(right[1]))
                    dir.x = 0;
            }
            else if (dir.x < 0)
            {
                if (CantPassThrough(left[1]))
                    dir.x = 0;
            }

            if (dir.y > 0)
            {
                if (CantPassThrough(up[1]))
                    dir.y = 0;
            }
            else if (dir.y < 0)
            {
                if (CantPassThrough(bottom[1]))
                    dir.y = 0;
            }

            return dir;
        }
    }

    public enum GridDir
    {
        UP,
        RIGHT,
        DOWN,
        LEFT
    }

    public class GridObject
    {
        public static readonly ObjectType[] WALL_TYPES = { ObjectType.BOMB };

        protected readonly bool _smartMove;
        public bool IsSmartMove { get => _smartMove; }

        protected readonly GridMap _map;
        public GridMap Map
        {
            get
            {
                return _map;
            }
        }

        protected readonly uint _uid;
        public uint UID
        {
            get
            {
                return _uid;
            }
        }

        protected readonly ObjectType _type;
        public ObjectType Type
        {
            get
            {
                return _type;
            }
        }

        protected bool _onMap;
        public bool IsOnMap
        {
            get
            {
                return _onMap;
            }
        }

        protected Vec2 _gridPos;
        virtual public Vec2 GridPos
        {
            get
            {
                return _gridPos;
            }
            set
            {
                if (_gridPos == value)
                    return;

                var previousGridPos = _gridPos;
                _gridPos = value;
                UpdateGridCell(previousGridPos);
                UpdateSurroundings(Map);
            }
        }

        protected Vec2f _worldPos;
        public Vec2f WorldPos
        {
            get
            {
                return _worldPos;
            }
        }

        protected readonly GridObjectSurrounding _surroundings;
        public GridObjectSurrounding Surroundings
        {
            get
            {
                return _surroundings;
            }
        }

        protected GridDir _facing;
        public GridDir Facing
        {
            get
            {
                return _facing;
            }
        }

        public GridObject(uint uid, ObjectType type, bool smartMove, GridMap map)
        {
            _uid = uid;
            _map = map;
            _type = type;
            _smartMove = smartMove;

            _gridPos = Vec2.INVALID;
            _onMap = false;

            _surroundings = new GridObjectSurrounding(this);
        }

        public virtual void UpdateGridCell(Vec2 previousGridPos)
        {
            if (previousGridPos.IsValid())
            {
                Map[previousGridPos.x, previousGridPos.y].Remove(_uid);
            }

            if (_onMap && _gridPos.IsValid())
            {
                Map[_gridPos.x, _gridPos.y].Add(this);
            }
        }

        public virtual void UpdateSurroundings(GridMap map)
        {
            if (!_gridPos.IsValid())
            {
                CLog.E("Can't compute surroundings with an invalid position or invalid corners!");
                return;
            }

            _surroundings.Update(map);
        }

        public virtual void EnterMap()
        {
            _onMap = true;
            _map.AddObject(this);
            UpdateGridCell(Vec2.INVALID);
        }

        public virtual void LeaveMap()
        {
            _onMap = false;
            _map.RemoveObject(this);
            UpdateGridCell(GridPos);
        }

        /// <summary>
        /// Reset all attributes of GridObject
        /// </summary>
        /// <param name="fromPool">Indicates if this method was called whitin the Pool Object. You probably won't need to set that as true.</param>
        public virtual void Reset()
        {
            _gridPos = Vec2.ZERO;
        }

        public virtual void Wrap(Vec2f worldPos)
        {
            _worldPos = worldPos;
            GridPos = _map.WorldToGrid(worldPos);
        }

        public virtual void AlignXCenter()
        {
            _worldPos.x = _map.GridToWorld(GridPos).x;
        }

        public virtual void AlignYCenter()
        {
            _worldPos.y = _map.GridToWorld(GridPos).y;
        }

        public virtual void ForceMove(float x, float y)
        {
            var dest = _worldPos;

            dest.x += x;
            dest.y += y;

            var newGridPos = _map.WorldToGrid(dest);

            //If the dest isn't on same grid and we are allowed to move, change grid position.
            if (_gridPos != newGridPos)
            {
                GridPos = newGridPos;
            }

            //Change facing direction
            if (x > 0)
                _facing = GridDir.RIGHT;
            else if (x < 0)
                _facing = GridDir.LEFT;
            else if (y > 0)
                _facing = GridDir.UP;
            else if (y < 0)
                _facing = GridDir.DOWN;

            _worldPos = dest;
        }

        public virtual void Move(float x, float y)
        {
            if (x == 0 && y == 0)
                return;

            if (x > 0 && y > 0)
            {
                throw new System.Exception("Can only move one direction at a time");
            }

            var dest = _worldPos;

            dest.x += x;
            dest.y += y;

            var center = _map.GridToWorld(_gridPos);
            var newGridPos = _map.WorldToGrid(dest);

            var dir = dest - _worldPos;
            dir.Normalize();

            bool sameGrid = _gridPos == newGridPos;

            var canMoveGrid = _surroundings.CheckMove(dir);

            //Work per axis
            if (x != 0)
            {
                if (canMoveGrid.x != 0) //Can move
                {
                    //Only allow to move on other axis, if we are centered.
                    var dist = _worldPos.y - center.y;
                    var alignDir = (x > 0) ? 1 : -1;

                    if (System.Math.Abs(dist) > 0.1f && _smartMove)
                    {
                        //Let's try to do the smart move
                        Move(0, (dist > 0) ? -x * alignDir : x * alignDir);
                        return;
                    }
                    else
                    {
                        dest.y = center.y; //Ensure it's always aligned on center.
                    }
                }
                else
                {
                    //Ensure object is always on center
                    if (dir.x > 0 && dest.x > center.x)
                        dest.x = center.x;
                    else if (dir.x < 0 && dest.x < center.x)
                        dest.x = center.x;

                    //If object isn't centered, let's try move it around object.
                    var dist = _worldPos.y - center.y;
                    var alignDir = (x > 0) ? 1 : -1;
                    if (System.Math.Abs(dist) > 0.1f && _smartMove)
                    {
                        if (x > 0 && _surroundings.CanPassThrough(_surroundings.right[(dist > 0) ? 2 : 0]))
                        {
                            Move(0, (dist > 0) ? x * alignDir : -x * alignDir);
                            return;
                        }
                        else if (x < 0 && _surroundings.CanPassThrough(_surroundings.left[(dist > 0) ? 2 : 0]))
                        {
                            Move(0, (dist > 0) ? x * alignDir : -x * alignDir);
                            return;
                        }
                    }
                }
            }
            else
            {
                if (canMoveGrid.y != 0) //Can move
                {
                    //Only allow to move on other ayis, if we are centered.
                    var dist = _worldPos.x - center.x;
                    var alignDir = (y > 0) ? 1 : -1;

                    if (System.Math.Abs(dist) > 0.1f && _smartMove)
                    {
                        Move((dist > 0) ? -y * alignDir : y * alignDir, 0);
                        return;
                    }
                    else
                    {
                        dest.x = center.x; //Ensure it's always aligned on center.
                    }
                }
                else
                {
                    //Ensure object is always on center
                    if (dir.y > 0 && dest.y > center.y)
                        dest.y = center.y;
                    else if (dir.y < 0 && dest.y < center.y)
                        dest.y = center.y;

                    //If object isn't centered, let's try move it around object.
                    var dist = _worldPos.x - center.x;
                    var alignDir = (y > 0) ? 1 : -1;
                    if (System.Math.Abs(dist) > 0.1f && _smartMove)
                    {
                        if (y > 0 && _surroundings.CanPassThrough(_surroundings.up[(dist > 0) ? 0 : 2]))
                        {
                            Move((dist > 0) ? y * alignDir : -y * alignDir, 0);
                            return;
                        }
                        else if (y < 0 && _surroundings.CanPassThrough(_surroundings.bottom[(dist > 0) ? 2 : 0]))
                        {
                            Move((dist > 0) ? y * alignDir : -y * alignDir, 0);
                            return;
                        }
                    }
                }
            }

            //If the dest isn't on same grid and we are allowed to move, change grid position.
            if (!sameGrid && canMoveGrid != Vec2f.ZERO)
            {
                GridPos = newGridPos;
            }

            //Change facing direction
            if (x > 0)
                _facing = GridDir.RIGHT;
            else if (x < 0)
                _facing = GridDir.LEFT;
            else if (y > 0)
                _facing = GridDir.UP;
            else if (y < 0)
                _facing = GridDir.DOWN;

            _worldPos = dest;
        }

        protected virtual Vec2f GetMoveDir(Vec2 newPos)
        {
            return new Vec2i(newPos.x - _gridPos.x, newPos.y - _gridPos.y).Normalize();
        }

        public virtual void Tick(float delta) { }
    }
}
