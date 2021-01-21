using CommonLib.GridEngine;
using CommonLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomServer.Logic.Object
{
    public abstract class HitableObject : GridObject
    {
        protected bool _live;
        public bool IsLive
        {
            get
            {
                return _live;
            }
        }

        protected HitableObject _lastHitter;

        protected abstract BaseObjectAttributes BaseAttr { get; }

        public HitableObject(uint uid, ObjectType type, bool smartMove, GridMap map) : base(uid, type, smartMove, map)
        {
            _live = true;
        }

        public override void Tick(float delta)
        {
            base.Tick(delta);

            if (_live && BaseAttr.lifePoints == 0)
            {
                Die();
            }
        }

        protected void Die()
        {
            OnDead(_lastHitter);

            _live = false;
        }

        public bool Hit(HitableObject hitter)
        {
            var myAttr = BaseAttr;

            //Check if object is dead or there isn't more life points left.
            //This is necessary because the object is only set as dead (_live = false) when the OnDead function is called on its own tick.
            if (!_live || myAttr.lifePoints == 0)
                return false;

#if _DEBUG
            if (hitter._live == false)
            {
                CLog.W("Dead object {0} tryint to hit!", hitter);
                return false;
            }
#endif

            var hitterAttr = hitter.BaseAttr;
            var damage = (long)hitterAttr.attackPoints - myAttr.defensePoints;

            if (damage <= 0)
                return false;

            if (!CanHit(hitter))
                return false;

            _lastHitter = hitter;

            if (damage > myAttr.lifePoints)
            {
                myAttr.lifePoints = 0;
            }
            else
            {
                myAttr.lifePoints -= (uint) damage;
            }

            return OnHit(hitter);
        }

        protected abstract void OnDead(HitableObject killer);
        protected abstract bool CanHit(HitableObject hitter);
        protected abstract bool OnHit(HitableObject hitter);

        internal void HitKill()
        {
            BaseAttr.lifePoints = 0;
            _lastHitter = null;
        }
    }
}
