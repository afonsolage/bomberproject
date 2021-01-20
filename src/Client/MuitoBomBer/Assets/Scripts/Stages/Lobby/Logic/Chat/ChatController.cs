using CommonLib.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatController
{
    private readonly int MAX_KEEP_HISTORY_MESSAGE = 100;
    private readonly int MAX_REMOVE_HISTORY_MESSAGE = 10;

    private LobbyStage _stage;
    public LobbyStage Stage { get { return _stage; } }

    private Dictionary<ChatType, List<ChatData>> _historyChat = new Dictionary<ChatType, List<ChatData>>();

    public class ChatData
    {
        public ulong playerIndex;
        public string name;
        public string message;
        public bool selfMsg;

        public ChatData(string name, string msg)
        {
            this.name = name;
            message = msg;
        }

        public ChatData(ulong index, string name, string msg, bool self)
        {
            playerIndex = index;
            this.name = name;
            message = msg;
            selfMsg = self;
        }
    }

    public ChatController(LobbyStage stage)
    {
        _stage = stage;

        // Initilization of list history chat.
        for (var type = ChatType.GENERAL; type < ChatType.MAX; type++)
            _historyChat.Add(type, new List<ChatData>());
    }

    private void AddHistoryMessage(ChatType type, ChatData data)
    {
        // Save all messages in GENERAL
        List<ChatData> messages = null;
        if (_historyChat.TryGetValue(ChatType.GENERAL, out messages))
        {
            // Verify that history has already reached the maximum number of saved messages. 
            // If yes, always delete the first message from the list.
            if (messages.Count >= MAX_KEEP_HISTORY_MESSAGE)
                messages.RemoveRange(0, MAX_REMOVE_HISTORY_MESSAGE);

            // Save message.
            messages.Add(data);
        }

        // Check if the current type you wanted to save is not General since we've added it before.
        if (type == ChatType.GENERAL)
            return;

        // Now save the message in the specific type.
        messages = null;
        if (_historyChat.TryGetValue(type, out messages))
        {
            // Verify that history has already reached the maximum number of saved messages. 
            // If yes, always delete the first 10 message from the list.
            if (messages.Count >= MAX_KEEP_HISTORY_MESSAGE)
                messages.RemoveRange(0, MAX_REMOVE_HISTORY_MESSAGE);

            // Save message.
            messages.Add(data);
        }
    }

    public List<ChatData> GetHistoryMessage(ChatType type)
    {
        // Check if is an unknown chat type.
        if (type >= ChatType.MAX)
            return null;

        // Get all messages of current type.
        List<ChatData> messages = null;
        if (_historyChat.TryGetValue(type, out messages))
            return messages;

        // Not find history message of type.
        return null;
    }

    public string ProcessText(ChatType type, string playerName, string msg)
    {
        // It's a good idea to strip out all symbols as we don't want user input to alter colors, add new lines, etc
        string message = NGUIText.StripSymbols(msg);

        switch (type)
        {
            case ChatType.NORMAL:   return string.Format("[FFFFFF][{0}][{1}]: [FFFFFF]{2}[-]", "Normal", playerName, message);
            case ChatType.WHISPER:  return string.Format("[{0}][E242F4][{1}]: {2}[-]", "Whisper", playerName, message);
            case ChatType.SYSTEM:   return string.Format("[fff400][{0}]: {1}[-]", "System", message);
            default:                return string.Format("[{0}][EF9037][{1}]: [FFFFFF]{2}[-]", "Normal", playerName, message);
        }
    }

    public void AddMessage(ChatType type, string msg)
    {
        string name = string.Empty;
        if (type == ChatType.SYSTEM)
            name = "System";

        // Save this message in history.
        AddHistoryMessage(type, new ChatData(name, msg));

        // If player has Chat Window active, then let to add message.
        var chatWindow = Stage.UIManager.FindInstance(WindowType.CHAT) as ChatWindow;
        if (chatWindow)
        {
            chatWindow.AddMessage(type, msg);
        }

        // Add message in Main Window.
        var mainWindow = Stage.UIManager.FindInstance(WindowType.MAIN) as MainWindow;
        if (mainWindow)
        {
            mainWindow.AddMessage(ProcessText(type, "", msg));
        }
    }

    public void AddMessage(ChatType type, ulong playerIndex, string playerName, string msg, bool self = false)
    {
        // Save this message in history.
        AddHistoryMessage(type, new ChatData(playerIndex, playerName, msg, self));

        // If player has Chat Window active, then let to add message.
        var chatWindow = Stage.UIManager.FindInstance(WindowType.CHAT) as ChatWindow;
        if(chatWindow)
        {
            chatWindow.AddMessage(type, playerIndex, playerName, msg, self);
        }

        // Add message in Main Window.
        var mainWindow = Stage.UIManager.FindInstance(WindowType.MAIN) as MainWindow;
        if(mainWindow)
        {
            mainWindow.AddMessage(ProcessText(type, playerName, msg));
        }
    }
}
