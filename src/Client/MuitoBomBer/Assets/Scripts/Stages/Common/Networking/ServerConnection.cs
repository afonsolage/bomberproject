using CommonLib.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class ServerConnection : ClientSocket
{
    protected readonly BaseStage _stage;

    public ServerConnection(BaseStage stage, string serverHost, int serverPort) : base(serverHost, serverPort)
    {
        _stage = stage;
    }

    public override void Handle(Packet packet)
    {
        _stage.AddToMainThreadQueue(packet);
    }
    public abstract void HandleOnMainThread(Packet packet);

    protected override bool OnDisconnect()
    {
        _stage.AddToMainThreadQueue("Disconnected");
        return false; //Don't reconnect, since the room server was disconnected.
    }
    protected override void OnStart()
    {
        _stage.AddToMainThreadQueue("Started");
    }

    protected override void OnReconnect()
    {
        _stage.AddToMainThreadQueue("Reconnected");
    }

    protected override void OnConnect()
    {
        _stage.AddToMainThreadQueue("Connected");
    }
}
