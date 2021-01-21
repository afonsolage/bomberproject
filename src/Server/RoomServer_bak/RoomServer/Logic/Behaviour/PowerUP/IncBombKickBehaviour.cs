using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoomServer.Logic.Object;

namespace RoomServer.Logic.Behaviour.PowerUP
{
    class IncBombKickBehaviour : PowerUpBehaviour
    {
        internal override bool OnCollect(Player collector)
        {
            _powerUp.Collected = true;
            collector.Attr.kickBomb = true;
            return true;
        }

        internal override void OnInit()
        {

        }
    }
}
