using CommonLib.GridEngine;
using CommonLib.Util.Math;
using LightJson;
using RoomServer.Logic.Behaviour;
using RoomServer.Logic.Behaviour.PowerUP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoomServer.Logic.Object
{
    internal class PowerUpInfo
    {
        internal uint index;
        internal string name;
        internal uint icon;
        internal float rate;
        internal Dictionary<string, JsonObject> behaviours;
    }

    internal class PowerUp : HitableObject
    {
        private static readonly string POWERUP_BEHAVIOUR_NAMESPACE = "RoomServer.Logic.Behaviour.PowerUP";

        private static int nextUID = 0;

        private static int NextUID()
        {
            return Interlocked.Increment(ref nextUID);
        }

        private static uint GenUniqueUID()
        {
            return (uint)("PowerUp " + NextUID()).GetHashCode();
        }

        protected string _name;
        protected uint _icon;
        public uint Icon
        {
            get
            {
                return _icon;
            }
        }

        protected float _rate;

        public bool Collected { get; set; }

        protected BehaviourController<PowerUpBehaviour> _behaviourController;
        public BehaviourController<PowerUpBehaviour> Behaviour
        {
            get
            {
                return _behaviourController;
            }
        }

        private BaseObjectAttributes _attr;
        protected override BaseObjectAttributes BaseAttr
        {
            get
            {
                return _attr;
            }
        }

        public PowerUp(GridMap map) : base(GenUniqueUID(), ObjectType.POWERUP, false, map)
        {
            _behaviourController = new BehaviourController<PowerUpBehaviour>(this);
            _attr = new BaseObjectAttributes()
            {
                attackPoints = 0,
                defensePoints = 0,
                lifePoints = 1,
            };
        }

        public void Setup(PowerUpInfo info, Vec2 pos)
        {
            _name = info.name;
            _icon = info.icon;
            _rate = info.rate;

            Wrap(_map.GridToWorld(pos));

            if (info.behaviours != null)
            {
                foreach (var pair in info.behaviours)
                {
                    _behaviourController.AddBehaviour(string.Format("{0}.{1}Behaviour", POWERUP_BEHAVIOUR_NAMESPACE, pair.Key), pair.Value);
                }
            }

            EnterMap();
        }

        public override void Tick(float delta)
        {
            base.Tick(delta);
            _behaviourController.Tick(delta);
        }

        public void Destroy()
        {
            _behaviourController.Destroy();
            LeaveMap();
        }

        protected override void OnDead(HitableObject killer)
        {
            Destroy();
        }

        protected override bool CanHit(HitableObject hitter)
        {
            return true;
        }

        protected override bool OnHit(HitableObject hitter)
        {
            return true;
        }
    }
}
