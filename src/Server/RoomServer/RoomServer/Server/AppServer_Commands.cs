using CommonLib.GridEngine;
using CommonLib.Util.Math;
using CommonLib.Messaging.Client;
using CommonLib.Messaging.Common;
using CommonLib.Server;
using CommonLib.Util;
using RoomServer.Logic.Object;
using RoomServer.Logic.PowerUP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomServer.Server
{
    internal partial class AppServer : GameLoopServer
    {
        #region PROCESS COMMAND
        protected override void ProcessCommand(string[] command)
        {
            base.ProcessCommand(command);

            var cmd = command[0];
            switch (cmd)
            {
                case "disconnect":
                    ProcessDisconnect(command);
                    break;
                case "rooms":
                    _roomManager.PrintRooms();
                    break;
                case "dummy":
                    ProcessDummies(command);
                    break;
                case "powerup":
                    ProcessPower(command);
                    break;
                case "removeallcell":
                    ProcessRemoveAllCell(command);
                    break;
                case "block":
                    ProcessBlock(command);
                    break;
                default:
                    CLog.W("Unkown command: {0}", cmd);
                    break;
            }
        }
        #endregion

        protected void ProcessDisconnect(string[] command)
        {
            if (command.Length < 2)
            {
                CLog.W("Disconnect command syntax: disconnect <connectionId>", command[0]);
                return;
            }

            var id = uint.Parse(command[1]);
            _socketServer.Disconnect(id);
        }

        #region DUMMIES

        /// <summary>
        /// All dummies in room.
        /// </summary>
        private static Dictionary<uint, List<Dummy>> _dummies = new Dictionary<uint, List<Dummy>>();

        private void ProcessDummies(string[] command)
        {
            if (command.Length < 3)
            {
                CLog.W("Dummy command: Dummy <add|remove|list|move> <room id>");
                return;
            }

            var cmd = command[1];

            switch (cmd)
            {
                case "add":
                    AddDummy(uint.Parse(command[2]));
                    break;
                case "remove":
                    RemoveDummy(uint.Parse(command[2]), command.Length == 4 ? command[3] : null);
                    break;
                case "list":
                    ListDummies(uint.Parse(command[2]));
                    break;
                case "freeze":
                    FreezeDummies(uint.Parse(command[2]));
                    break;
                case "unfreeze":
                    UnfreezeDummies(uint.Parse(command[2]));
                    break;
                case "move":
                    if (command.Length < 5)
                    {
                        CLog.W("Dummy move command is wrong!");
                        return;
                    }

                    MoveDummy(uint.Parse(command[2]), float.Parse(command[3]), float.Parse(command[4]));
                    break;
                default:
                    CLog.W("Dummy command: Dummy <add|remove|list|move> <room id>");
                    break;
            }
        }

        private void UnfreezeDummies(uint roomUID)
        {
            var room = _roomManager.Find(roomUID);

            if (room == null)
            {
                CLog.W("Failed to unfreeze dummy. Room not found: {0}", roomUID);
                return;
            }

            room.ListActiveObjectsAsync(objs =>
            {
                foreach (var obj in objs)
                {
                    if (obj is Dummy dummy)
                    {
                        dummy.Unfreeze();
                    }
                }

                CLog.I("Dummies unfreezed successfully! on room {0}", roomUID);
            });
        }

        private void FreezeDummies(uint roomUID)
        {
            var room = _roomManager.Find(roomUID);

            if (room == null)
            {
                CLog.W("Failed to freeze dummy. Room not found: {0}", roomUID);
                return;
            }

            room.ListActiveObjectsAsync(objs =>
            {
                foreach (var obj in objs)
                {
                    if (obj is Dummy dummy)
                    {
                        dummy.Freeze();
                    }
                }

                CLog.I("Dummies freezed successfully! on room {0}", roomUID);
            });
        }

        private void AddDummy(uint roomUID)
        {
            var room = _roomManager.Find(roomUID);

            if (room == null)
            {
                CLog.F("Failed to add dummy. Room not found: {0}", roomUID);
                return;
            }

            var pos = room.FindRandFreeGrid();

            if (!pos.IsValid())
            {
                CLog.F("Failed to find an empty grid on room: {0}", roomUID);
                return;
            }

            var dummy = new Dummy(this, room);

            if (!room.AllocSlot(dummy))
            {
                CLog.W("The room is full!");
                return;
            }

            var spawn = room.GetSpawnPos(dummy);
            dummy.Wrap(room.GridToWorld(spawn));
            dummy.EnterMap();

            if (_dummies.ContainsKey(room.UID))
            {
                _dummies[room.UID].Add(dummy);
            }
            else
            {
                var dummyList = new List<Dummy>
                {
                    dummy
                };
                _dummies[room.UID] = dummyList;
            }
        }

        private void RemoveDummy(uint roomUID, string rawUID)
        {
            var room = _roomManager.Find(roomUID);

            Dummy removed = null;

            if (room == null)
            {
                CLog.F("Failed to add dummy. Room not found: {0}", roomUID);
                return;
            }

            if (!_dummies.ContainsKey(room.UID) || _dummies[room.UID].Count == 0)
            {
                CLog.W("There is no dummy at given room: {0}", roomUID);
                return;
            }
            else
            {
                if (rawUID == null)
                {
                    removed = _dummies[room.UID][0];
                    _dummies[room.UID].Remove(removed);
                }
                else
                {
                    var uid = uint.Parse(rawUID);
                    removed = _dummies[room.UID].Find((d) => d.UID == uid);

                    if (removed == null)
                    {
                        CLog.W("There is no dummy at room {0} with UID {1}", roomUID, uid);
                        return;
                    }
                    else
                    {
                        _dummies[room.UID].Remove(removed);
                    }
                }
            }

            removed.LeaveMap();
        }

        private void ListDummies(uint roomUID)
        {
            if (_dummies.ContainsKey(roomUID))
            {
                var dummies = _dummies[roomUID];
                CLog.D("Exists {0} dummies at room {1}", dummies.Count, roomUID);
            }
            else
            {
                CLog.D("There is no dummy at room {0}", roomUID);
            }
        }

        private void MoveDummy(uint roomUID, float moveX, float moveY)
        {
            var room = _roomManager.Find(roomUID);

            if (room == null)
            {
                CLog.F("Failed to add dummy. Room not found: {0}", roomUID);
                return;
            }

            if (!_dummies.ContainsKey(room.UID) || _dummies[room.UID].Count == 0)
            {
                CLog.W("There is no dummy at given room: {0}", roomUID);
                return;
            }

            CLog.S("Moving dummy {0} units.", new Vec2f(moveX, moveY));

            var dummy = _dummies[room.UID][0];
            var b4 = dummy.WorldPos;
            dummy.Move(moveX, moveY);

            if (dummy.WorldPos == b4)
            {
                CLog.W("Failed to move.");
            }
            else
            {
                room.BroadcastWorldPos(dummy);
            }
        }
        #endregion // DUMMIES

        #region POWER

        protected void ProcessPower(string[] command)
        {
            if (command.Length < 5)
            {
                CLog.W("Power command: powerup <add> <room id> <index> <x> <y>");
                return;
            }

            var cmd = command[1];

            switch (cmd)
            {
                case "add":
                    AddPowerUp(uint.Parse(command[2]), int.Parse(command[3]), int.Parse(command[4]), int.Parse(command[5]));
                    break;
                default:
                    CLog.W("powerup command: PowerUP add <room id> <type> <x> <y>");
                    break;
            }
        }

        protected void AddPowerUp(uint roomUID, int index, int x, int y)
        {
            var room = _roomManager.Find(roomUID);

            if (room == null)
            {
                CLog.F("Failed to add power up. Room not found: {0}", roomUID);
                return;
            }

            if (PowerUpManager.ForceInstanciate(room, new Vec2(x, y), index))
            {
                CLog.S("Created power up in X: {0} Y: {1} in room {2}", x, y, roomUID);
            }
            else
            {
                CLog.F("Failed to create power up in X: {0} Y: {1} in room {2}", x, y, roomUID);
            }
        }
        #endregion

        #region REMOVE ALL CELL
        protected void ProcessRemoveAllCell(string[] command)
        {
            if (command.Length < 2)
            {
                CLog.F("RemoveAllCell command: RemoveAllCell <roomId>");
                return;
            }

            var roomUID = uint.Parse(command[1]);
            var room = _roomManager.Find(roomUID);

            if (room == null)
            {
                CLog.F("Failed to add power up. Room not found: {0}", roomUID);
                return;
            }

            List<Vec2> explosionObject = new List<Vec2>();

            var cells = room.CellData;
            foreach (var cell in cells)
            {
                if (cell.HasAttribute(CellAttributes.BREAKABLE))
                {
                    explosionObject.Add(cell.Pos);
                    cell.UpdateCell(CellType.None);
                }
            }

            if (explosionObject.Count > 0)
            {
                var objectExplosion = new List<VEC2>(explosionObject.Count);
                foreach (var pos in explosionObject)
                {
                    objectExplosion.Add(new VEC2()
                    {
                        x = pos.x,
                        y = pos.y
                    });
                }

                room.BroadcastMessage(new CR_BOMB_EXPLODED_OBJECT_NFY()
                {
                    area = objectExplosion
                });
            }
        }
        #endregion

        #region BLOCK
        private void ProcessBlock(string[] command)
        {
            if (command.Length < 2)
            {
                CLog.W("Block command: block <add>");
                return;
            }

            var cmd = command[1];

            switch (cmd)
            {
                case "add":
                    ProcessBlockAdd(command);
                    break;
                default:
                    CLog.W("Block command: block add <room id> <block type> <x> <y>");
                    break;
            }
        }

        private void ProcessBlockAdd(string[] command)
        {
            if (command.Length < 6)
            {
                CLog.W("Block command: block add <room id> <block type> <x> <y>");
                return;
            }

            try
            {
                var roomIdx = uint.Parse(command[2]);
                var room = _roomManager.Find(roomIdx);

                if (room == null)
                {
                    CLog.F("Failed to add block. Room not found: {0}", roomIdx);
                    return;
                }

                var type = (CellType) Enum.Parse(typeof(CellType), command[3]);
                if (!GridCell.Types.Contains(type))
                {
                    CLog.F("Failed to add block. Type not found: {0}", command[3]);
                    return;
                }

                Vec2 pos = new Vec2(int.Parse(command[4]), int.Parse(command[5]));

                if (!pos.IsValid() || !pos.IsOnBounds(room.MapSize.x, room.MapSize.y))
                {
                    CLog.F("Failed to add block. Invalid position: {0}", pos);
                    return;
                }

                var cell = room[pos.x, pos.y];

                cell.UpdateCell((CellType) type);
                room.BroadcastMessage(new CR_HURRY_UP_CELL_NFY()
                {
                    cell = new VEC2()
                    {
                        x = pos.x,
                        y = pos.y,
                    },
                    replaceType = (CellType)type,
                });
            }
            catch (Exception e)
            {
                CLog.W("Failed to process block add command: " + e.Message);
            }
        }

        #endregion
    }
}
