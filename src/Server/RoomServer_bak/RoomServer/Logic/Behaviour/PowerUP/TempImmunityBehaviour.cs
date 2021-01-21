using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoomServer.Logic.Object;
using LightJson;

namespace RoomServer.Logic.Behaviour.PowerUP
{
    class TempImmunityBehaviour : PowerUpBehaviour
    {
        private float _duration = 0;

        internal override bool OnCollect(Player collector)
        {
            _powerUp.Collected = true;
            collector.SetImmune(_duration);
            return true;
        }

        internal override void OnInit()
        {
        }

        internal override void Setup(params object[] args)
        {
            base.Setup(args);

            var settings = args[1] as JsonObject;

            _duration = (float)settings["duration"].AsNumber;
        }
    }
}
