using CommonLib.Messaging;
using CommonLib.Messaging.Client;
using CommonLib.Messaging.Common;
using CommonLib.Networking;
using CommonLib.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LobbyStage : BaseStage
{
    public CLogType logLevel = CLogType.Debug;

    public LobbyServerConnection ServerConnection
    {
        get { return (LobbyServerConnection)_connection; }
    }

    private UIManager _uiManager;
    internal UIManager UIManager { get { return _uiManager; } }

    private MainPlayer _mainPlayer;
    internal MainPlayer MainPlayer { get { return _mainPlayer; } }

    private RoomController _roomController;
    internal RoomController RoomController { get { return _roomController; } }

    private FriendController _friendController;
    internal FriendController FriendController { get { return _friendController; } }

    private ChatController _chatController;
    internal ChatController ChatController { get { return _chatController; } }

    private string _serverIP = "";
    private int _serverPort = 0;

    public override void Init(params object[] args)
    {
        _roomController = new RoomController(this);
        _friendController = new FriendController(this);
        _chatController = new ChatController(this);

        var uiRoot = GameObject.Find("UI Root");
        if (uiRoot == null)
        {
            CLog.E("An UI Root must exists on this scene.");
            return;
        }

        _uiManager = uiRoot.GetComponent<UIManager>();
        _uiManager.Setup(this);

        _serverIP = args[0] as string;
        _serverPort = int.Parse(args[1].ToString());

        _connection = new LobbyServerConnection(this, _serverIP, _serverPort)
        {
#if _DEBUG
            LatencySimulation = (uint)latency
#endif
        };

        _connection.Start();
    }

    public void InitMainPlayer(PLAYER_INFO player)
    {
        _mainPlayer = new MainPlayer(player.index, player.nick, player.gender, player.level, player.experience)
        {
            Stage = player.stage,
            RoomIndex = player.roomIndex,
            Ready = player.state == PlayerState.Ready,
            Offline = player.state == PlayerState.Offline,
        };

        string welcomeMessage = string.Format("Welcome {0} to Bomberman Origin!", player.nick);
        ChatController.AddMessage(ChatType.SYSTEM, welcomeMessage);

        // TODO : DEBUG PURPOSE, DELETE THIS CODE!
        var mainWindow = UIManager.FindInstance(WindowType.MAIN) as MainWindow;
        if(mainWindow)
        {
            //mainWindow.SetInfoPlayer(player.nick, player.level);
        }
        else
        {
            Debug.LogError("Not found mainWindow.");
        }
    }

    private void Start()
    {
        var token = LocalStorage.GetString("TOKEN");
        if (token == null)
            ShowLoginWindow();
        else
            DoTokenAuthentication(token);
    }

    internal void Logout()
    {
        ServerConnection.Send(new CL_LOGOUT_REQ() { });

        LocalStorage.SetString("TOKEN", null);

        UIManager.DestroyAll();

        _connection.Stop();

        _connection = new LobbyServerConnection(this, _serverIP, _serverPort);
        _connection.Start();
    }

    public void ShowLoginWindow()
    {
        UIManager.Instanciate(WindowType.LOGIN);
    }

    public void AuthSuccess(bool firstLogin)
    {
        if (firstLogin)
        {
            UIManager.Instanciate(WindowType.PLAYER_CREATION);
        }
        else
        {
            // Init Main Window.
            UIManager.Instanciate(WindowType.MAIN);
        }
    }

    private void DoTokenAuthentication(string token)
    {
        ShowWaiting("Authenticating...");

        ServerConnection.Send(new CX_TOKEN_REQ() { token = token });
    }

    public override void OnTick(float delta)
    {

    }

    protected override void ProcessMessage(string message)
    {
        switch (message)
        {
            case "Started":
                ShowWaiting("Trying to connect to server...");
                break;
            case "Connected":
                HideWaiting();
                Start();
                break;
            case "Disconnected":
                ShowWaiting("Connection lost. Trying to reconnect...");
                break;
            case "Reconnected":
                HideWaiting();
                break;
            default:
                CLog.W("Unknown message received: {0}", message);
                break;
        }
    }

    public void ShowWaiting(string message)
    {
        var waiting = UIManager.FindInstance(WindowType.WAITING, true) as Waiting;
        waiting.AddWaitingMessage(message);
    }

    public void HideWaiting()
    {
        UIManager.Destroy(WindowType.WAITING);
    }

    public void ShowHint(string text)
    {
        var msgHint = UIManager.FindInstance(WindowType.MSG_HINT, true) as MessageHint;
        msgHint.AddMessageHint(text);
    }

    public override void OnDispose()
    {
        _connection?.Stop();
        _roomController.Clear();
        UIManager.DestroyAll();
    }

    public void RequestLobbyListPlayers()
    {
        ServerConnection.Send(new CL_PLAYER_LOBBY_LIST_REQ() { });
    }
}
