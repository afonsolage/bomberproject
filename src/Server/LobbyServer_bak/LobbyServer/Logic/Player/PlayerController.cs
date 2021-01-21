using CommonLib.Messaging;
using CommonLib.Messaging.Client;
using CommonLib.Messaging.Common;
using CommonLib.Messaging.DB;
using CommonLib.Server;
using CommonLib.Util;
using LobbyServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LobbyServer.Logic
{
    internal partial class PlayerController : ITickable
    {
        private readonly List<Player> _players;

        private readonly AppServer _app;
        public AppServer App { get => _app; }

        public string Name => "PlayerController";

        internal PlayerController(AppServer app)
        {
            _app = app;
            _players = new List<Player>();

            // Runs this once per second is enough
            app.Register(this, 1);
        }

        internal Player Create(ulong index, string nick, PlayerGender sex, uint level, ulong experience, ClientSession session, PlayerStage stage)
        {
            var player = new Player(index, nick, sex, level, experience, session, stage);

            lock (_players)
            {
                _players.Add(player);
            }

            if (stage != PlayerStage.Creating)
            {
                player.SendInitialData();
                player.InfoSendEnd();
            }

            return player;
        }

        internal void Destroy(Player player)
        {
            lock (_players)
            {
                _players.RemoveAll((p) => p.Index == player.Index);
            }

            player.Destroy();
        }


        /// <summary>
        /// This method is called when the player is "ready" and connected to game, not just a socket connection.
        /// </summary>
        /// <param name="player"></param>
        internal void OnConnect(Player player)
        {
            foreach (var friendInfo in player.Friends)
            {
                if (friendInfo.state != FriendState.Online)
                    continue;

                var friend = Find(friendInfo.nick);

                if (friend == null)
                    continue;

                friend.Session.Send(new CL_FRIEND_ONLINE_NFY()
                {
                    nick = player.Nick,
                });
            }
        }

        internal void OnDisconnect(Player player)
        {
            if (player.Session?.RemoteDisconnection ?? false)
            {
                CLog.D("Player {0} was remote disconnected. Keep his object until he reconnects.", player.Nick);

                // This means the player disconnected to join the RoomServer. Let's keep his object active until he reconnects.
                player.DestroySessionOnly();

                if (player.Room != null)
                {
                    player.Room.Broadcast(new CX_PLAYER_OFFLINE_NFY()
                    {
                        index = player.Index,
                    });
                }

                foreach (var friendInfo in player.Friends)
                {
                    if (friendInfo.state != FriendState.Online)
                        continue;

                    var friend = Find(friendInfo.nick);

                    if (friend == null)
                        continue;

                    friend.Session.Send(new CL_FRIEND_OFFLINE_NFY()
                    {
                        nick = player.Nick,
                    });
                }
            }
            else
            {
                Destroy(player);
            }
        }

        internal Player Find(ulong index)
        {
            lock (_players)
            {
                return _players.Find((p) => p.Index == index);
            }
        }

        internal Player Find(string nick)
        {
            lock (_players)
            {
                return _players.Find((p) => p.Nick.ToLower() == nick.ToLower());
            }
        }

        internal void Broadcast<T>(T msg) where T : IMessage
        {
            lock (_players)
            {
                foreach (var player in _players)
                {
                    if (player.IsOffline)
                        continue;

                    player.Session.Send(msg);
                }
            }
        }

        internal void Broadcast<T>(T msg, PlayerStage stage) where T : IMessage
        {
            lock (_players)
            {
                foreach (var player in _players)
                {
                    if (player.IsOffline || player.Stage != stage)
                        continue;

                    player.Session.Send(msg);
                }
            }
        }

        internal List<Player> ListAllPlayers()
        {
            var res = new List<Player>();

            lock (_players)
            {
                res.AddRange(_players);
            }

            return res;
        }

        internal void RemoveOrphanObjects()
        {
            var now = DateTime.Now.Ticks;

            if (!long.TryParse(App.GetGlobalConfig("playerReconnectTimeout", "60"), out var timeout))
            {
                timeout = 60;
            }

            // Convert seconds to ticks
            timeout *= TimeSpan.TicksPerSecond;

            List<Player> playersToRemove;
            lock (_players)
            {
                playersToRemove = _players.Where(p => p.Stage != PlayerStage.Playing && p.TimeoutTicks > 0 && p.TimeoutTicks + timeout <= now).ToList();

                if (playersToRemove == null || playersToRemove.Count == 0)
                    return;

                foreach (var player in playersToRemove)
                {
                    CLog.D("Removing player {0} due to reconnect timeout.", player.Nick);
                    player.Destroy();
                    _players.Remove(player);
                }
            }
        }

        public void Tick(float delta)
        {
            RemoveOrphanObjects();

            // Process Ticks in each player.
            lock (_players)
            {
                foreach (var player in _players)
                {
                    player?.Tick(delta);
                }
            }
        }

        internal void RequestAdditionalInfo(Player player)
        {
            App.DBClient.Send(new DL_PLAYER_ADD_INFO_REQ()
            {
                index = player.Index,
            });
        }

        internal int Count()
        {
            lock (_players)
            {
                return _players.Count;
            }
        }

        internal void UpdateFriendsState(Player player, bool notifyClient = true)
        {
            lock (_players)
            {
                // Only accepted friends that are online
                var onlineFriends = player.Friends
                    .Where(f => (f.state == FriendState.Online || f.state == FriendState.Offline) && _players.Find(p => f.index == p.Index && !p.IsOffline) != null);

                foreach (var onlineFriend in onlineFriends)
                {
                    onlineFriend.state = FriendState.Online;
                }
            }

            if (notifyClient)
            {
                player.Session.Send(new CL_FRIEND_INFO_NFY()
                {
                    // IMPORTANT! Never send the login of friends to client.
                    friends = player.Friends.Select(f => new FRIEND_INFO() { index = f.index, nick = f.nick, state = f.state }).ToList(),
                });
            }
        }

        internal void AddFriend(Player player, ulong index, string login, string nick, FriendState state = FriendState.Requested, bool addAlsoOnFriend = true)
        {
            player.Friends.Add(new Friend()
            {
                index = index,
                login = login,
                nick = nick,
                state = state,
            });

            UpdateFriendsState(player);

            if (!addAlsoOnFriend)
                return;

            // Add also on friend
            var friend = Find(nick);
            if (friend != null && !friend.IsOffline)
            {
                AddFriend(friend, player.Index, player.Session.Login, player.Nick, FriendState.WaitingApproval, false);
            }
        }

        internal void RemoveFriend(Player player, string nick, bool removeAlsoOnFriend = true)
        {
            player.Friends.RemoveAll(f => f.nick == nick);
            UpdateFriendsState(player);

            if (!removeAlsoOnFriend)
                return;

            // Remove also from friend
            var friend = Find(nick);
            if (friend != null && !friend.IsOffline)
            {
                RemoveFriend(friend, player.Nick, false);
            }
        }

        internal void AcceptFriend(Player player, string requester, bool acceptAlsoOnFriend = true)
        {
            var friend = player.Friends.Find(f => f.nick == requester);

            if (friend != null)
            {
                friend.state = FriendState.Offline;
            }

            UpdateFriendsState(player);

            if (!acceptAlsoOnFriend)
                return;

            var friendPlayer = Find(requester);
            if (friendPlayer != null && !friendPlayer.IsOffline)
            {
                AcceptFriend(friendPlayer, player.Nick, false);
            }
        }

    }
}
