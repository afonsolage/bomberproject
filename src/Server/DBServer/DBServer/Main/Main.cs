using DBServer.Server;

namespace DBServer
{
    class MainApp
    {
        static void Main(string[] args)
        {
            var instanceId = uint.Parse(args[0]);

            AppServer server = new AppServer(instanceId);
            server.Init();
            server.Start();
        }
    }
}
