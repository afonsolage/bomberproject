using CommonLib.Networking;
using CommonLib.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using CommonLib.Messaging.Client;
using Assets.Scripts.Engine.Logic.Object;

public class RoomStage : BaseStage
{
    public int latency = 0;

    public CLogType logLevel = CLogType.Debug;

    public RoomServerConnection ServerConnection
    {
        get
        {
            return (RoomServerConnection)_connection;
        }
    }

    private SpriteDB _spriteDB;

    private uint _roomIndex;
    public uint RoomIndex
    {
        get
        {
            return _roomIndex;
        }
    }

    private uint _sessionID;
    public uint SessionID
    {
        get
        {
            return _sessionID;
        }
        set
        {
            _sessionID = value;
        }
    }

    private UIManager _uiManager;
    internal UIManager UIManager { get { return _uiManager; } }

    private GridEngineInstance _gridEngine;
    public GridEngineInstance GridEngine
    {
        get
        {
            return _gridEngine;
        }
    }

    private ObjectManager _objectManager;
    public ObjectManager ObjectManager
    {
        get
        {
            return _objectManager;
        }
    }

    public override void OnTick(float delta) { }

    protected override void ProcessMessage(string message)
    {
        switch (message)
        {
            case "FinishRoomState":
                FinishRoomState();
                break;
            case "Started":
                ShowWaiting("Trying to connect to server...");
                break;
            case "Connected":
                HideWaiting();
                Start();
                break;
            case "Disconnected":
                ShowWaiting("Connection lost. Trying to reconnect...", () => { StageManager.ChangeStage(StageType.Lobby); });
                break;
            case "Reconnected":
                HideWaiting();
                break;
            default:
                CLog.W("Unknown message received: {0}", message);
                break;
        }
    }

    public void ShowWaiting(string message, EventDelegate.Callback onCancel = null)
    {
        var waiting = UIManager.FindInstance(WindowType.WAITING, true) as Waiting;
        waiting.AddWaitingMessage(message, onCancel);
    }

    public void HideWaiting()
    {
        UIManager.Destroy(WindowType.WAITING);
    }

    private void Start()
    {
        var token = LocalStorage.GetString("TOKEN");

        if (token == null)
        {
            CLog.E("Invalid token! Unable to connect to Room Server");
            StageManager.ChangeStage(StageType.Lobby);

            return;
        }

        ServerConnection.Send(new CX_TOKEN_REQ()
        {
            token = token,
        });
    }

    public override void OnDispose()
    {
        _connection?.Stop();
        _objectManager?.Clear();

        if (GridEngine != null)
            GameObject.Destroy(GridEngine.gameObject);

        if (ObjectManager != null)
            GameObject.Destroy(ObjectManager.gameObject);
    }

    public override void Init(params object[] args)
    {
        var uiRoot = GameObject.Find("UI Root");

#if DEBUG
        if (uiRoot == null)
            CLog.E("A UI Root must exists on this scene.");
#endif

        _uiManager = uiRoot.GetComponent<UIManager>();
        _uiManager.Setup(this);

        //var dbGO = Resources.Load<GameObject>("Prefabs/Map/SpriteDB");
        //_spriteDB = dbGO?.GetComponent<SpriteDB>();
        _spriteDB = Resources.Load<SpriteDB>("Prefabs/Map/SpriteDB");

        if (_spriteDB == null)
        {
            CLog.E("Unable to load sprite database.");
            Application.Quit();
            return;
        }

        _objectManager = new GameObject("ObjectManager").AddComponent<ObjectManager>();
        _objectManager.Init(this);

        _gridEngine = new GameObject("GridEngine").AddComponent<GridEngineInstance>();

        _roomIndex = uint.Parse(args[0].ToString());
        var serverIp = args[1] as string;
        var port = int.Parse(args[2].ToString());

        _connection = new RoomServerConnection(this, serverIp, port)
        {
#if _DEBUG
            LatencySimulation = (uint)latency
#endif
        };
        _connection.Start();
    }

    internal void MatchEnd(uint winnerUID)
    {
        //var player = GridEngine.Map.FindObject(winnerUID) as PlayerRoom;
        //TODO: Add name of player also...

        var winnerWindow = UIManager.FindInstance(WindowType.WINNER, true) as WinnerWindow;
        winnerWindow.SetMessage(string.Format("Player {0} won!", winnerUID));
    }

    public void FinishRoomState()
    {
        //UIHelper.FindUI("MatchEnd")?.SetActive(false);
        //UIHelper.FindUI("FakeLobbyPanel")?.SetActive(true);

        _objectManager.Reset();

        var go = new GameObject("GridEngine");
        _gridEngine = go.AddComponent<GridEngineInstance>();
    }

    public void MapLoaded()
    {
        var renderer = _gridEngine.gameObject.AddComponent<GridMapRenderer>();
        renderer.typeSprites = _spriteDB.TypeSprites;
        renderer.typeBlocksObjects = _spriteDB.TypeBlocks;

        _objectManager.Map = GridEngine.Map;
    }
}
