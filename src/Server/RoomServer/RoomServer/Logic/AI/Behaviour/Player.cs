using CommonLib.GridEngine;
using CommonLib.Util;
using CommonLib.Util.Math;
using FluentBehaviourTree;
using RoomServer.Logic.Behaviour.Map;
using RoomServer.Logic.Object;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomServer.Logic.AI.Behaviour
{
    internal enum PlayerAISkill
    {
        Beginner,
        Talented,
        Experienced,
        Expert,

        Max
    }

    internal enum TargetType
    {
        None,
        SafePlace,
        ForceSafePlace,
        RandomPlace,
        Block,
        Player,
        Powerup,
    }

    internal class PlayerBehaviour
    {
        protected const int FAIL_ATTEMPTS = 20;
        protected const int SUCCESS_ATTEMPTS = 20;

        //Use random direction order to avoid repeated patterns
        protected readonly Vec2[][] RND_DIRS = new Vec2[][]
        {
            new Vec2[] { Vec2.UP, Vec2.RIGHT, Vec2.DOWN, Vec2.LEFT },
            new Vec2[] { Vec2.RIGHT, Vec2.DOWN, Vec2.LEFT, Vec2.UP },
            new Vec2[] { Vec2.LEFT, Vec2.UP, Vec2.RIGHT, Vec2.DOWN },
            new Vec2[] { Vec2.DOWN, Vec2.LEFT, Vec2.UP, Vec2.RIGHT },
        };

        //TODO: Add this on a settings file.
        //Each value represents the rate or interval according to the difficulty level, ranging from left to right.
        protected readonly float[] LAZYNESS = new float[] { 1.5f, 0.8f, 0.1f, 0 };
        protected readonly float[] CHECK_WAYPOINT_INTERVAL = new float[] { 2f, 1.7f, 1.3f, 1f };
        protected readonly float[] AGGRO_RATE = new float[] { 0.2f, 0.4f, 0.6f, 0.8f };
        protected readonly int[] VISION_RANGE = new int[] { 3, 5, 7, 9 };

        protected float _lazyness;
        protected float _checkWaypointInterval;
        protected float _aggroRate;
        protected int _visionRange;

        protected readonly Player _player;
        protected readonly IBehaviourTreeNode _root;
        protected readonly PlayerAISkill _skill;

        protected Dictionary<Vec2, long> _explosions;
        protected List<Bomb> _bombs;

        //Fast get utlity
        protected GridMap Map { get => _player.Map; }
        protected Room Room { get => _player.Map as Room; }
        protected Vec2 GridPos { get => _player.GridPos; }
        protected Vec2f WorldPos { get => _player.WorldPos; }

        protected Random _rnd = new Random();

        /// <summary>
        /// The target to go. This isn't the next place to go, is a long term target
        /// </summary>
        protected Vec2 _targetWaypoint = Vec2.INVALID;

        protected float _checkBombInterval = 0.1f;
        protected bool _placeBombAtTarget;
        protected bool _justPlacedBomb;
        protected TargetType _targetType;

        protected bool _kickBombAtTarget;

        protected long _lastSetTarget;
        protected long _nextWalkAround;
        protected long _elapsedTime;

        /// <summary>
        /// This list holds places that we already check and we can't go for. Should be cleared whenever a new target is set.
        /// </summary>
        protected List<Vec2> _exceptBlockPlaces = new List<Vec2>();

        protected float _checkBombAccum;
        protected float _checkWaipointAccum;
        protected Dictionary<Vec2, long> _hurryUp;

        public PlayerBehaviour(Player player, PlayerAISkill skill = PlayerAISkill.Experienced)
        {
            _player = player;
            _skill = skill;
            _root = BuildTree();

            _bombs = new List<Bomb>();
            _explosions = new Dictionary<Vec2, long>();

            SetupSkillParams();

            if (Room == null || Room.Behaviour == null)
                Debugger.Break();

            if (Room.Behaviour.HasBehaviour<ClassicBehaviour>())
            {
                var behaviour = Room.Behaviour.GetBehaviour<ClassicBehaviour>();
                _hurryUp = behaviour.GetHurryUpPlaces();
            }
        }

        protected void SetupSkillParams()
        {
#if _DEBUG
            var min = new List<int>() {
                LAZYNESS.Length,
                CHECK_WAYPOINT_INTERVAL.Length,
                AGGRO_RATE.Length,
                VISION_RANGE.Length
            }.Min();

            Debug.Assert(min == (int)PlayerAISkill.Max, "There should be the same amount of items on arrays as numbers of skills");
#endif
            _lazyness = LAZYNESS[(int)_skill];
            _checkWaypointInterval = CHECK_WAYPOINT_INTERVAL[(int)_skill];
            _aggroRate = AGGRO_RATE[(int)_skill];
            _visionRange = VISION_RANGE[(int)_skill];
        }

        protected void SetTarget(Vec2 target, TargetType type)
        {
            _targetWaypoint = target;
            _targetType = type;
            _lastSetTarget = _elapsedTime;

            _exceptBlockPlaces.Clear();

            _placeBombAtTarget = false;

            _nextTryCalcWaypoint = 0;
        }

        public void Tick(float deltaTime)
        {
            if (!_player.IsLive)
                return;

            _elapsedTime += (long)(deltaTime * TimeSpan.TicksPerSecond);
            _root.Tick(new TimeData(deltaTime));
        }

        #region BEHAVIOUR TREE
        protected IBehaviourTreeNode BuildTree()
        {
            return new BehaviourTreeBuilder()
                //All children actions will run in parallel
                .Parallel("Root", FAIL_ATTEMPTS, SUCCESS_ATTEMPTS)
                    //Find a place to go.
                    .Selector("Find Next Waypoint")
                        .Condition("Should we check next waypoint", t => ShouldCheckWaypoint(t))
                        .Do("Lazyiness", t => DoLayziness(t))
                        //If we are on a place where we'll be hit, leave from here
                        .Sequence("Risk Area")
                            .Condition("Should Leave This Place", t => CheckLeaveThisPlace())
                            .Do("Find Safe Place", t => FindSafePlace())
                        .End()
                        .Condition("Should we find another waypoint", t => !ShouldChangeWaypoint(t))
                        //If there is some player near, let's try to kill him
                        .Sequence("Try to kill player")
                            .Condition("Should we try to kill a player", t => CheckAggro())
                            .Do("Try to lock'n'kill player", t => TryToLockNKillPlayer())
                        .End()
                        //Else if there is some powerup near, let's go and get it
                        .Sequence("Pick Power-ups")
                            .Do("Goto near power-up", t => GotoNearestPowerup())
                        .End()
                        //Else let's destroy some block
                        .Sequence("Destroy Blocks")
                            .Do("Goto block place and place bomb", t => GotoNearestBlock())
                        .End()
                        //Else just walk around
                        .Sequence("Walk Around")
                            .Condition("Should we try to kill a player", t => CheckWalkAround())
                            .Do("Just walk around", t => FindRandomSafePlace())
                        .End()
                        //Else sitdown and relax
                        .Sequence("Stand")
                            .Do("Just walk around", t => JustStand())
                        .End()
                    .End()

                    .Sequence("Kick")
                        .Do("Kick bomb", t => KickBomb())
                    .End()

                    //Place bomb if necessary and detect all bombs whitin our vision range
                    .Sequence("Bombs work")
                        .Do("Place bomb", t => PlaceBomb())
                        .Condition("Should we detect bombs now", t => ShouldWeDetectBombs(t))
                        .Do("Find near bombs", t => FindNearBombs())
                        .Do("Calculate explosions", t => CalcExplosions())
                    .End()

                    .Do("Walk to Waypoint", t => WalkToWaypoint(t))
                .End()
                .Build();
        }

        private float _lazynessAccum;
        private float _nextLazyness;
        private BehaviourTreeStatus DoLayziness(TimeData t)
        {
            if (_lazynessAccum < _nextLazyness)
            {
                _lazynessAccum += t.deltaTime;
                return BehaviourTreeStatus.Running;
            }

            _nextLazyness = _rnd.Next(0, (int)(_lazyness * 1000)) / 1000f;
            _lazynessAccum = 0;
            return BehaviourTreeStatus.Failure; //We need to return failure, else the Selector won't run another ones
        }

        private BehaviourTreeStatus JustStand()
        {
            SetTarget(Vec2.INVALID, TargetType.None);

            return BehaviourTreeStatus.Success;
        }

#if _DEBUG
        private long _begin;
#endif
        private void PrepareBench()
        {
#if _DEBUG
            _begin = DateTime.Now.Ticks;
#endif
        }

        private void DoneBench(string place)
        {
#if _DEBUG
            var elapsed = DateTime.Now.Ticks - _begin;

            var ms = elapsed / TimeSpan.TicksPerMillisecond;

            if (ms > 1)
            {
                CLog.W("The action {0} took {1}ms", place, ms);
            }
        }
#endif

        private bool ShouldCheckWaypoint(TimeData t)
        {
            _checkWaipointAccum += t.deltaTime;

            if (_checkWaipointAccum < _checkWaypointInterval)
                return false;

            _checkWaipointAccum = 0;

            return true;
        }

        private bool ShouldChangeWaypoint(TimeData t)
        {
            if (!_targetWaypoint.IsValid())
                return true;

            if (_placeBombAtTarget && _targetType != TargetType.Player) //Players keep moving, so we should be able to quick change target place
                return false;

            if (_elapsedTime - _lastSetTarget < TimeSpan.TicksPerSecond) //1 second
                return false;

            return true;
        }

        private BehaviourTreeStatus KickBomb()
        {
            if (_kickBombAtTarget && GridPos == _targetWaypoint
                && (WorldPos - Map.GridToWorld(GridPos)).Magnitude() < 0.1f)
            {

                TryToKickBomb();
                _kickBombAtTarget = false;
            }

            return BehaviourTreeStatus.Success;
        }

        private BehaviourTreeStatus PlaceBomb()
        {
            if (_placeBombAtTarget && GridPos == _targetWaypoint
                && (WorldPos - Map.GridToWorld(GridPos)).Magnitude() < 0.1f
                && _player.PlaceBomb())
            {
                AddExplosions(new Bomb(Map, _player)
                {
                    GridPos = GridPos,
                }, _player.Attr.bombArea);

                _placeBombAtTarget = false;
                _checkBombAccum = 0;
                _justPlacedBomb = true;
            }

            return BehaviourTreeStatus.Success;
        }

        /// <summary>
        /// The next position to walk in order to reach the _targetWaypoint. This _nextPos is one node on path to reach _targetWaypoint
        /// </summary>
        protected Vec2f _nextPos = Vec2f.INVALID;
        protected List<Vec2> _path = new List<Vec2>();
        protected float _nextTryCalcWaypoint;

        private BehaviourTreeStatus WalkToWaypoint(TimeData t)
        {
            try
            {
                PrepareBench();
                if (!_nextPos.IsValid())
                {
                    _nextPos = WorldPos;
                }
                else if (_nextPos != WorldPos)
                {
                    //Walk to _nextPos using the delta time.
                    var dist = _nextPos - WorldPos;

                    var dir = dist;
                    dir.Normalize();

                    var force = dir * t.deltaTime * _player.Attr.moveSpeed; //Game runs at 30 fps, but server at 60fps, so we need to multiply it by 2.

                    if (force.x != 0 && force.y != 0)
                    {
                        if (force.x > force.y)
                        {
                            force.y = 0;
                            _player.AlignYCenter();
                        }
                        else
                        {
                            force.x = 0;
                            _player.AlignXCenter();
                        }
                    }

                    bool moved = false;
                    //If we are close enought, just move to it.
                    if (force.Magnitude() > dist.Magnitude())
                    {
                        var delta = _nextPos - WorldPos;
                        moved = _player.LazyMove(delta.x, delta.y);
                    }
                    else
                    {
                        moved = _player.LazyMove(force.x, force.y);
                    }

                    if (!moved)
                    {
                        //If we were trying to hit a block, let's ignore it, since for some reason, we can't reach on it.
                        if (_targetType == TargetType.Block)
                            _exceptBlockPlaces.Add(_targetWaypoint);

                        _targetWaypoint = Vec2.INVALID;
                        _targetType = TargetType.None;
                        _nextPos = WorldPos;
                        _checkWaipointAccum = _elapsedTime; //Force to check again for another way.

                        TryToKickBomb();
                    }

                    return BehaviourTreeStatus.Running;
                }
                else if (_nextPos == WorldPos && _targetWaypoint.IsValid() && GridPos != _targetWaypoint)
                {
                    if (_nextTryCalcWaypoint > 0)
                    {
                        _nextTryCalcWaypoint -= t.deltaTime;
                    }

                    //Compute _nextPos since we've reached our destination.
                    if (!ComputeNextPosToWaypoint(out _nextPos))
                    {
                        _targetWaypoint = Vec2.INVALID;

                        //Since it failed, wait for a threshold before computing again.
                        _nextTryCalcWaypoint = 1; //Wait 1 sec
                    }
                    else
                    {
                        //Reset error threshold.
                        _nextTryCalcWaypoint = 0;
                    }
                }

                return BehaviourTreeStatus.Success;
            }
            finally
            {
                DoneBench("WalkToWaypoint");
            }
        }

        private void TryToKickBomb()
        {
            if (!_player.Attr.kickBomb)
                return;

            var facing = _player.Facing;
            var dir = Vec2.ALL_DIRS[(int)facing];

            var possibleBombPos = GridPos + dir;

            if (possibleBombPos.IsValid() && possibleBombPos.IsOnBounds(Map.MapSize.x, Map.MapSize.y))
            {
                var cell = Map[possibleBombPos.x, possibleBombPos.y];

                if (cell.FindFirstByType(ObjectType.BOMB) is Bomb bomb)
                {
                    bomb.Kick(_player, facing);
                }
            }
        }

        private BehaviourTreeStatus CalcExplosions()
        {
            try
            {
                PrepareBench();

                if (_hurryUp != null)
                {
                    foreach (var pair in _hurryUp)
                    {
                        //HurryUp time is given in milliseconds, so we need to convert into ticks
                        _explosions.Add(pair.Key, pair.Value * TimeSpan.TicksPerMillisecond);
                    }
                }

                foreach (var bomb in _bombs)
                {
                    AddExplosions(bomb, bomb.Area);
                }

                return BehaviourTreeStatus.Success;
            }
            finally
            {
                DoneBench("CalcExplosions");
            }
        }

        private BehaviourTreeStatus FindNearBombs()
        {
            try
            {
                PrepareBench();

                _explosions.Clear();
                _bombs.Clear();

                var bombs = Map.FindAllByType<Bomb>().Where(b => IsWhitinVisionRange(b)).ToList();
                if (bombs.Count == 0)
                {
                    //There is no bomb in our vision range, so return a failure so the others action aren't executed
                    return BehaviourTreeStatus.Failure;
                }

                _bombs = bombs;
                return BehaviourTreeStatus.Success;
            }
            finally
            {
                DoneBench("FindNearBombs");
            }
        }

        private bool ShouldWeDetectBombs(TimeData t)
        {
            _checkBombAccum += t.deltaTime;

            if (_checkBombAccum < _checkBombInterval)
                return false;

            _checkBombAccum = 0;

            //TODO: Maybe do some other checks?

            return true;
        }

        private bool CheckNonExistingWaypoint()
        {
            //TODO: Check if we are not going somewhere already
            return false;
        }

        private BehaviourTreeStatus WalkAround()
        {
            //TODO: Find a random place to go
            return BehaviourTreeStatus.Failure;
        }

        private BehaviourTreeStatus GotoNearestBlock()
        {
            try
            {
                PrepareBench();
                var target = FindNearestBlockPos();

                if (!target.IsValid())
                    return BehaviourTreeStatus.Failure;

                target = FindSideToPlaceBomb(target);

                if (!target.IsValid())
                    return BehaviourTreeStatus.Failure;

                SetTarget(target, TargetType.Block);
                _placeBombAtTarget = true;

                return _targetWaypoint.IsValid() ? BehaviourTreeStatus.Success : BehaviourTreeStatus.Failure;
            }
            finally
            {
                DoneBench("GotoNearestBlock");
            }
        }

        //private bool CheckBlockNear()
        //{
        //    //TODO: Check if there is some block whitin our vision range
        //    return false;
        //}

        private BehaviourTreeStatus TryToLockNKillPlayer()
        {
            try
            {
                PrepareBench();


                var target = FindNearestPlayerPos();

                if (!target.IsValid())
                    return BehaviourTreeStatus.Failure;

                target = FindSideToPlaceBomb(target, true);

                if (!target.IsValid())
                    return BehaviourTreeStatus.Failure;

                SetTarget(target, TargetType.Player);
                _placeBombAtTarget = true;

                return _targetWaypoint.IsValid() ? BehaviourTreeStatus.Success : BehaviourTreeStatus.Failure;
            }
            finally
            {
                DoneBench("TryToLockNKillPlayer");
            }
        }

        private bool CheckWalkAround()
        {
            if (_targetType == TargetType.None && _nextWalkAround > _elapsedTime)
            {
                return false;
            }

            _nextWalkAround = _elapsedTime + (TimeSpan.TicksPerSecond * _rnd.Next(1, 3));

            return true;
        }

        private bool CheckAggro()
        {
            if (_player.Attr.bombCount == 0)
                return false;

            if (_targetType != TargetType.Player && _targetWaypoint.IsValid())
            {
                return TriggeredAggro();
            }

            return true;
        }

        private BehaviourTreeStatus GotoNearestPowerup()
        {
            try
            {
                PrepareBench();

                var target = FindNearestPowerupPos();
                if (target.IsValid())
                {
                    SetTarget(target, TargetType.Powerup);
                    return BehaviourTreeStatus.Success;
                }
                else
                {
                    return BehaviourTreeStatus.Failure;
                }
            }
            finally
            {
                DoneBench("GotoNearestPowerup");
            }
        }

        //private bool CheckPowerupNear()
        //{
        //    //TODO: Check if there is a powerup whitin our vision range
        //    return false;
        //}

        private BehaviourTreeStatus FindSafePlace()
        {
            try
            {
                PrepareBench();

                var target = FindNearestSafeAccessiblePos(GridPos);
                if (target.IsValid())
                {

                    SetTarget(target, TargetType.SafePlace);
                    return BehaviourTreeStatus.Success;
                }

                if (_player.Attr.kickBomb)
                {
                    target = FindNearestBombPos();
                    if (target.IsValid())
                    {
                        target = FindSideToKickBomb(target);
                        if (target.IsValid())
                        {
                            _kickBombAtTarget = true;
                            SetTarget(target, TargetType.ForceSafePlace);
                            return BehaviourTreeStatus.Success;
                        }
                    }
                }

                target = FindSafestAccessiblePos(GridPos);
                if (target.IsValid())
                {
                    SetTarget(target, TargetType.ForceSafePlace);
                    return BehaviourTreeStatus.Success;
                }

                return BehaviourTreeStatus.Failure;
            }
            finally
            {
                DoneBench("FindSafePlace");
            }
        }

        private BehaviourTreeStatus FindRandomSafePlace()
        {
            try
            {
                PrepareBench();

                var target = FindRandomSafeAccessiblePos();
                if (target.IsValid())
                {
                    SetTarget(target, TargetType.RandomPlace);
                    return BehaviourTreeStatus.Success;
                }
                else
                {
                    return BehaviourTreeStatus.Failure;
                }
            }
            finally
            {
                DoneBench("FindRandomSafePlace");
            }
        }

        protected bool CheckLeaveThisPlace()
        {
            if (_justPlacedBomb)
            {
                _justPlacedBomb = false;
                return true;
            }
            return !IsSafePlace(GridPos, _targetWaypoint.IsValid() ? SafetyGuarantee.JustPass : SafetyGuarantee.StayForLong);
        }

        #endregion


        #region LOGIC

        protected bool TriggeredAggro()
        {
            return _rnd.NextDouble() < _aggroRate;
        }

        protected Vec2[] GetRndDirs()
        {
            return RND_DIRS[_rnd.Next(0, RND_DIRS.Length)];
        }

        protected bool IsWhitinVisionRange(GridObject obj)
        {
            if (obj == null
                || obj == _player
                || (obj is HitableObject hitable && !hitable.IsLive))
                return false;

            var distance = (Vec2f)obj.GridPos - (Vec2f)_player.GridPos;

            return Math.Abs(distance.x) < _visionRange && Math.Abs(distance.y) < _visionRange;
        }

        protected void AddExplosions(Bomb bomb, uint area)
        {
            var explosionTicks = _elapsedTime + (long)(bomb.Timeout * TimeSpan.TicksPerSecond);

            //First add the bomb place.
            if (_explosions.TryGetValue(bomb.GridPos, out var existing))
            {
                _explosions[bomb.GridPos] = Math.Min(existing, explosionTicks);
            }
            else
            {
                _explosions[bomb.GridPos] = explosionTicks;
            }

            var addedExplosions = new HashSet<Vec2>();
            //Now add the explosions that it will generate
            foreach (var dir in Vec2.ALL_DIRS)
            {
                for (int i = 0; i < area; i++)
                {
                    var pos = bomb.GridPos + (dir * i);

                    var cell = Map[pos.x, pos.y];

                    if (cell.Type == CellType.None)
                    {
                        addedExplosions.Add(pos);
                        if (_explosions.TryGetValue(pos, out existing))
                        {
                            //If there is a bomb already there, calc the explosion chain
                            if (cell.FindFirstByType(ObjectType.BOMB) != null)
                            {
                                if (existing == explosionTicks)
                                {
                                    continue;
                                }
                                else if (existing < explosionTicks)
                                {
                                    //Update our explosion time
                                    explosionTicks = existing;
                                    foreach (var posAgain in addedExplosions)
                                    {
                                        _explosions[posAgain] = explosionTicks;
                                    }
                                }
                                else
                                {
                                    UpdateExplosionChain(cell, explosionTicks);
                                }
                            }
                            else
                            {
                                //If there is already an explosion on this area, pick the shortest one
                                _explosions[pos] = Math.Min(existing, explosionTicks);
                            }
                        }
                        else
                        {
                            //Save the position of the explosion and when it is gonna happen
                            _explosions[pos] = explosionTicks;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

#if _DEBUG
            //foreach (var pos in _explosions.Keys)
            //{
            //    Debug.Assert(Map[pos.x, pos.y].Type == CellType.None);
            //}
#endif
        }

        private void UpdateExplosionChain(GridCell bombCell, long ticks, HashSet<Vec2> checkedOnes = null)
        {
            if (checkedOnes == null)
                checkedOnes = new HashSet<Vec2>();

            var area = (bombCell.FindFirstByType(ObjectType.BOMB) as Bomb).Area;

            foreach (var dir in Vec2.ALL_DIRS)
            {
                for (int i = 0; i < area; i++)
                {
                    var pos = bombCell.Pos + (dir * i);

                    if (!pos.IsValid() || !pos.IsOnBounds(Map.MapSize.x, Map.MapSize.y))
                        continue;

                    var explosionCell = Map[pos.x, pos.y];

                    if (explosionCell.Type == CellType.None && !checkedOnes.Contains(explosionCell.Pos))
                    {
                        checkedOnes.Add(explosionCell.Pos);

                        if (!_explosions.TryGetValue(pos, out var existing))
                            continue;

                        if (!_hurryUp.Keys.Contains(explosionCell.Pos)) //Skip hurry up in this check
                            continue;

                        if (existing < ticks)
                        {
                            if (explosionCell.FindFirstByType(ObjectType.BOMB) != null)
                            {
                                    UpdateExplosionChain(explosionCell, ticks, checkedOnes);
                            }
                            else
                            {
                                _explosions[pos] = ticks;
                            }
                        }
                    }
                }
            }
        }

        protected enum SafetyGuarantee
        {
            JustPass,
            StayForShort,
            StayForLong,
        }

        /// <summary>
        /// This is used to check the origin of distance when computing if a place is safe. This is usually set when doing some prediction in future, so this the starting point to reach the safe point in the future.
        /// </summary>
        private Vec2 _fromSafe = Vec2.INVALID;
        private long _delaySafe;

        protected bool IsSafePlace(Vec2 place, SafetyGuarantee guarantee)
        {
            var from = _fromSafe.IsValid() ? _fromSafe : GridPos;

            //If we aren't willing a place to stay and this place isn't gonna blow
            if (_explosions.TryGetValue(place, out var when))
            {
                //If we are going to stay for long, we cant be on a place that gonna blow, even if we have time to leave after.
                if (guarantee == SafetyGuarantee.StayForLong)
                    return false;

                var timeLeft = (when - _elapsedTime) / TimeSpan.TicksPerMillisecond;

                var safeThreshold = 1000 / _player.Attr.moveSpeed //The time to walk at least on unit
                    + 500 //A safe threshold to avoid floating point erros.
                    + _delaySafe; //An additional delay to use when doing some check in the future

                //If this place is gonna blow before we can reach it or if we are willing to stay for a little but it will blow soon, better don't go there.
                if (timeLeft < safeThreshold || ((guarantee == SafetyGuarantee.StayForShort) && timeLeft < (safeThreshold * 4)))
                    return false;

                var dist = from.Distance(place);
                var eta = dist / (float)_player.Attr.moveSpeed;

                //this place is only safe if we can reach if there before the place blow and with a safety margin.
                if (timeLeft - (eta * 1000) < safeThreshold) //Seconds to milliseconds
                    return false;
            }

            return true;
        }

        private bool IsDeadEnd(Vec2 place)
        {
            foreach (var dir in Vec2.ALL_DIRS)
            {
                var pos = place + dir;

                if (pos.IsValid() && pos.IsOnBounds(Map.MapSize.x, Map.MapSize.y) && _player.CanPassThrough(Map[pos.x, pos.y], true)) //Since we are doing a check in the future, we should ignore the skip bomb condition
                {
                    return false;
                }
            }

            return true;
        }

        protected int CalcWhenGonnaBlow(Vec2 place)
        {
            if (_explosions.TryGetValue(place, out var when))
            {
                var timeLeft = (when - _elapsedTime) / TimeSpan.TicksPerMillisecond;
                var dist = GridPos.Distance(place);
                var eta = (dist / (float)_player.Attr.moveSpeed) * 1000;

                return (int)(eta - timeLeft);
            }

            return -10000;
        }

        protected bool CanWalkBy(Vec2 place)
        {
            return _player.CanPassThrough(place) && IsSafePlace(place, SafetyGuarantee.JustPass);
        }

        protected Vec2 FindSideToKickBomb(Vec2 target)
        {
            foreach (var dir in Vec2.ALL_DIRS)
            {
                var neighbor = dir + target;

                if (!neighbor.IsValid() || !neighbor.IsOnBounds(Map.MapSize.x, Map.MapSize.y))
                    continue;

                var cell = Map[neighbor.x, neighbor.y];

                if (cell.FindFirstByType(ObjectType.BOMB) != null)
                {
                    continue;
                }

                if (CanWalkBy(neighbor) //We must place a bomb where we can walk
                    && IsSafePlace(neighbor, SafetyGuarantee.JustPass)) //We should be able to got there to just kick the bomb
                {
                    var path = Map.PathFind(GridPos, neighbor, c => CanWalkBy(c.Pos));

                    if (path == null || path.Count == 0
                        || path.Last() != neighbor) //We just want a path that leads to our neighbor. No intermediate path.
                        continue;

                    return path.Last();
                }
            }

            return Vec2.INVALID;
        }

        protected Vec2 FindSideToPlaceBomb(Vec2 target, bool lockTarget = false)
        {
            foreach (var dir in Vec2.ALL_DIRS)
            {
                var neighbor = dir + target;

                if (!neighbor.IsValid() || !neighbor.IsOnBounds(Map.MapSize.x, Map.MapSize.y))
                    continue;

                var cell = Map[neighbor.x, neighbor.y];

                if (cell.FindFirstByType(ObjectType.BOMB) != null)
                {
                    if (lockTarget)
                        return Vec2.INVALID; //If there is already a bomb near it, and we don't wanna try to lock this (like a block, for instnace), just return invalid.
                    else
                        continue;
                }

                if (CanWalkBy(neighbor) //We must place a bomb where we can walk
                    && IsSafePlace(neighbor, SafetyGuarantee.StayForShort)) //We should be able to got there, place a bomb and leave, so the place must be safe enough
                {
                    if (CheckBombPosWouldBlockMyself(neighbor)) //We can't place a bomb in a place where would block and kill us.
                        continue;

                    if (neighbor == GridPos)
                    {
                        return GridPos;
                    }

                    var path = Map.PathFind(GridPos, neighbor, c => CanWalkBy(c.Pos));

                    if (path == null || path.Count == 0
                        || path.Last() != neighbor) //We just want a path that leads to our neighbor. No intermediate path.
                        continue;

                    return path.Last();
                }
            }

            return Vec2.INVALID;
        }

        protected Vec2 FindNearestSafeAccessiblePos(Vec2 from, bool skipCurrent = false)
        {
            return Map.FindClosestPos(from,
                int.MaxValue,
                c => IsSafePlace(c.Pos, SafetyGuarantee.StayForLong) && !IsDeadEnd(c.Pos) && !(skipCurrent && from == c.Pos),
                c => CanWalkBy(c.Pos),
                Vec2.RND_DIRS[_rnd.Next(0, Vec2.RND_DIRS.Length)]);
        }

        protected Vec2 FindSafestAccessiblePos(Vec2 from)
        {
            return Map.FindBestPos(from,
                int.MaxValue,
                c => CalcWhenGonnaBlow(c.Pos) + from.Distance(GridPos),
                c => _player.CanPassThrough(c),
                c => _player.CanPassThrough(c),
                Vec2.RND_DIRS[_rnd.Next(0, Vec2.RND_DIRS.Length)]);
        }

        protected Vec2 FindRandomSafeAccessiblePos()
        {
            var rndDist = _rnd.Next(1, _visionRange);

            return Map.FindClosestPos(GridPos,
                _visionRange,
                c => IsSafePlace(c.Pos, SafetyGuarantee.StayForLong) && c.Pos.Distance(GridPos) >= rndDist,
                c => CanWalkBy(c.Pos),
                Vec2.RND_DIRS[_rnd.Next(0, Vec2.RND_DIRS.Length)]);
        }

        protected Vec2 FindNearestPowerupPos()
        {
            return Map.FindClosestPos(GridPos,
                _visionRange,
                c => IsSafePlace(c.Pos, SafetyGuarantee.StayForLong) && IsWhitinVisionRange(c.FindFirstByType(ObjectType.POWERUP)),
                c => CanWalkBy(c.Pos),
                Vec2.RND_DIRS[_rnd.Next(0, Vec2.RND_DIRS.Length)]);
        }

        protected Vec2 FindNearestPlayerPos()
        {
            return Map.FindClosestPos(GridPos,
                _visionRange,
                c => c.FindFirstByType(ObjectType.PLAYER) is HitableObject o && o.UID != _player.UID && o.IsLive,
                c => CanWalkBy(c.Pos),
                Vec2.RND_DIRS[_rnd.Next(0, Vec2.RND_DIRS.Length)]);
        }

        protected Vec2 FindNearestBombPos()
        {
            return Map.FindClosestPos(GridPos,
                _visionRange,
                c => c.FindFirstByType(ObjectType.BOMB) is HitableObject o && o.IsLive,
                c => c.Type == CellType.None,
                Vec2.RND_DIRS[_rnd.Next(0, Vec2.RND_DIRS.Length)]);
        }

        private Vec2 FindNearestBlockPos()
        {
            return Map.FindClosestPos(GridPos,
                _visionRange,
                c => c.HasAttribute(CellAttributes.BREAKABLE) && !_exceptBlockPlaces.Contains(c.Pos), //It should be a breakable cell and can't be on exception list
                c => c.FindFirstByType(ObjectType.BOMB) == null,
                Vec2.RND_DIRS[_rnd.Next(0, Vec2.RND_DIRS.Length)]);
        }

        private bool ComputeNextPosToWaypoint(out Vec2f nextPos)
        {
            //If we aren't able to find where to go next, just stay where we are.
            nextPos = WorldPos;

            var path = Map.PathFind(GridPos, _targetWaypoint, c => CanWalkBy(c.Pos), Vec2.RND_DIRS[_rnd.Next(0, Vec2.RND_DIRS.Length)]);

            //We need to have at least 2 items on path list to it be valid
            if (path?.Count > 1)
            {
                var dest = path[1];

                if (IsDeadEnd(dest))
                    return false;

                nextPos = Map.GridToWorld(path[1]); //Since the first item on path is current path, we need to walk to next item (1)

                return true;
            }

            //If we need to find a way by force, we'll try to reach our place ignoring explisions.
            if (_targetType == TargetType.ForceSafePlace)
            {
                path = Map.PathFind(GridPos, _targetWaypoint, c => _player.CanPassThrough(c), Vec2.RND_DIRS[_rnd.Next(0, Vec2.RND_DIRS.Length)]);

                if (path?.Count > 1)
                {
                    nextPos = Map.GridToWorld(path[1]); //Since the first item on path is current path, we need to walk to next item (1)

                    return true;
                }
            }

            return false;
        }

        class FakeBomb : Bomb
        {
            Vec2 _fakeGridPos;
            public FakeBomb(Vec2 gridPos, GridMap map, GridObject owner, float additionalDelay) : base(map, owner)
            {
                _fakeGridPos = gridPos;
                _attr.timeout += additionalDelay;
            }

            public override Vec2 GridPos { get => _fakeGridPos; set => new InvalidOperationException("You can't do this, dumbass"); }
        }


        private bool CheckBombPosWouldBlockMyself(Vec2 pos)
        {
            //Backup the actual explosion dictionary
            var bak = _explosions;
            _fromSafe = pos;

            _delaySafe = GridPos.Distance(pos) / _player.Attr.moveSpeed * 1000; //Add the ETA
            _delaySafe += 1000 / _player.Attr.moveSpeed; //Add the time to leave this place (Place the bomb then leave)

            //Create a new temporary one
            _explosions = new Dictionary<Vec2, long>(_explosions);

            //Add temp bomb on temp explosions dict
            AddExplosions(new FakeBomb(pos, Map, _player, (_delaySafe / 1000f)), _player.Attr.bombArea);

            //Try to find a safe position on this temp dict
            var t = FindNearestSafeAccessiblePos(pos, true);

            //Restore original one
            _explosions = bak;
            _fromSafe = Vec2.INVALID;
            _delaySafe = 0;

            return !t.IsValid();
        }

        #endregion
    }
}
