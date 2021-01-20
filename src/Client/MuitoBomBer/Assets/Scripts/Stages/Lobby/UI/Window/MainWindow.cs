using CommonLib.Messaging;
using CommonLib.Messaging.Client;
using System.Collections.Generic;
using UnityEngine;

internal class MainWindow : UIComponent
{
    public GameObject _charInfo;

    public UILabel _charName;
    public UILabel _charLevel;

    public GameObject _menu;

    // Chat
    public GameObject _chat;
    public UITextList _chatBox;

    // Battery
    public GameObject _battery;
    public UI2DSprite _batterySprite;
    private float _batteryAccumTime;
    private readonly float BATTERY_UPDATE_TIME = 1f;

    // Wi-Fi Signal
    public GameObject _wifi;
    public List<GameObject> _wifiSprites;

    private void Start()
    {
        //_chatBox?.Clear();
    }

    private void Update()
    {
        // Battery process.
        if (_battery.activeSelf)
        {
            _batteryAccumTime += Time.deltaTime;

            if (_batteryAccumTime > BATTERY_UPDATE_TIME)
            {
                var level = SystemInfo.batteryLevel;
                _batterySprite.fillAmount = (level == -1) ? 1 : level;

                _batteryAccumTime = 0f;
            }
        }
    }

    public void SetInfoPlayer(string name, uint level)
    {
        _charName.text = name;
        _charLevel.text = string.Format("Lv. {0}", level);
    }

    public void SetPing(long ping)
    {
        if(ping < 100)
        {
            _wifiSprites[0].SetActive(false);
            _wifiSprites[1].SetActive(true);
            _wifiSprites[2].SetActive(true);
            _wifiSprites[3].SetActive(true);
        }
        else if (ping >= 100 && ping < 150)
        {
            _wifiSprites[0].SetActive(false);
            _wifiSprites[1].SetActive(true);
            _wifiSprites[2].SetActive(true);
            _wifiSprites[3].SetActive(false);
        }
        else if (ping >= 150 && ping < 250)
        {
            _wifiSprites[0].SetActive(false);
            _wifiSprites[1].SetActive(true);
            _wifiSprites[2].SetActive(false);
            _wifiSprites[3].SetActive(false);
        }
        else if (ping >= 250)
        {
            _wifiSprites[0].SetActive(true);
            _wifiSprites[1].SetActive(false);
            _wifiSprites[2].SetActive(false);
            _wifiSprites[3].SetActive(false);
        }
    }

    public void AddMessage(string msg)
    {
        _chatBox.Add(msg);
    }

    public void OnClickedRoom()
    {
        _parent.Instanciate(WindowType.LOBBY);

        // Request player's on lobby.
        var lobbyStage = _parent.Stage as LobbyStage;
        lobbyStage.RequestLobbyListPlayers();

        DisableComponents(true);
    }

    public void DisableComponents(bool disable)
    {
        if(disable)
        {
            _menu?.SetActive(false);
            _charInfo?.SetActive(false);
            _chat?.SetActive(false);
        }
        else
        {
            _menu?.SetActive(true);
            _charInfo?.SetActive(true);
            _chat?.SetActive(true);
        }
    }

    public void OnClickedFriend()
    {
        _parent.Instanciate(WindowType.FRIEND);
        DisableComponents(true);
    }

    public void OnLogout()
    {
        var lobbyStage = _parent.Stage as LobbyStage;
        lobbyStage.Logout();
    }

    public void OnClickedChat()
    {
        DisableComponents(true);

        _parent.Instanciate(WindowType.CHAT);
    }
}
