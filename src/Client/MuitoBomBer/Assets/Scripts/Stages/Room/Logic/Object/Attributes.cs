using CommonLib.Messaging.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Engine.Logic.Object
{
    public class BaseObjectAttributes
    {
        public uint lifePoints;
        public uint attackPoints;
        public uint defensePoints;
    }

    public class PlayerAttributes : BaseObjectAttributes
    {
        public uint bombCount;
        public uint bombArea;
        public uint moveSpeed;
        public float immunityTime;
        public bool kickBomb;

        public PlayerAttributes() {}
        public PlayerAttributes(PLAYER_ATTRIBUTES attr)
        {
            attackPoints = attr.attackPoints;
            defensePoints = attr.defensePoints;
            lifePoints = attr.lifePoints;

            bombCount = attr.bombCount;
            bombArea = attr.bombArea;
            moveSpeed = attr.common.moveSpeed;
            immunityTime = attr.immunityTime;
            kickBomb = attr.kickBomb;
        }
    }
}