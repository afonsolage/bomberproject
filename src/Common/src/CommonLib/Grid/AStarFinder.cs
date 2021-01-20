using CommonLib.Util.Math;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib.GridEngine
{
    class AStarFinder
    {
        class BFSNode
        {
            public Vec2 pos;
            public int depth;
            public int greedy;
        }

        private readonly GridMap _map;
        private readonly Dictionary<Vec2, BFSNode> _nodeMap = new Dictionary<Vec2, BFSNode>();
        private readonly List<BFSNode> _queue = new List<BFSNode>();
        private readonly Vec2[] _dirs;

        public AStarFinder(GridMap map, Vec2[] dirs)
        {
            _map = map;
            _dirs = dirs;
        }

        public virtual List<Vec2> FindShortestPath(Vec2 origin, Vec2 dest, Func<GridCell, bool> canPass)
        {
            if (!origin.IsValid() || !dest.IsValid() || canPass == null || origin == dest)
                return new List<Vec2>();

            _nodeMap.Clear();
            _queue.Clear();

            Add(new BFSNode()
            {
                pos = origin,
                depth = 0,
                greedy = origin.Distance(dest),
            });

            while (_queue.Count > 0)
            {
                var node = _queue[0];
                _queue.RemoveAt(0);

                if (node.pos == dest)
                    return Backtrace(node);

                foreach (var dir in _dirs)
                {
                    var neighbor = dir + node.pos;

                    if (_nodeMap.ContainsKey(neighbor))
                        continue;

                    if (!canPass(_map[neighbor.x, neighbor.y]))
                        continue;

                    Add(new BFSNode()
                    {
                        pos = neighbor,
                        depth = node.depth + 1,
                        greedy = neighbor.Distance(dest),
                    });
                }

                SortQueue();
            }

            //If we didn't find a way to get there. Find the closest way.
            _queue.Clear();
            _queue.AddRange(_nodeMap.Values);

            if (_queue.Count == 0)
                return new List<Vec2>();

            SortQueue();

            return Backtrace(_queue[0]);
        }

        private List<Vec2> Backtrace(BFSNode node)
        {
            var res = new List<Vec2>
            {
                node.pos
            };

            var depth = node.depth;
            var n = node;

            while (depth > 0)
            {
#if _DEBUG
                var currentDepth = depth;
#endif
                var currentPos = n.pos;

                foreach (var dir in _dirs)
                {
                    var neighbor = dir + currentPos;

                    if (!_nodeMap.TryGetValue(neighbor, out var neighborNode))
                        continue;

                    if (neighborNode.depth < depth)
                    {
                        n = neighborNode;
                        depth = n.depth;
                    }
                }

#if _DEBUG
                Debug.Assert(currentDepth != depth, "This means we are in an eternal loop");
#endif
                res.Add(n.pos);
            }

            //Since we compute the backtrace (from dest to origin, we need to reverse the order to have a valid path - origin to dest)
            res.Reverse();

            return res;
        }

        private void Add(BFSNode node)
        {
            _queue.Add(node);
            _nodeMap.Add(node.pos, node);
        }

        private void SortQueue()
        {
            _queue.Sort((v1, v2) => v1.greedy == v2.greedy ? v1.depth - v2.depth : v1.greedy - v2.greedy);
        }
    }
}
