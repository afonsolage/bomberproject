using LightJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib.Messaging.Common;
using LightJson.Serialization;
using RoomServer.Logic.Object;
using CommonLib.Util.Math;
using CommonLib.GridEngine;

namespace RoomServer.Logic.PowerUP
{
    internal abstract class PowerUpManager
    {
        private static readonly Dictionary<string, PowerUpInfo> _infos = new Dictionary<string, PowerUpInfo>();
        private static readonly Random _rnd = new Random((int)DateTime.Now.Ticks);

        internal static void Setup(List<POWERUP> list)
        {
            foreach (var item in list)
            {
                var info = new PowerUpInfo()
                {
                    index = item.index,
                    name = item.name,
                    icon = item.icon,
                    rate = item.rate,
                    behaviours = new Dictionary<string, JsonObject>(),
                };

                if (item.behaviour?.Count > 0)
                {
                    foreach (var pair in item.behaviour)
                    {
                        JsonObject val = (string.IsNullOrEmpty(pair.Value)) ? null : JsonReader.Parse(pair.Value).AsJsonObject;
                        info.behaviours.Add(pair.Key, val);
                    }
                }

                _infos[info.name] = info;
            }
        }

        private static bool CanCreatePowerUp(Room map, Vec2 pos)
        {
            // Get all power ups from the room.
            var powerUp = map.FindAllByType<PowerUp>();
            if (powerUp.Count > 0)
            {
                foreach(var power in powerUp)
                {
                    // Check if exist some block in this place.
                    if (!map[pos.x, pos.y].HasAttribute(CellAttributes.NONE))
                        return false;

                    // Already exist a power up in this position.
                    if (power.GridPos.x == pos.x && power.GridPos.y == pos.y)
                        return false;
                }
            }

            // Don't exist some power up in current room.
            return true;
        }

        public static bool ForceInstanciate(Room map, Vec2 pos, int index)
        {
            // Check if already has power up in current position.
            if (CanCreatePowerUp(map, pos))
            {
                // Generate some power up.
                foreach (var info in _infos)
                {
                    if(info.Value.index == index)
                    {
                        var powerup = new PowerUp(map);
                        powerup.Setup(info.Value, pos);

                        // Successfully in to created a power up.
                        return true;
                    }
                }
            }

            // Failed in to created a power up.
            return false;
        }

        public static bool RandomInstanciate(Room map, Vec2 pos, float dropRate = 1f)
        {
            // Check if already has power up in current position.
            if(CanCreatePowerUp(map, pos))
            {
                // Generate some power up.
                foreach (var info in _infos)
                {
                    var random = _rnd.Next(0, 100);

                    if (info.Value.rate * dropRate >= random)
                    {
                        var powerup = new PowerUp(map);
                        powerup.Setup(info.Value, pos);

                        // Successfully in to created a power up.
                        return true;
                    }
                }
            }

            // Failed in to created a power up.
            return false;
        }
    }
}
