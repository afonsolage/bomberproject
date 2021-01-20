#if _TEST

using CommonLib.Messaging;
using CommonLib.Messaging.Base;
using CommonLib.Networking;
using CommonLib.Server;
using CommonLib.Util;
using ProtoBuf;
using System;
using System.Diagnostics;
using System.Threading;

class TesticuloServer : GameLoopServer
{
    public TesticuloServer(uint instanceId, string name, string version, uint ticksPerSecond) : base(instanceId, name, version, ticksPerSecond)
    {
    }

    public override void Tick(float delta)
    {
    }

    protected override void OnStart()
    {
    }

    protected override void ProcessCommand(string[] command)
    {
        CLog.D("Received: " + command);
    }
}

class TestMain
{
    public static int Main(String[] args)
    {
        CLog.writter = (CLogType type, string formattedMessage) =>
        {
            switch (type)
            {
                case CLogType.Success:
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case CLogType.Fatal:
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case CLogType.Error:
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case CLogType.Warn:
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case CLogType.Info:
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case CLogType.Debug:
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
            }

            Console.WriteLine(formattedMessage);
        };

        var test = new TesticuloServer(1, "Testiculo Server", "0.0.0.0", 64);
        test.Init();
        test.Start();

        return 0;
    }
}

#endif