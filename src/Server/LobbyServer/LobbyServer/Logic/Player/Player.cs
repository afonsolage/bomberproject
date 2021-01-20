using CommonLib.Messaging;
using CommonLib.Messaging.Client;
using CommonLib.Messaging.Common;
using CommonLib.Util;
using LobbyServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LobbyServer.Logic
{
    class Friend
    {
        public ulong index;
        public string login;
        public string nick;
        public FriendState state;
    }

    class Player : IEquatable<Player>
    {
        /// <summary>
        /// Index of Player.
        /// </summary>
        protected readonly ulong _index;
        public ulong Index { get => _index; }

        /// <summary>
        /// Nickname of Player.
        /// </summary>
        protected string _nick;
        public string Nick { get => _nick; set { _nick = value; } }

        /// <summary>
        /// Gender of Player.
        /// </summary>
        protected PlayerGender _gender;
        public PlayerGender Gender { get => _gender; set { _gender = value; } }

        private PlayerStage _stage;
        //public PlayerStage Stage { get => _stage; set => _stage = value; }
        public PlayerStage Stage
        {
            get { return _stage; }
            set
            {
                _stage = value;

                // Nfy for anothers players new current stage.
                SendCurrentStage();
            }
        }

        private uint _level;
        public uint Level
        {
            get { return _level; }
        }

        private ulong _experience;
        public ulong Experience
        {
            get { return _experience; }
        }

        protected Room _room;
        public Room Room { get => _room; set => _room = value; }

        protected bool _isDummy;
        public bool IsDummy { get => _isDummy; set => _isDummy = value; }

        protected ClientSession _session;
        public ClientSession Session { get => _session; }

        #region PING
        /// <summary>
        /// Current ping ticks retrieved after request to server.
        /// </summary>
        private long _currentPingTicks = 0;
        public long CurrentPingTicks { get { return _currentPingTicks; } }

        /// <summary>
        /// Received ping time.
        /// </summary>
        private float _currentPingTime = 0;

        /// <summary>
        /// Current ping.
        /// </summary>
        private long _currentPing = 0;
        public long Ping { get => _currentPing; set => _currentPing = value; }

        /// <summary>
        /// Ping time checker in seconds.
        /// </summary>
        private const long _pingTimeCheck = 10;
        #endregion

        protected List<Friend> _friends;
        public List<Friend> Friends { get => _friends; }

        public long TimeoutTicks { get; protected set; }

        protected bool _ready;
        public bool Ready
        {
            get => _ready;
            set
            {
                if (_ready == value)
                    return;

                _ready = value;

                OnReady(_ready);
            }
        }

        protected void OnReady(bool ready)
        {
            Room?.Broadcast(new CL_PLAYER_READY_NFY() { index = _index, ready = ready });
        }

        internal void SetFriends(List<Friend> friendList)
        {
            _friends = friendList;
            try
            {
                _session.App.PlayerController.UpdateFriendsState(this);
            }
            catch (Exception e)
            {
                CLog.Catch(e);
            }
        }

        public bool IsOffline { get => _session == null; }

        /// <summary>
        /// Constructor of Player.
        /// </summary>
        /// <param name="index">Index of Player.</param>
        /// <param name="nick">Nick/Name of Player.</param>
        /// <param name="sex">Gender of Player.</param>
        /// <param name="session">Current client session of Player.</param>
        /// <param name="stage">Current stage of Player.</param>
        public Player(ulong index, string nick, PlayerGender sex, uint level, ulong experience, ClientSession session, PlayerStage stage)
        {
            _index = index;
            _nick = nick;
            _gender = sex;

            _level = level;
            _experience = experience;

            _session = session;

            _session.Player = this;

            Ready = false;
            Stage = stage;

            IsDummy = false;

            Ping = 0;

            // Force to ping after the player has started.
            // So don't have to wait for the time to calculate the first ping.
            _currentPingTime += _pingTimeCheck;

            _friends = new List<Friend>();
        }

        public void Tick(float delta)
        {
            if (IsOffline) return;

            #region PING
            _currentPingTime += delta;
            if (_currentPingTime >= _pingTimeCheck)
            {
                _currentPingTime -= _pingTimeCheck;
                _currentPingTicks = DateTime.UtcNow.Ticks;

                HeartBeatReq();
            }
            #endregion
        }

        /// <summary>
        /// This function sends initial data to player. Note that this function may be called on first connect of player and also when player reconnects, so his object maybe arround for some time.
        /// </summary>
        internal void SendInitialData()
        {
            //Request additional data from DB
            _session.App.PlayerController.RequestAdditionalInfo(this);

            // Send main player info
            _session.Send(new CL_MAIN_PLAYER_INFO_NFY()
            {
                player = new PLAYER_INFO()
                {
                    index = _index,
                    nick = _nick,
                    gender = _gender,
                    level = _level,
                    experience = _experience,
                    stage = _stage,
                    state = IsOffline ? PlayerState.Offline : _ready ? PlayerState.Ready : PlayerState.NotReady,
                    roomIndex = _room?.Index ?? 0,
                    roomSlotIndex = _room?.FindPlayerSlot(_index).SlotIndex ?? -1,
                    ping = Ping,
                }
            });

            // Send existing room info
            _session.Send(new CL_ROOM_LIST_NFY()
            {
                //We don't send info about rooms that is already playing
                rooms = Session.App.RoomController.ListAllRooms().Where(r => !r.IsPlaying).Select(r => new ROOM_INFO()
                {
                    index = r.Index,
                    mapId = r.MapId,
                    maxPlayer = r.MaxPlayer,
                    name = r.Title,
                    owner = r.Owner.Nick,
                    playerCnt = r.PlayerCnt,
                    stage = r.Stage,
                    password = r.Password,
                    isPublic = (!string.IsNullOrEmpty(r.Password)) ? true : false,
                }).ToList(),
            });

            if (_room != null)
            {
                // Send info about existing players on room.
                var players = _room.ListAllPlayers();

                foreach (var existingPlayer in players)
                {
                    _session.Send(new CL_PLAYER_JOINED_NFY()
                    {
                        roomIndex = _room.Index,
                        player = new PLAYER_INFO()
                        {
                            index = existingPlayer.Index,
                            nick = existingPlayer.Nick,
                            level = existingPlayer.Level,
                            experience = existingPlayer.Experience,
                            state = existingPlayer.IsOffline ? PlayerState.Offline : existingPlayer.Ready ? PlayerState.Ready : PlayerState.NotReady,
                            ping = existingPlayer.Ping,
                            roomIndex = existingPlayer.Room.Index,
                            roomSlotIndex = existingPlayer.Room?.FindPlayerSlot(existingPlayer.Index).SlotIndex ?? -1,
                        },
                    });
                }

                if (Stage == PlayerStage.Playing)
                {
                    // Tell player the match already started
                    _session.Send(new CL_ROOM_START_NFY()
                    {
                        roomIndex = Room.RoomIndex,
                        serverInfo = new SERVER_INFO() //TODO: Find a better way of getting Room Server Info.
                        {
                            address = "127.0.0.1",
                            port = 9876,
                        }
                    });
                }
                else if (Stage == PlayerStage.Room)
                {
                    // Send info about what players are ready
                    foreach (var existingPlayer in players)
                    {
                        if (!existingPlayer.Ready)
                            continue;

                        _session.Send(new CL_PLAYER_READY_NFY()
                        {
                            index = existingPlayer.Index,
                            ready = true,
                        });
                    }
                }
            }

            //TODO: Add others initial infos, like friend, items, achivements, etc
        }

        /// <summary>
        /// Reconnect the existing player object with the given session.
        /// </summary>
        /// <param name="session"></param>
        internal void Reconnect(ClientSession session)
        {
            _session = session;
            _session.Player = this;
            TimeoutTicks = 0;

            SendInitialData();

            if (_room != null)
            {
                _room.Broadcast(new CX_PLAYER_ONLINE_NFY()
                {
                    index = _index,
                });
            }

            //TODO: Send state packages, like if the player is on some room or if it was on some UI like shop.

            InfoSendEnd();
        }

        internal void InfoSendEnd()
        {
            _session?.Send(new CL_INFO_END_NFY());
        }

        internal void DestroySessionOnly()
        {
            _session.Player = null;
            _session = null;
            _ready = false;
            TimeoutTicks = DateTime.Now.Ticks;
        }

        internal void Destroy()
        {
            if (_room != null)
                _room.Leave(this);

            _room = null;

            if (_session != null)
                _session.Player = null;
        }

        public override bool Equals(object other)
        {
            return other is Player && Equals(other as Player);
        }

        public bool Equals(Player other)
        {
            return _index == other._index;
        }

        public override int GetHashCode()
        {
            return 894489890 + _index.GetHashCode();
        }

        public void Disconnect()
        {
            Session.Send(new CX_DISCONNECTED_NFY() { });
            Session.Stop();
        }

        public override string ToString()
        {
            return string.Format("[{0}]({1} - {2}) {3} - {4} ({5}{6})", Index, Session?.Login ?? "--", Session?.ID ?? 0, Nick, Stage, Room?.Index, (Room != null && Ready) ? " Ready" : "");
        }

        internal virtual void OnFinishMatch()
        {
            // Not use the Setter version (Ready) to avoid sending packeages, since when this method is called, no player will be on room (yet).
            _ready = false;

            Stage = PlayerStage.Room;

            // Refresh the timeout ticks to give some time to player reconnect, after room match has finished
            TimeoutTicks = DateTime.Now.Ticks;
        }

        internal void SendCurrentStage()
        {
            _session?.App.PlayerController.Broadcast(new CL_PLAYER_STAGE_NFY()
            {
                player = new PLAYER_INFO()
                {
                    index = _index,
                    nick = _nick,
                    gender = _gender,
                    level = _level,
                    experience = _experience,
                    stage = _stage,
                    roomIndex = _room?.Index ?? 0,
                    roomSlotIndex = _room?.FindPlayerSlot(_index).SlotIndex ?? -1,
                    ping = Ping
                }
            }/*, PlayerStage.Lobby*/);
        }

        public void HeartBeatReq()
        {
            _session?.Send(new CL_PLAYER_HEARTBEAT_REQ() { });
        }

        private void AddExp(ulong addExp)
        {
            if (addExp == 0)
                return;

            ulong totalExperience = _experience;

            if(!_session.App.ResourceController.GetExpTable(_level, out ulong experienceTable))
            {
                CLog.E("This should not happen");
                return;
            }

            if (!_session.App.ResourceController.GetNextExpTable(_level + 1))
            {
                _experience = (experienceTable <= totalExperience + addExp) ? experienceTable - 1 : _experience + addExp;
                _session?.Send(new CL_PLAYER_EXPERIENCE_RES() { newExperience = _experience });
            }
            else
            {
                totalExperience += addExp;

                ulong maxExperience = experienceTable;
                uint levelUp = 0;

                while(totalExperience >= maxExperience)
                {
                    totalExperience -= maxExperience;

                    ++levelUp;

                    if (!_session.App.ResourceController.GetExpTable(_level + levelUp, out experienceTable))
                    {
                        CLog.E("This should not happen");
                        return;
                    }

                    maxExperience = experienceTable;
                }

                if(levelUp > 0)
                {
                    LevelUp(levelUp, totalExperience);
                }
                else
                {
                    _experience = totalExperience;
                    _session?.Send(new CL_PLAYER_EXPERIENCE_RES() { newExperience = _experience });
                }
            }
        }

        private void LevelUp(uint addLevel, ulong totalExperience)
        {
            uint sumLevel = _level + addLevel;
            if(_session.App.ResourceController.MaxLevel < sumLevel)
            {
                CLog.E("This should not happen");
                return;
            }

            _level = sumLevel;
            _experience = totalExperience;

            _session?.Send(new CL_PLAYER_LEVEL_RES() { newLevel = _level, newExperience = _experience });
        }
    }
}
