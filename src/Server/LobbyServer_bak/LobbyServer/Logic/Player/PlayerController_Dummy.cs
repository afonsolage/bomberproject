using CommonLib.Messaging;
using CommonLib.Messaging.Client;
using CommonLib.Messaging.Common;
using CommonLib.Messaging.DB;
using LobbyServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LobbyServer.Logic
{
    internal partial class PlayerController
    {
        internal Dummy CreateDummy()
        {
            var dummy = new Dummy(App);

            lock (_players)
            {
                _players.Add(dummy);
            }

            return dummy;
        }

        internal List<Dummy> ListAllDummies()
        {
            var res = new List<Dummy>();

            lock (_players)
            {
                res.AddRange(_players.Where(p => p is Dummy).Select(p => p as Dummy));
            }

            return res;
        }

    }
}
