using CommonLib.GridEngine;
using CommonLib.Util.Math;
using CommonLib.Messaging.Client;
using CommonLib.Messaging.Common;
using LightJson;
using RoomServer.Logic.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomServer.Logic.Behaviour.Map
{
    class DebugBehaviour : RoomBehaviour
    {
        internal override void Setup(params object[] args)
        {
            base.Setup(args);

            var settings = args[1] as JsonObject;
        }

        internal override void OnInit()
        {
            GenerateTerrain();
        }

        private void GenerateTerrain()
        {
            var excludedArea = new List<Vec2>();

            foreach (var spawn in _room.SpawnPlaces)
            {
                excludedArea.Add(spawn);

                foreach (var dir in Vec2.ALL_DIRS)
                {
                    var pos = spawn + dir;
                    var cell = _room[pos.x, pos.y];

                    if (cell == null)
                        continue;

                    excludedArea.Add(pos);
                }
            }

            for (var x = 0; x < _room.MapSize.x; x++)
            {
                for (var y = 0; y < _room.MapSize.y; y++)
                {
                    var pos = new Vec2(x, y);
                    var cell = _room[pos.x, pos.y];

                    if (cell == null)
                        continue;

                    if (excludedArea.Contains(pos))
                        continue;

                    if (cell.Type != CellType.None)
                        continue;

                    if (cell.FindAllByType<GridObject>()?.Count > 0)
                        continue;

                    cell.UpdateCell(GenerateCell(pos));
                }
            }
        }

        private Random _genCellRand = new Random((int)DateTime.Now.Ticks);
        private CellType GenerateCell(Vec2 pos)
        {
            var rand = _genCellRand.Next(0, 100);

            if (rand < 30)
                return CellType.None;
            else if (rand < 80)
                return CellType.Plant;
            else
                return CellType.Wooden;
        }

        internal override void OnTick(float delta)
        {
            CheckEndGame();
        }

        private void CheckEndGame()
        {
            var owner = _room.GetOwner<Player>();
            if (owner?.IsLive ?? false)
            {
                _room.EndMatch(new MatchEndInfo()
                {
                    winner = owner.UID,
                });
            }
        }

        internal override void Destroy()
        {
        }
    }
}
