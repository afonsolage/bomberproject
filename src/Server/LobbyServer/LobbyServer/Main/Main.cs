using CommonLib.Server;
using LobbyServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

class MainApp
{
    static int Main(string[] args)
    {
        var instanceId = uint.Parse(args[0]);

        AppServer server = new AppServer(instanceId);

        server.Init();
        server.Start();

        return 0;
    }
}
