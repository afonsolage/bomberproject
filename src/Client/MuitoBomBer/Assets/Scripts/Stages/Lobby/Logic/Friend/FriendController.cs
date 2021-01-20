using CommonLib.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class FriendController
{
    private LobbyStage _stage;
    public LobbyStage Stage { get { return _stage; } }

    private readonly List<Friend> _friends;
    public List<Friend> Friends { get { return _friends; } }

    public FriendController(LobbyStage stage)
    {
        _stage = stage;
        _friends = new List<Friend>();
    }

    public void SetFriends(List<Friend> friends, bool refreshUI = true)
    {
        _friends.Clear();
        _friends.AddRange(friends);

        if (refreshUI)
        {
            RefreshFriendsUI();
        }
    }

    public void UpdateFriendState(string nick, FriendState state, bool refreshUI = true)
    {
        var friend = _friends.Find(f => f.Nick == nick);

        if (friend != null)
        {
            friend.State = state;
        }

        if (refreshUI)
        {
            RefreshFriendsUI();
        }
    }

    public void RefreshFriendsUI()
    {
        var friendWindow = _stage.UIManager.FindInstance(WindowType.FRIEND) as FriendWindow;

        if (friendWindow != null)
        {
            friendWindow.UpdateState();
        }
    }
}
