using CommonLib.Messaging;
using CommonLib.Messaging.Client;
using CommonLib.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class ChatWindow : UIComponent
{
    public GameObject _chatMessageSlotComponent;
    private List<GameObject> _lstMessageSlotObjects = new List<GameObject>();

    private ChatType _currentType = ChatType.GENERAL;

    public UIInput _inputChat;

    // Components
    public UIGrid _grid;
    public UIScrollView _scrollView;

    // Buttons Types
    public UIButton _btnGeneral;
    public UIButton _btnNormal;
    public UIButton _btnSystem;

    private void Start()
    {
        ClearChat();

        InitButtonTypes();

        AddHistoryChat();
    }

    private void InitButtonTypes()
    {
        UIEventListener.Get(_btnGeneral.gameObject).onClick = (s) => { ChangeChatType(ChatType.GENERAL); };
        UIEventListener.Get(_btnNormal.gameObject).onClick = (s) => { ChangeChatType(ChatType.NORMAL); };
        UIEventListener.Get(_btnSystem.gameObject).onClick = (s) => { ChangeChatType(ChatType.SYSTEM); };
    }

    private void AddHistoryChat()
    {
        var chatController = CurrentStage<LobbyStage>().ChatController;
        if(chatController != null)
        {
            var messages = chatController.GetHistoryMessage(_currentType);
            if(messages != null)
            {
                foreach (var m in messages)
                {
                    AddMessage(_currentType, m.playerIndex, m.name, m.message, m.selfMsg);
                }
            }
        }
    }

    private void ClearChat()
    {
        foreach (var go in _lstMessageSlotObjects)
        {
            NGUITools.Destroy(go);
        }

        _lstMessageSlotObjects.Clear();

        _grid.Reposition();
        _scrollView.ResetPosition();
    }

    public void OnSendClicked()
    {
        if (string.IsNullOrEmpty(_inputChat.value))
        {
            var msgHint = _parent.FindInstance(WindowType.MSG_HINT, true) as MessageHint;
            msgHint.AddMessageHint("You need to type some text before to send message.");

            return;
        }

        // Get Lobby Stage.
        var lobbyStage = _parent.Stage as LobbyStage;

        switch (_currentType)
        {
            case ChatType.GENERAL:
            case ChatType.NORMAL:
                {
                    lobbyStage.ServerConnection.Send(new CL_CHAT_NORMAL_REQ()
                    {
                        uid = lobbyStage.MainPlayer.Index,
                        msg = _inputChat.value
                    });
                }
                break;
        }

        // Reset message from input.
        _inputChat.value = "";
    }

    public void AddMessage(ChatType type, string msg)
    {
        var gridObject = _grid.gameObject;
        if (gridObject)
        {
            // Create new room.
            var go = NGUITools.AddChild(gridObject, _chatMessageSlotComponent);

            var component = go.GetComponent<ChatMessageSlotComponent>();
            if (component)
            {
                component.Message(name, msg);
            }

            _lstMessageSlotObjects.Add(go);

            _grid.Reposition();
        }
    }

    public void AddMessage(ChatType type, ulong playerIndex, string playerName, string msg, bool self = false)
    {
        var gridObject = _grid.gameObject;
        if (gridObject)
        {
            // Create new room.
            var go = NGUITools.AddChild(gridObject, _chatMessageSlotComponent);

            var component = go.GetComponent<ChatMessageSlotComponent>();
            if(component)
            {
                component.Message(playerIndex, playerName, msg, self);
            }

            _lstMessageSlotObjects.Add(go);

            _grid.Reposition();
        }
    }

    public void OnClickedCloseBtn()
    {
        ClearChat();

        _parent.Destroy(WindowType.CHAT);

        var main = _parent.FindInstance(WindowType.MAIN) as MainWindow;
        main?.DisableComponents(false);
    }

    public void ChangeChatType(ChatType type)
    {
        // Ignore if new chat type is current one.
        if (_currentType == type)
            return;

        // Define new chat type selected.
        _currentType = type;

        // Clear messages from old chat type in text area.
        ClearChat();

        // Get old messages from current chat type.
        AddHistoryChat();
    }


    /////////////////////////////////////////////////////////////////////////////////

    //private ChatType _currentType = ChatType.NORMAL;
    //private Dictionary<ChatType, List<string>> _historyChat;
    //
    //public UITextList _textMessage;
    //
    //public GameObject _nameWhisperInput;
    //public GameObject _whisperInput;
    //
    //public GameObject _normalInput;
    //
    //public void Init()
    //{
    //    _historyChat = new Dictionary<ChatType, List<string>>();
    //
    //    for (var type = ChatType.NORMAL; type < ChatType.MAX; type++)
    //    {
    //        _historyChat.Add(type, new List<string>());
    //    }
    //
    //    _textMessage.Clear();
    //}
    //
    //public void OnCloseClicked()
    //{
    //    var chatWindow = _parent.FindInstance(WindowType.CHAT).gameObject;
    //    chatWindow?.SetActive(false);
    //}
    //
    //public void OnSendClicked()
    //{
    //    UIInput input = (_currentType == ChatType.WHISPER) ? _whisperInput.GetComponentInChildren<UIInput>() : _normalInput.GetComponentInChildren<UIInput>();
    //    if (input == null)
    //    {
    //        CLog.F("Input component is null.");
    //        return;
    //    }
    //
    //    if (string.IsNullOrEmpty(input.value))
    //    {
    //        var msgHint = _parent.FindInstance(WindowType.MSG_HINT, true) as MessageHint;
    //        msgHint.AddMessageHint("You need to type some text before to send message.");
    //        return;
    //    }
    //
    //    // Get Lobby Stage.
    //    var lobbyStage = _parent.Stage as LobbyStage;
    //
    //    switch(_currentType)
    //    {
    //        case ChatType.NORMAL:
    //            {
    //                lobbyStage.ServerConnection.Send(new CL_CHAT_REQ()
    //                {
    //                    uid = lobbyStage.MainPlayer.Index,
    //                    type = _currentType,
    //                    msg = input.value
    //                });
    //            }
    //            break;
    //        case ChatType.WHISPER:
    //            {
    //                UIInput nameInput = _nameWhisperInput.GetComponentInChildren<UIInput>();
    //                if (nameInput == null)
    //                {
    //                    CLog.F("Name Input component is null.");
    //                    return;
    //                }
    //
    //                if (string.IsNullOrEmpty(nameInput.value))
    //                {
    //                    var msgHint = _parent.FindInstance(WindowType.MSG_HINT, true) as MessageHint;
    //                    msgHint.AddMessageHint("You need to type name of the player want to send whisper.");
    //                    return;
    //                }
    //
    //                lobbyStage.ServerConnection.Send(new CL_CHAT_WHISPER_REQ()
    //                {
    //                    uid = lobbyStage.MainPlayer.Index,
    //                    toName = nameInput.value,
    //                    msg = input.value
    //                });
    //            }
    //            break;
    //    }
    //
    //    // Reset message from input.
    //    input.value = "";
    //}
    //
    //public void ChangeChatNormalType()
    //{
    //    ChangeChatType(ChatType.NORMAL);
    //}
    //
    //public void ChangeChatWhisperType()
    //{
    //    ChangeChatType(ChatType.WHISPER);
    //}
    //
    //public void ChangeChatSystemType()
    //{
    //    ChangeChatType(ChatType.SYSTEM);
    //}
    //
    //private void ChangeChatType(ChatType type)
    //{
    //    // Ignore if new chat type is current one.
    //    if (_currentType == type)
    //        return;
    //
    //    if(type == ChatType.WHISPER)
    //    {
    //        _normalInput.SetActive(false);
    //
    //        _whisperInput.SetActive(true);
    //        _nameWhisperInput.SetActive(true);
    //    }
    //    else
    //    {
    //        if(_currentType == ChatType.WHISPER)
    //        {
    //            _normalInput.SetActive(true);
    //
    //            _whisperInput.SetActive(false);
    //            _nameWhisperInput.SetActive(false);
    //        }
    //    }
    //
    //    // Define new chat type selected.
    //    _currentType = type;
    //
    //    // Clear messages from old chat type in text area.
    //    _textMessage.Clear();
    //
    //    // Get old messages from current chat type.
    //    List<string> messages = null;
    //    if (_historyChat.TryGetValue(type, out messages))
    //    {
    //        foreach(var msg in messages)
    //        {
    //            _textMessage.Add(msg);
    //        }
    //    }
    //}
    //
    //public void AddText(ChatType type, string msg)
    //{
    //    if (_currentType == type)
    //    {
    //        _textMessage.Add(msg);
    //    }
    //}
    //
    //public void AddWhisperText(string toName, string fromName, string msg)
    //{
    //    // It's a good idea to strip out all symbols as we don't want user input to alter colors, add new lines, etc
    //    string text = NGUIText.StripSymbols(msg);
    //
    //    text = string.Format("[E242F4][{0}] to [{1}]: {2}[-]", toName, fromName, text);
    //
    //    if (_currentType == ChatType.WHISPER)
    //    {
    //        _textMessage.Add(text);
    //    }
    //}
}
