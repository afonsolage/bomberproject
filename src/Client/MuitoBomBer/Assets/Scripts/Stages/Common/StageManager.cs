using CommonLib.Networking;
using CommonLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class BaseStage
{
    private const int MAX_PACKETS_PER_TICK = 10;

    protected ServerConnection _connection;
    protected Queue<Packet> _mainThreadPacketQueue = new Queue<Packet>();
    protected Queue<string> _mainThreadMessageQueue = new Queue<string>();

    public abstract void Init(params object[] args);
    public abstract void OnDispose();
    protected abstract void ProcessMessage(string message);
    public abstract void OnTick(float delta);

    public void FixedTick(float delta)
    {
        for (var i = MAX_PACKETS_PER_TICK; i > 0; i--)
        {
            Packet packet = Packet.Empty;

            lock (_mainThreadPacketQueue)
            {
                if (_mainThreadPacketQueue.Count > 0)
                    packet = _mainThreadPacketQueue.Dequeue();
            }

            if (packet.buffer != null)
            {
                _connection.HandleOnMainThread(packet);
            }
            else
            {
                break;
            }
        }

        string message = null;
        lock (_mainThreadMessageQueue)
        {
            if (_mainThreadMessageQueue.Count > 0)
            {
                message = _mainThreadMessageQueue.Dequeue();
            }
        }

        if (message != null)
            ProcessMessage(message);
    }

    public void Tick(float delta)
    {
        OnTick(delta);
    }

    internal void AddToMainThreadQueue(Packet packet)
    {
        lock (_mainThreadPacketQueue)
        {
            _mainThreadPacketQueue.Enqueue(packet);
        }
    }

    internal void AddToMainThreadQueue(string message)
    {
        lock (_mainThreadMessageQueue)
        {
            _mainThreadMessageQueue.Enqueue(message);
        }
    }

    internal void Dispose()
    {
        OnDispose();

        lock (_mainThreadPacketQueue)
            _mainThreadPacketQueue.Clear();

        lock (_mainThreadMessageQueue)
            _mainThreadMessageQueue.Clear();
    }
}

public enum StageType
{
    Loading,
    Lobby,
    Room,
}

public static class StageManager
{
    private static Dictionary<StageType, BaseStage> _stageDict = new Dictionary<StageType, BaseStage>();

    private static BaseStage Instanciate(StageType type)
    {
        CLog.I("Changing stage to: {0}", type);
        return (BaseStage)Activator.CreateInstance(GetStageType(type));
    }

    private static BaseStage _currentStage;
    private static StageType _currentStageType;
    public static StageType CurrentStageType
    {
        get { return _currentStageType; }
    }

    private static Type GetStageType(StageType type)
    {
        switch (type)
        {
            case StageType.Loading:     return typeof(LoadingStage);
            case StageType.Lobby:       return typeof(LobbyStage);
            case StageType.Room:        return typeof(RoomStage);
            default:                    throw new NotImplementedException();
        }
    }

    public static T GetCurrent<T>() where T : BaseStage
    {
        return _currentStage as T;
    }

    public static void ChangeStage(StageType stageType, params object[] stageArgs)
    {
        BaseStage stage = null;
        if (!_stageDict.TryGetValue(stageType, out stage))
        {
            stage = Instanciate(stageType);
            _stageDict[stageType] = stage;
        }

        if (_currentStage != null)
            _currentStage.Dispose();

        stage.Init(stageArgs);

        _currentStage = stage;
        _currentStageType = stageType;
    }

    public static void Tick(float delta)
    {
        if (_currentStage != null)
            _currentStage.Tick(delta);
    }

    public static void FixedTick(float delta)
    {
        if (_currentStage != null)
            _currentStage.FixedTick(delta);
    }

    internal static void Shutdown()
    {
        if (_currentStage != null)
            _currentStage.Dispose();
    }
}
