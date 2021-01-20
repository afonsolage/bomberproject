using CommonLib.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class Friend
{
    //TODO: Add more friend info
    public string Nick { get; private set; }
    public FriendState State { get; set; }

    public Friend (string nick)
    {
        Nick = nick;
    }

    public override bool Equals(object obj)
    {
        var friend = obj as Friend;
        return friend != null &&
               Nick == friend.Nick;
    }

    public override int GetHashCode()
    {
        return -1485547842 + EqualityComparer<string>.Default.GetHashCode(Nick);
    }
}
