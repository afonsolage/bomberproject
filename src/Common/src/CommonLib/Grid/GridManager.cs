using CommonLib.Util;
using System.Collections.Generic;
using System.Threading;

namespace CommonLib.GridEngine
{
    public abstract class GridManager<T> where T : GridMap
    {
        protected readonly Dictionary<uint, T> _maps = new Dictionary<uint, T>();
        protected uint _nextMapUid = 1;

        protected bool _initialized = false;
        public bool IsInitialized
        {
            get
            {
                return _initialized;
            }
        }

        public virtual void Init(List<CellConfig> typeList)
        {
            if (_initialized)
                return;

            GridCell.LoadTypes(typeList);

            _initialized = true;
        }

        public virtual void Tick(float delta)
        {
            lock(_maps)
            {
                foreach (T map in _maps.Values)
                {
                    map.Tick(delta);
                }
            }
        }

        public virtual T Find(uint uid)
        {
            lock(_maps)
            {
                if (_maps.TryGetValue(uid, out T res))
                    return res;
                else
                    return null;
            }
        }

        protected abstract T Instanciate(float cellSize, ushort width, ushort height, uint uid);

        public virtual T Create(float cellSize, ushort width, ushort height, byte[] data = null)
        {
            T map;
            lock(_maps)
            {
                map = Instanciate(cellSize, width, height, _nextMapUid++);
                _maps.Add(map.UID, map);
            }

            if (data != null)
            {
                map.Load(data);
            }

            return map;
        }

        public virtual void Destroy(T map)
        {
            lock (_maps)
            {
                _maps.Remove(map.UID);
            }
        }
    }
}
