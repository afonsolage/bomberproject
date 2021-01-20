using CommonLib.GridEngine;
using CommonLib.Util.Math;
using CommonLib.Messaging.Client;
using CommonLib.Messaging.Common;
using RoomServer.Logic.Object;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace RoomServer.Logic.Behaviour
{
    internal abstract class IBehaviour
    {
        internal abstract void Setup(params object[] args);
        internal abstract void OnInit();
        internal abstract void OnTick(float delta);
        internal abstract void Destroy();
    }

    internal class BehaviourController<T> where T : IBehaviour
    {
        protected List<T> _behaviours;
        protected object _parent;
        protected MethodInfo _addBehaviour;

        internal BehaviourController(object parent)
        {
            _behaviours = new List<T>();
            _parent = parent;
            _addBehaviour = GetType().GetMethod("AddBehaviour", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public void AddBehaviour(string behaviourClassName, params object[] args)
        {
            var behaviourType = Type.GetType(behaviourClassName);
            var method = _addBehaviour.MakeGenericMethod(behaviourType);
            method.Invoke(this, new object[] { args });

        }

        internal void AddBehaviour<K>(params object[] args) where K : T, new()
        {
            var behaviour = new K();
            behaviour.Setup(_parent, args?.Length > 0 ? args[0] : null);
            behaviour.OnInit();

            _behaviours.Add(behaviour);
        }

        internal void RemoveBehaviour<K>() where K : T, new()
        {
            _behaviours.RemoveAll((b) => b is K);
        }

        internal bool HasBehaviour<K>() where K : T, new()
        {
            return _behaviours.Find(b => b is K) != null;
        }

        internal K GetBehaviour<K>() where K : T, new()
        {
            return _behaviours.Find(b => b is K) as K;
        }

        internal void Tick(float delta)
        {
            foreach (var behavour in _behaviours)
            {
                behavour.OnTick(delta);
            }
        }

        internal void Destroy()
        {
            foreach (var behavour in _behaviours)
            {
                behavour.Destroy();
            }

            _behaviours.Clear();
        }

        internal void ForEach(Action<T> cb)
        {
            _behaviours.ForEach(cb);
        }
    }

    internal abstract class RoomBehaviour : IBehaviour
    {
        protected Room _room;

        internal override void Setup(params object[] args)
        {
#if _DEBUG
            if (args?.Length < 1 || !(args[0] is Room))
            {
                throw new ArgumentException("First argument to create a RoomBehaviour should be a room object.");
            }
#endif

            _room = args[0] as Room;
        }
    }

    internal abstract class PowerUpBehaviour : IBehaviour
    {
        protected PowerUp _powerUp;
        protected GridCell _cell;

        internal override void Setup(params object[] args)
        {
#if _DEBUG
            if (args?.Length < 1 || !(args[0] is PowerUp))
            {
                throw new ArgumentException("First argument to create a PowerUpBehaviour should be a powerup object.");
            }
#endif
            _powerUp = args[0] as PowerUp;
            _cell = _powerUp.Map[_powerUp.GridPos.x, _powerUp.GridPos.y];
        }

        internal override void OnTick(float delta)
        {
            if (_cell.FindFirstByType(ObjectType.PLAYER) is Player collector)
            {
                if (OnCollect(collector))
                {
                    collector.Session?.Send(new CR_PLAYER_UPDATE_ATTRIBUTES_RES()
                    {
                        uid = collector.UID,
                        attributes = new PLAYER_ATTRIBUTES()
                        {
                            lifePoints = collector.Attr.lifePoints,
                            attackPoints = collector.Attr.attackPoints,
                            defensePoints = collector.Attr.defensePoints,
                            bombCount = collector.Attr.bombCount,
                            bombArea = collector.Attr.bombArea,
                            immunityTime = collector.Attr.immunityTime,
                            kickBomb = collector.Attr.kickBomb,

                            common = new COMMON_PLAYER_ATTRIBUTES()
                            {
                                moveSpeed = collector.Attr.moveSpeed,
                            },
                        }
                    });

                    // Player got the power up, then we need to remove it of the map..
                    Destroy();
                }
            }
        }

        /// <summary>
        /// Called when there is a player on same cell of PowerUP.
        /// </summary>
        /// <param name="collector">The player which collected the powerup</param>
        /// <returns>True if the powerup should be destroyed. False otherwise.</returns>
        internal abstract bool OnCollect(Player collector);

        internal override void Destroy()
        {
            _powerUp.LeaveMap();
        }
    }
}
