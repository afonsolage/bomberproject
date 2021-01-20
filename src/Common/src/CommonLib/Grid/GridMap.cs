using CommonLib.Util.Math;
using CommonLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;

namespace CommonLib.GridEngine
{
    [Flags]
    public enum CellAttributes
    {
        NONE = 0x00,
        WALL = 0x01,
        BREAKABLE = 0x02,
        COLLECTABLE = 0x04,

        INVALID = 0xFF,
    }

    public enum CellType
    {
        None = 0,
        Invisible = 1,
        Plant = 2,
        Rock = 3,
        Wooden = 4,
        Anvil = 5,

        Invalid, //Should be always the last one
    }

    public struct CellConfig
    {
        public CellType cellType;
        public CellAttributes cellAttr;

        public CellConfig(int cellType, int cellAttr)
        {
            this.cellType = (CellType)cellType;
            this.cellAttr = (CellAttributes)cellAttr;
        }
    }

    public class GridCell
    {
        private static readonly Dictionary<CellType, CellAttributes> _TypeAttr = new Dictionary<CellType, CellAttributes>();

        public static List<CellType> Types
        {
            get
            {
                return _TypeAttr.Keys.ToList();
            }
        }

        public static Dictionary<CellType, CellAttributes> TypesAttr
        {
            get
            {
                return new Dictionary<CellType, CellAttributes>(_TypeAttr);
            }
        }

        public static void LoadTypes(List<CellConfig> list)
        {
            foreach (var tp in list)
            {
                _TypeAttr[tp.cellType] = tp.cellAttr;
            }
        }

        public static GridCell Deserialize(byte rawByte, Vec2 pos)
        {
            return new GridCell((CellType)rawByte, pos);
        }

        private CellAttributes _attr;
        public CellAttributes Attr
        {
            get
            {
                return _attr;
            }
        }

        private CellType _type;
        public CellType Type
        {
            get
            {
                return _type;
            }
        }

        private readonly List<GridObject> _objects;
        private readonly Vec2 _pos;
        public Vec2 Pos
        {
            get
            {
                return _pos;
            }
        }

        public GridCell(CellType type, Vec2 pos)
        {
            _objects = new List<GridObject>();
            _pos = pos;
            UpdateCell(type);
        }

        public bool HasAttribute(CellAttributes attr)
        {
            return (_attr & attr) == attr;
        }

        public void UpdateCell(CellType type)
        {
            _type = type;
            _attr = (type == CellType.Invalid) ? CellAttributes.NONE : _TypeAttr[type];
        }

        public byte Serialize()
        {
            return (byte)_type;
        }

        public void Add(GridObject obj)
        {
#if _DEBUG
            if (Find(obj.UID) != null)
            {
                CLog.F("Trying to add an object that alread exists on grid. Obj UID: {0}, pos: {1}", obj.UID, obj.GridPos);
                return;
            }
#endif
            _objects.Add(obj);
        }

        public GridObject Find(uint uid)
        {
            return _objects.Find((o) => o.UID == uid);
        }

        public GridObject Find(Predicate<GridObject> predicate)
        {
            return _objects.Find(predicate);
        }

        public GridObject FindFirstByType(params ObjectType[] types)
        {
            return _objects.Find((o) => Array.IndexOf(types, o.Type) >= 0);
        }

        public List<GridObject> FindAllByType<T>() where T : GridObject
        {
            return _objects.FindAll((o) => o is T);
        }

        public void Remove(uint uid)
        {
            _objects.RemoveAll((o) => o.UID == uid);
        }

        public bool IsEmpty()
        {
            return _type == CellType.None && _objects.Count == 0;
        }
    }

    public class GridMap
    {
        protected readonly float _cellSize;
        public float CellSize
        {
            get
            {
                return _cellSize;
            }
        }

        protected readonly Vec2 _mapSize;
        public Vec2 MapSize
        {
            get
            {
                return _mapSize;
            }
        }

        protected readonly GridCell[] _cells;
        public GridCell[] CellData
        {
            get { return _cells; }
        }

        protected readonly Vec2 _centerOffset;
        public Vec2 CenterOffset
        {
            get
            {
                return _centerOffset;
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

#if _SERVER
        protected GridObject[] _spawnSlots;
#endif
        protected Vec2[] _spawnPlaces;
        public Vec2[] SpawnPlaces
        {
            get
            {
                return _spawnPlaces;
            }
        }

        public int MaxPlayer
        {
            get
            {
                return _spawnPlaces?.Length ?? 0;
            }
        }

        public delegate void ObjectAdded(GridObject obj);
        public delegate void ObjectRemoved(GridObject obj);

        protected event ObjectAdded _onObjectAdded;
        public event ObjectAdded OnObjectAdded
        {
            add
            {
                _onObjectAdded += value;
            }
            remove
            {
                _onObjectAdded -= value;
            }
        }

        protected event ObjectAdded _onObjectRemoved;
        public event ObjectAdded OnObjectRemoved
        {
            add
            {
                _onObjectRemoved += value;
            }
            remove
            {
                _onObjectRemoved -= value;
            }
        }

#if _DEBUG
        protected int _threadTickId;
#endif
        protected List<GridObject> ActiveObjects
        {
            get
            {
#if _DEBUG
                if (_threadTickId > 0 && _threadTickId != Thread.CurrentThread.ManagedThreadId)
                {
                    Debug.Assert(false,
                        "You can't access ActiveObjects outside the thread that ticks the Map. This will cause concurrency problems. Use GetActiveObjectsAsync instead. " +
                        string.Format("ThreadTickId: {0}, CurrentThreadId: {1}", _threadTickId, Thread.CurrentThread.ManagedThreadId));
                }
#endif
                return _activeObjects;
            }
        }
        private readonly List<GridObject> _activeObjects;

        private readonly List<GetActiveObjectAsyncDelegate> _delegates = new List<GetActiveObjectAsyncDelegate>();
        private void SendActiveObjectsAsync()
        {
            if (_delegates.Count == 0)
                return;

            lock(_delegates)
            {
                foreach(var cb in _delegates)
                {
                    cb(new List<GridObject>(_activeObjects));
                }
                _delegates.Clear();
            }
        }

        protected delegate void GetActiveObjectAsyncDelegate(List<GridObject> activeObjects);
        protected void GetActiveObjectsAsync(GetActiveObjectAsyncDelegate callback)
        {
            lock(_delegates)
            {
                _delegates.Add(callback);
            }
        }

        public virtual void Shutdown()
        {
#if _DEBUG
            _threadTickId = 0;
#endif
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="cellSize"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="uid"></param>
        public GridMap(float cellSize, int width, int height, uint uid)
        {
            _uid = uid;
            _cellSize = cellSize;
            _mapSize = new Vec2(width, height);
            _centerOffset = new Vec2(width / 2, height / 2);

            _cells = new GridCell[width * height];
            _activeObjects = new List<GridObject>();
        }

        public virtual bool Load(byte[] buffer)
        {
            if (buffer == null || buffer.Length < _cells.Length)
                return false;

            // The buffer is composed of spawnPlaces + mapData, so first read spawns, after it, read map data.
            int i = 0;

            // Check how many places there are on this mapa.
            var playersSlots = buffer[i++];
            _spawnPlaces = new Vec2[playersSlots];
#if _SERVER
            _spawnSlots = new GridObject[playersSlots];
#endif

            int k = 0;
            // Read spawn places
            for (; k < playersSlots; k++)
            {
                _spawnPlaces[k] = new Vec2(buffer[i++], buffer[i++]);
            }

            // Read map data
            for (k = 0; k < _cells.Length; i++, k++)
            {
                _cells[k] = GridCell.Deserialize(buffer[i], FromIndex(k));
            }

            OnLoad();

            return true;
        }

        public virtual void UpdateCells()
        {

        }

        protected virtual void OnLoad()
        {

        }

#if _SERVER
        public virtual byte[] Serialize()
        {
            // We just need to serialize the type, since Attributes and Position will be computed on Load method.
            return _cells.Select(c => (byte)c.Type).ToArray();
        }

        /// <summary>
        /// Get spawn coordinates (x, y).
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual Vec2 GetSpawnPos(GridObject obj)
        {
            for (var i = 0; i < _spawnSlots.Length; i++)
            {
                var slot = _spawnSlots[i];
                if (slot == null)
                    continue;

                if (slot.UID == obj.UID)
                {
                    return _spawnPlaces[i];
                }
            }

            return Vec2.INVALID;
        }

        /// <summary>
        /// Allocate the object into a slot by index.
        /// If it fails and tryRandomSlot is true, it will attempt to allocate the object to a random slot.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual bool AllocSlot(GridObject obj, int spawnIndex, bool tryRandomSlot = false)
        {
            if (!IsAvailableSlot(spawnIndex))
            {
                if (tryRandomSlot)
                {
                    var idx = GetNextFreeSlot();
                    if (idx == -1)
                        return false;

                    spawnIndex = idx;
                }
                else
                {
                    return false;
                }
            }

            _spawnSlots[spawnIndex] = obj;

            return true;
        }

        /// <summary>
        /// Allocate the object into a random slot.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual bool AllocSlot(GridObject obj)
        {
            var idx = GetNextFreeSlot();
            if (idx == -1)
                return false;

            _spawnSlots[idx] = obj;

            return true;
        }

        /// <summary>
        /// Remove the object from the slot.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected virtual bool FreeSlot(GridObject obj)
        {
            for (var i = 0; i < _spawnSlots.Length; i++)
            {
                var slot = _spawnSlots[i];
                if (slot == null)
                    continue;

                if (slot.UID == obj.UID)
                {
                    _spawnSlots[i] = null;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check from the index if the slot is in use.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected virtual bool IsAvailableSlot(int index)
        {
            // Check if the index is larger than the size of the current spawn slots.
            if (index >= _spawnSlots.Length)
                return false;

            // Check if already has some object in current slot index.
            if (_spawnSlots[index] != null)
                return false;

            return true;
        }

        /// <summary>
        /// Get the next slot not in use.
        /// </summary>
        /// <returns></returns>
        protected virtual int GetNextFreeSlot()
        {
            for (int i = 0; i < _spawnSlots.Length; i++)
            {
                if (_spawnSlots[i] == null)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Get the number of slots in use.
        /// </summary>
        /// <returns></returns>
        protected virtual int SlotsInUse()
        {
            var cnt = 0;

            foreach (var b in _spawnSlots)
            {
                if (b != null)
                    cnt++;
            }

            return cnt;
        }

        /// <summary>
        /// Check for available slots.
        /// </summary>
        /// <returns></returns>
        public virtual bool HasFreeSlot()
        {
            return SlotsInUse() < _spawnSlots.Length;
        }
#endif

        public virtual int ToIndex(int x, int y)
        {
            return (x * _mapSize.y) + y;
        }

        public virtual int ToIndex(Vec2 pos)
        {
            return (pos.x * _mapSize.y) + pos.y;
        }

        public virtual Vec2 FromIndex(int index)
        {
            return new Vec2(index / _mapSize.y, index % _mapSize.y);
        }

        protected List<GridObject> _toAddObjects = new List<GridObject>();
        internal void AddObject(GridObject obj)
        {
            lock (_toAddObjects)
            {
                _toAddObjects.Add(obj);
            }
        }
        protected virtual void AddAllPendingObjects()
        {
            lock (_toAddObjects)
            {
                foreach (var obj in _toAddObjects)
                {
                    if (!obj.GridPos.IsValid())
                    {
                        CLog.E("Failed to add object. Invalid position!");
                        return;
                    }

                    var idx = ToIndex(obj.GridPos);

                    if (idx == -1)
                    {
                        CLog.E("Failed to add object. Invalid index!");
                        return;
                    }

                    if (FindObject(obj.UID) != null)
                    {
                        CLog.E("Trying to add an object that already exists.");
                    }
                    else
                    {
                        _activeObjects.Add(obj);
                        _onObjectAdded?.Invoke(obj);
                    }
                }
                _toAddObjects.Clear();
            }
        }

        protected List<GridObject> _toRemoveObjects = new List<GridObject>();
        public virtual void RemoveObject(GridObject obj)
        {
            lock (_toRemoveObjects)
            {
                _toRemoveObjects.Add(obj);
            }
        }

        protected virtual void RemoveAllPendingObjects()
        {
            lock (_toRemoveObjects)
            {
                foreach (var obj in _toRemoveObjects)
                {
                    if (obj.Map != this)
                    {
                        CLog.E("Trying to remove an object from another grid!");
                        continue;
                    }

                    var removed = _activeObjects.RemoveAll((existingObj) => existingObj.UID == obj.UID) > 0;

                    if (removed)
                    {
                        _onObjectRemoved?.Invoke(obj);
#if _SERVER
                        FreeSlot(obj);
#endif
                    }
                }
                _toRemoveObjects.Clear();
            }
        }

        public virtual GridObject FindObject(uint uid)
        {
            return _activeObjects.Find((obj) => obj.UID == uid);
        }

        public virtual GridObject FindFirstByType(params ObjectType[] types)
        {
            return _activeObjects.Find((o) => Array.IndexOf(types, o.Type) >= 0);
        }

        public virtual List<T> FindAllByType<T>() where T : GridObject
        {
            return _activeObjects.FindAll((o) => o is T).Select(o => o as T).ToList();
        }

        public GridCell this[int x, int y]
        {
            get
            {
                return CellAt(ToIndex(x, y));
            }
            set
            {
                CellAt(ToIndex(x, y), value);
            }
        }

        public GridCell this[int i]
        {
            get
            {
                return CellAt(i);
            }
            set
            {
                CellAt(i, value);
            }
        }

        protected virtual GridCell CellAt(int idx)
        {
            if (idx < 0 || idx >= _cells.Length)
                return null;
            else
                return _cells[idx];
        }

        protected virtual void CellAt(int idx, GridCell val)
        {
            if (idx >= 0 && idx < _cells.Length)
                _cells[idx] = val;
        }

        public virtual Vec2 WorldToGrid(Vec2f world)
        {
            var zeroOriginPos = world + new Vec2f(_centerOffset.x * _cellSize, _centerOffset.y * _cellSize);
            return new Vec2((int)(zeroOriginPos.x / _cellSize + _cellSize / 2), (int)(zeroOriginPos.y / _cellSize + _cellSize / 2));
        }

        public virtual Vec2f GridToWorld(Vec2 grid)
        {
            var worldOffset = new Vec2f(_centerOffset.x * _cellSize, _centerOffset.y * _cellSize);
            var worldPos = new Vec2f(grid.x * _cellSize, grid.y * _cellSize);

            worldPos -= worldOffset;

            return worldPos;
        }

        public virtual void Tick(float delta)
        {
#if _DEBUG
            _threadTickId = Thread.CurrentThread.ManagedThreadId;
#endif

            RemoveAllPendingObjects();
            AddAllPendingObjects();

            var tempList = new List<GridObject>(_activeObjects);

            foreach (var obj in tempList)
            {
                obj.Tick(delta);
            }
        }

        public virtual GridCell[] GetDump()
        {
            var dump = new GridCell[_cells.Length];

            _cells.CopyTo(dump, 0);

            foreach (GridObject obj in _activeObjects)
            {
                if (!obj.GridPos.IsValid())
                {
                    continue;
                }

                var idx = ToIndex(obj.GridPos.x, obj.GridPos.y);
                dump[idx] = new GridCell(CellType.Invalid, obj.GridPos);
            }

            return dump;
        }

        /// <summary>
        /// Finds the shortest path from an origin to a destination, using a given function to check if can pass through the cell.
        /// <para>Returns an empty list if the path can be calculated.</para> 
        /// <para>If the destination is unreachable, this function will return the closest reachable position.</para> 
        /// <para>To check if the destination position is reachable, just check if the last position on returned list is the destination one.</para> 
        /// </summary>
        /// <param name="origin">Origin of the path</param>
        /// <param name="dest">Destination of the path</param>
        /// <param name="canPass">Function to check if we can pass through the given position.</param>
        /// <param name="dirs">An array of directions, used to avoid predictable movement pattern. If null, Vec2.ALL_DIRS will be used</param>
        /// <returns>The list of positions that forms the path.</returns>
        public List<Vec2> PathFind(Vec2 origin, Vec2 dest, Func<GridCell, bool> canPass, Vec2[] dirs = null)
        {
            if (dirs == null)
                dirs = Vec2.ALL_DIRS;

            var finder = new AStarFinder(this, dirs);
            return finder.FindShortestPath(origin, dest, canPass);
        }

        /// <summary>
        /// Finds the closest walkable position, given a function to check if the position is the desired one.
        /// </summary>
        /// <param name="origin">Where to start searching from</param>
        /// <param name="checkCell">Function to check if the given position is the one that we want</param>
        /// <param name="canPass">Function to check if we can pass through the given position.</param>
        /// <param name="dirs">An array of directions, used to avoid predictable movement pattern. If null, Vec2.ALL_DIRS will be used</param>
        /// <returns>A valid position if we were able to find one. An invalid position (Vec2.INVALID) otherwise</returns>
        public Vec2 FindClosestPos(Vec2 origin, int maxRange, Predicate<GridCell> checkCell, Predicate<GridCell> canPass, Vec2[] dirs = null)
        {
            if (dirs == null)
                dirs = Vec2.ALL_DIRS;

            var range = System.Math.Min(System.Math.Max(MapSize.x, MapSize.y), maxRange);
            //Just keep the track of what positions we already verified to avoid eternal loop.
            var verifiedPos = new HashSet<Vec2>();

            //Since we are working with a max range, we can have enought buckets as our range.
            var queues = new Queue<Vec2>[range + 1];

            for (var i = 0; i < queues.Length; i++)
            {
                queues[i] = new Queue<Vec2>(10); //Init with an avarage amount of "slots" to aviod memory alloc on loop.
            }

            var currentDist = 0;
            queues[currentDist].Enqueue(origin);

            while (currentDist <= range)
            {
                var queue = queues[currentDist];

                if (queue.Count == 0)
                {
                    //We need to find the queue closest queue that has something enqueued.
                    currentDist = range + 1; //If all queues is empty, this will make us leave the loop.
                    for (var i = 0; i < queues.Length; i++) //Since all queues are indexed by distance, we can iterate over it's length
                    {
                        if (queues[i].Count > 0)
                        {
                            currentDist = i;
                            break;
                        }
                    }
                    continue;
                }

                var pos = queue.Dequeue();
                var cell = this[pos.x, pos.y];

                if (checkCell(cell))
                    return pos;

                foreach (var dir in dirs)
                {
                    var neighbor = dir + pos;

                    if (!neighbor.IsOnBounds(MapSize.x, MapSize.y))
                        continue;

                    var dist = neighbor.Distance(origin);
                    if (dist > range)
                        continue;

                    if (verifiedPos.Contains(neighbor))
                        continue;
                    else
                        verifiedPos.Add(neighbor);

                    var neighborCell = this[neighbor.x, neighbor.y];

                    if (canPass(neighborCell))
                    {
                        queues[dist].Enqueue(neighbor);
                    }
                }
            }

            return Vec2.INVALID;
        }

        private struct PosRanking
        {
            public Vec2 Pos;
            public int Ranking;
        }

        /// <summary>
        /// Finds the best walkable position using a given function to compute the ranking of each cell.
        /// </summary>
        /// <param name="origin">Where to start searching from</param>
        /// <param name="maxRange">Maximum distance allowed to perform the check</param>
        /// <param name="calcRanking">Function to calculate the ranking of the given position. Lower is better.</param>
        /// <param name="checkCell">Function to check if the given position is the one that we want</param>
        /// <param name="canPass">Function to check if we can pass through the given position.</param>
        /// <param name="dirs">An array of directions, used to avoid predictable movement pattern. If null, Vec2.ALL_DIRS will be used</param>
        /// <returns>A valid position if we were able to find one. An invalid position (Vec2.INVALID) otherwise</returns>
        public Vec2 FindBestPos(Vec2 origin, int maxRange, Func<GridCell, int> calcRanking, Predicate<GridCell> checkCell, Predicate<GridCell> canPass, Vec2[] dirs = null)
        {
            if (dirs == null)
                dirs = Vec2.ALL_DIRS;

            var range = System.Math.Min(System.Math.Max(MapSize.x, MapSize.y), maxRange);

            //Just keep the track of what positions we already verified to avoid eternal loop.
            var verifiedPos = new HashSet<Vec2>();
            var candidates = new List<PosRanking>();

            var queue = new Queue<Vec2>();
            queue.Enqueue(origin);

            while (queue.Count > 0)
            {
                var pos = queue.Dequeue();
                var cell = this[pos.x, pos.y];

                if (checkCell(cell))
                    candidates.Add(new PosRanking() { Pos = pos, Ranking = calcRanking(cell) });

                foreach (var dir in dirs)
                {
                    var neighbor = dir + pos;

                    if (!neighbor.IsOnBounds(MapSize.x, MapSize.y))
                        continue;

                    var dist = neighbor.Distance(origin);
                    if (dist > range)
                        continue;

                    if (verifiedPos.Contains(neighbor))
                    {
                        continue;
                    }
                    else
                    {
                        verifiedPos.Add(neighbor);
                    }

                    var neighborCell = this[neighbor.x, neighbor.y];
                    if (canPass(neighborCell))
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }

            if (candidates.Count > 0)
            {
                candidates.Sort((a, b) => a.Ranking - b.Ranking);
                return candidates[0].Pos;
            }
            else
            {
                return Vec2.INVALID;
            }
        }
    }
}
