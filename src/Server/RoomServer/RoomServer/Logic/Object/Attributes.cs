using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomServer.Logic.Object
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
    }
}
