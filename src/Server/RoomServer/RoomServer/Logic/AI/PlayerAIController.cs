using CommonLib.GridEngine;
using CommonLib.Util.Math;
using CommonLib.Util;
using RoomServer.Logic.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespace RoomServer.Logic.AI
//{
//    public enum AIState
//    {
//        State_Think = 0,
//        State_Walk,
//        State_Defense,
//        State_Item,
//        State_Attack,
//    }
//
//    public enum EnemyDirection
//    {
//        Direction_Unknown = 0,
//        Direction_Here,
//        Direction_Above,
//        Direction_Below,
//        Direction_Left,
//        Direction_Right
//    }
//
//    public enum EnemyMove
//    {
//        Move_None = 0,
//        Move_Up,
//        Move_Down,
//        Move_Left,
//        Move_Right
//    }
//
//    public class PlayerAIController
//    {
//        private readonly int AI_VIEW_SIZE = 6;
//        private readonly int SOFT_WALL_NEAR_MAX_DEPTH = 2;
//        private readonly int MAX_NEAR_DISTANCE = 3;
//
//        private Player _player;
//
//        // If the player does not have to stop commanding his bomber.
//        private float _stopTimeLeft;
//
//        private AIState _state;
//        private EnemyMove _move;
//
//        private Random _rand;
//
//        private Vec2i _goal;
//
//        // Current target of the AI.
//        private uint _targetUID;
//
//        private bool _itemDropBomb;
//
//        // test
//        private int _numAccessible;
//        private int[,] _accessible;
//        private int[,] _pseudoAccessible;
//
//        private int[,] _deadEnd;
//        private int[,] _softWallNear;
//
//        private static int[,] _burnMark;
//
//        private int _callsOfModeItem;
//
//        internal PlayerAIController(Player player)
//        {
//            _player = player;
//
//            // Wait a little before thinking for the first time.
//            _stopTimeLeft = 0.1f;
//
//            _targetUID = 0;
//
//            _goal = Vec2i.ZERO;
//
//            // Seed random.
//            _rand = new Random(Guid.NewGuid().GetHashCode()); //new Random((int)DateTime.Now.Ticks);
//
//            // Set default state.
//            SetState(AIState.State_Think);
//
//            _move = EnemyMove.Move_None;
//
//            _accessible = new int[player.Map.MapSize.x, player.Map.MapSize.y];
//            _pseudoAccessible = new int[player.Map.MapSize.x, player.Map.MapSize.y];
//
//            _softWallNear = new int[player.Map.MapSize.x, player.Map.MapSize.y];
//            _deadEnd = new int[player.Map.MapSize.x, player.Map.MapSize.y];
//
//            _burnMark = new int[4, 6]
//            {
//                {  0,  0,  0,  0,  0,  0 },
//                { 10,  8,  5,  3,  2,  1 },
//                { 20, 17, 15, 12, 10,  5 },
//                { 30, 26, 24, 22,  5, 10 }
//            };
//
//            _callsOfModeItem = 0;
//        }
//
//        private void UpdateAccessibility()
//        {
//            //****************
//            // ACCESSIBLE
//            //****************
//
//            int curBlockX = _player.GridPos.x;
//            int curBlockY = _player.GridPos.y;
//
//            int BlockX;
//            int BlockY;
//
//            bool Updated;
//
//            // All squares are assumed inaccessible
//            for (BlockX = 0; BlockX < _player.Map.MapSize.x; BlockX++)
//            {
//                for (BlockY = 0; BlockY < _player.Map.MapSize.y; BlockY++)
//                {
//                    _accessible[BlockX, BlockY] = -1;
//                }
//            }
//
//            // The square where the bomber is is accessible (distance zero)
//            _accessible[curBlockX, curBlockY] = 0;
//
//            _numAccessible = 1;
//
//            // Find all the squares that are accessible to the bomber
//            // If no update was performed then it's over because there 
//            // are no more accessible squares that weren't marked.
//            do
//            {
//                Updated = false;
//
//                // Scan each block of the AI view
//                for (BlockX = curBlockX - AI_VIEW_SIZE; BlockX < curBlockX + AI_VIEW_SIZE; BlockX++)
//                {
//                    for (BlockY = curBlockY - AI_VIEW_SIZE; BlockY < curBlockY + AI_VIEW_SIZE; BlockY++)
//                    {
//                        // If the block is on the edges of the arena grid
//                        if (BlockX <= 0 || BlockX >= _player.Map.MapSize.x - 1 ||
//                            BlockY <= 0 || BlockY >= _player.Map.MapSize.y - 1)
//                        {
//                            continue;
//                        }
//                        // If it is an accessible square 
//                        else if (_accessible[BlockX, BlockY] != -1)
//                        {
//                            // If there is a square ABOVE that is marked as not accessible and it's not a wall/bomb
//                            if (_accessible[BlockX, BlockY - 1] == -1 &&
//                                !IsWall(BlockX, BlockY - 1) && !IsWallBreakable(BlockX, BlockY - 1) &&
//                                !IsBomb(BlockX, BlockY - 1))
//                            {
//                                // Mark it as accessible
//                                _accessible[BlockX, BlockY - 1] = _accessible[BlockX, BlockY] + 1;
//
//                                Updated = true;
//
//                                _numAccessible++;
//                            }
//
//                            // If there is a square BELOW that is marked as not accessible and it's not a wall/bomb
//                            if (_accessible[BlockX, BlockY + 1] == -1 &&
//                                !IsWall(BlockX, BlockY + 1) && !IsWallBreakable(BlockX, BlockY + 1) &&
//                                !IsBomb(BlockX, BlockY + 1))
//                            {
//                                // Mark it as accessible
//                                _accessible[BlockX, BlockY + 1] = _accessible[BlockX, BlockY] + 1;
//
//                                Updated = true;
//
//                                _numAccessible++;
//                            }
//
//                            // If there is a square TO THE LEFT that is marked as not accessible and it's not a wall/bomb
//                            if (_accessible[BlockX - 1, BlockY] == -1 &&
//                                !IsWall(BlockX - 1, BlockY) && !IsWallBreakable(BlockX - 1, BlockY) &&
//                                !IsBomb(BlockX - 1, BlockY))
//                            {
//                                // Mark it as accessible
//                                _accessible[BlockX - 1, BlockY] = _accessible[BlockX, BlockY] + 1;
//
//                                Updated = true;
//
//                                _numAccessible++;
//                            }
//
//                            // If there is a square TO THE RIGHT that is marked as not accessible and it's not a wall/bomb
//                            if (_accessible[BlockX + 1, BlockY] == -1 &&
//                                !IsWall(BlockX + 1, BlockY) && !IsWallBreakable(BlockX + 1, BlockY) &&
//                                !IsBomb(BlockX + 1, BlockY))
//                            {
//                                // Mark it as accessible
//                                _accessible[BlockX + 1, BlockY] = _accessible[BlockX, BlockY] + 1;
//
//                                Updated = true;
//
//                                _numAccessible++;
//                            }
//                        }
//                    }
//                }
//            }
//            while (Updated);
//        }
//
//        private void UpdateDeadZone()
//        {
//            // Auxiliary variables for code readability : is there a wall around the current block?
//            bool IsWallHere;
//            bool IsWallUp;
//            bool IsWallDown;
//            bool IsWallLeft;
//            bool IsWallRight;
//
//            // Current dead end number. Incremented each time there is a new dead end.
//            int CurrentDeadEnd = 0;
//
//            // Scan each block of the dead end array
//            for (var BlockX = 0; BlockX < _player.Map.MapSize.x; BlockX++)
//            {
//                for (var BlockY = 0; BlockY < _player.Map.MapSize.y; BlockY++)
//                {
//                    // Set undefined dead end (is there one? we don't know yet)
//                    _deadEnd[BlockX, BlockY] = -2;
//                }
//            }
//
//            // Scan each block of the arena
//            for (var BlockX = 0; BlockX < _player.Map.MapSize.x; BlockX++)
//            {
//                for (var BlockY = 0; BlockY < _player.Map.MapSize.y; BlockY++)
//                {
//                    // If the dead end on this block is currently undefined
//                    if (_deadEnd[BlockX, BlockY] == -2)
//                    {
//                        if (BlockX == 0 || BlockX == _player.Map.MapSize.x - 1 ||
//                        BlockY == 0 || BlockY == _player.Map.MapSize.y - 1)
//                        {
//                            _softWallNear[BlockX, BlockY] = -1;
//                        }
//                        else
//                        {
//                            IsWallHere = IsWall(BlockX, BlockY) || IsWallBreakable(BlockX, BlockY);
//
//                            // If there is no wall on this block
//                            if (!IsWallHere)
//                            {
//                                // Is there a wall around this block?
//                                IsWallUp = IsWall(BlockX, BlockY - 1) || IsWallBreakable(BlockX, BlockY - 1);
//                                IsWallDown = IsWall(BlockX, BlockY + 1) || IsWallBreakable(BlockX, BlockY + 1);
//                                IsWallLeft = IsWall(BlockX - 1, BlockY) || IsWallBreakable(BlockX - 1, BlockY);
//                                IsWallRight = IsWall(BlockX + 1, BlockY) || IsWallBreakable(BlockX + 1, BlockY);
//
//                                // If this block is the back of a dead end ("[")
//                                if (IsWallLeft && IsWallUp && IsWallDown)
//                                {
//                                    // Start scanning the dead end on this block
//                                    int DeadEndBlockX = BlockX;
//                                    int DeadEndBlockY = BlockY;
//
//                                    // While we are still in this dead end and there is no wall blocking the way
//                                    while (IsWallUp && IsWallDown && !IsWallHere)
//                                    {
//                                        // Set the dead end number of the current block : it's the current dead end number
//                                        _deadEnd[DeadEndBlockX, DeadEndBlockY] = CurrentDeadEnd;
//
//                                        // Continue scanning (go right)
//                                        DeadEndBlockX++;
//
//                                        // Update the auxiliary variables value.
//                                        IsWallHere = IsWall(DeadEndBlockX, DeadEndBlockY) || IsWallBreakable(DeadEndBlockX, DeadEndBlockY);
//                                        IsWallUp = IsWall(DeadEndBlockX, DeadEndBlockY - 1) || IsWallBreakable(DeadEndBlockX, DeadEndBlockY - 1);
//                                        IsWallDown = IsWall(DeadEndBlockX, DeadEndBlockY + 1) || IsWallBreakable(DeadEndBlockX, DeadEndBlockY + 1);
//                                    }
//
//                                    // Next dead end number
//                                    CurrentDeadEnd++;
//                                }
//                                // If this block is the back of a dead end ("Ú¿")
//                                else if (IsWallUp && IsWallLeft && IsWallRight)
//                                {
//                                    // Start scanning the dead end on this block
//                                    int DeadEndBlockX = BlockX;
//                                    int DeadEndBlockY = BlockY;
//
//                                    // While we are still in this dead end and there is no wall blocking the way
//                                    while (IsWallLeft && IsWallRight && !IsWallHere)
//                                    {
//                                        // Set the dead end number of the current block : it's the current dead end number
//                                        _deadEnd[DeadEndBlockX, DeadEndBlockY] = CurrentDeadEnd;
//
//                                        // Continue scanning (go down)
//                                        DeadEndBlockY++;
//
//                                        // Update the auxiliary variables value.
//                                        IsWallHere = IsWall(DeadEndBlockX, DeadEndBlockY) || IsWallBreakable(DeadEndBlockX, DeadEndBlockY);
//                                        IsWallLeft = IsWall(DeadEndBlockX - 1, DeadEndBlockY) || IsWallBreakable(DeadEndBlockX - 1, DeadEndBlockY);
//                                        IsWallRight = IsWall(DeadEndBlockX + 1, DeadEndBlockY) || IsWallBreakable(DeadEndBlockX + 1, DeadEndBlockY);
//                                    }
//
//                                    // Next dead end number
//                                    CurrentDeadEnd++;
//                                }
//                                // If this block is the back of a dead end ("]")
//                                else if (IsWallRight && IsWallUp && IsWallDown)
//                                {
//                                    // Start scanning the dead end on this block
//                                    int DeadEndBlockX = BlockX;
//                                    int DeadEndBlockY = BlockY;
//
//                                    // While we are still in this dead end and there is no wall blocking the way
//                                    while (IsWallUp && IsWallDown && !IsWallHere)
//                                    {
//                                        // Set the dead end number of the current block : it's the current dead end number
//                                        _deadEnd[DeadEndBlockX, DeadEndBlockY] = CurrentDeadEnd;
//
//                                        // Continue scanning (go left)
//                                        DeadEndBlockX--;
//
//                                        // Update the auxiliary variables value.
//                                        IsWallHere = IsWall(DeadEndBlockX, DeadEndBlockY) || IsWallBreakable(DeadEndBlockX, DeadEndBlockY);
//                                        IsWallUp = IsWall(DeadEndBlockX, DeadEndBlockY - 1) || IsWallBreakable(DeadEndBlockX, DeadEndBlockY - 1);
//                                        IsWallDown = IsWall(DeadEndBlockX, DeadEndBlockY + 1) || IsWallBreakable(DeadEndBlockX, DeadEndBlockY + 1);
//                                    }
//
//                                    // Next dead end number
//                                    CurrentDeadEnd++;
//                                }
//                                // If this block is the back of a dead end ("ÀÙ)
//                                else if (IsWallDown && IsWallLeft && IsWallRight)
//                                {
//                                    // Start scanning the dead end on this block
//                                    int DeadEndBlockX = BlockX;
//                                    int DeadEndBlockY = BlockY;
//
//                                    // While we are still in this dead end and there is no wall blocking the way
//                                    while (IsWallLeft && IsWallRight && !IsWallHere)
//                                    {
//                                        // Set the dead end number of the current block : it's the current dead end number
//                                        _deadEnd[DeadEndBlockX, DeadEndBlockY] = CurrentDeadEnd;
//
//                                        // Continue scanning (go up)
//                                        DeadEndBlockY--;
//
//                                        // Update the auxiliary variables value.
//                                        IsWallHere = IsWall(DeadEndBlockX, DeadEndBlockY) || IsWallBreakable(DeadEndBlockX, DeadEndBlockY);
//                                        IsWallLeft = IsWall(DeadEndBlockX - 1, DeadEndBlockY) || IsWallBreakable(DeadEndBlockX - 1, DeadEndBlockY);
//                                        IsWallRight = IsWall(DeadEndBlockX + 1, DeadEndBlockY) || IsWallBreakable(DeadEndBlockX + 1, DeadEndBlockY);
//                                    }
//
//                                    // Next dead end number
//                                    CurrentDeadEnd++;
//                                }
//                                // If this block is the back of NO dead end
//                                else
//                                {
//                                    // Set there is no dead end on this block. There may be a dead end
//                                    // containing this block but we record dead ends by detecting their back.
//                                    _deadEnd[BlockX, BlockY] = -1;
//                                }
//                            }
//                            // If there is a wall on this block
//                            else
//                            {
//                                // There is definitely no dead end here
//                                _deadEnd[BlockX, BlockY] = -1;
//                            }
//                        }
//                    }
//                }
//            }
//        }
//
//        private int GetDeadEnd(int BlockX, int BlockY)
//        {
//            return _deadEnd[BlockX, BlockY];
//        }
//
//        private void UpdateSoftWall()
//        {
//            // Auxiliary variables for code readability : is there a wall around the current block?
//            bool IsWallHere;
//            bool IsWallUp;
//            bool IsWallDown;
//            bool IsWallLeft;
//            bool IsWallRight;
//
//            // Auxiliary variables for code readability : is there a soft wall around the current block?
//            bool IsSoftWallUp;
//            bool IsSoftWallDown;
//            bool IsSoftWallLeft;
//            bool IsSoftWallRight;
//
//            // Current distance of the scanned block from the start block
//            int Depth;
//
//            for (var BlockX = 0; BlockX < _player.Map.MapSize.x; BlockX++)
//            {
//                for (var BlockY = 0; BlockY < _player.Map.MapSize.y; BlockY++)
//                {
//                    if (BlockX == 0 || BlockX == _player.Map.MapSize.x - 1 ||
//                        BlockY == 0 || BlockY == _player.Map.MapSize.y - 1)
//                    {
//                        _softWallNear[BlockX, BlockY] = -1;
//                    }
//                    else
//                    {
//                        IsWallHere = IsWall(BlockX, BlockY) || IsWallBreakable(BlockX, BlockY);
//
//                        if (!IsWallHere)
//                        {
//                            Depth = 1;
//
//                            do
//                            {
//                                IsWallUp = IsWall(BlockX, BlockY + Depth) || IsWallBreakable(BlockX, BlockY + Depth);
//
//                                IsSoftWallUp = IsWallBreakable(BlockX, BlockY + Depth);
//
//                                Depth++;
//                            } while (Depth <= SOFT_WALL_NEAR_MAX_DEPTH && !IsWallUp && !IsItem(BlockX, BlockY + Depth));
//
//                            ////////////////////////////
//                            Depth = 1;
//
//                            do
//                            {
//                                IsWallDown = IsWall(BlockX, BlockY - Depth) || IsWallBreakable(BlockX, BlockY - Depth);
//
//                                IsSoftWallDown = IsWallBreakable(BlockX, BlockY - Depth);
//
//                                Depth++;
//                            } while (Depth <= SOFT_WALL_NEAR_MAX_DEPTH && !IsWallDown && !IsItem(BlockX, BlockY - Depth));
//
//                            /////////////////////////////////
//                            Depth = 1;
//
//                            do
//                            {
//                                IsWallLeft = IsWall(BlockX - Depth, BlockY) || IsWallBreakable(BlockX - Depth, BlockY);
//
//                                IsSoftWallLeft = IsWallBreakable(BlockX - Depth, BlockY);
//
//                                Depth++;
//                            } while (Depth <= SOFT_WALL_NEAR_MAX_DEPTH && !IsWallLeft && !IsItem(BlockX - Depth, BlockY));
//
//                            /////////////////////////////////
//                            Depth = 1;
//
//                            do
//                            {
//                                IsWallRight = IsWall(BlockX + Depth, BlockY) || IsWallBreakable(BlockX + Depth, BlockY);
//
//                                IsSoftWallRight = IsWallBreakable(BlockX + Depth, BlockY);
//
//                                Depth++;
//                            } while (Depth <= SOFT_WALL_NEAR_MAX_DEPTH && !IsWallRight && !IsItem(BlockX + Depth, BlockY));
//
//                            //--------------------------------------------------
//                            // Count total number of soft walls near this block
//                            //--------------------------------------------------
//
//                            // No soft walls near this block yet
//                            int NumSoftWallsNear = 0;
//
//                            // Increase this number for each direction if there is a soft wall in this direction
//                            if (IsSoftWallUp) NumSoftWallsNear++;
//                            if (IsSoftWallDown) NumSoftWallsNear++;
//                            if (IsSoftWallLeft) NumSoftWallsNear++;
//                            if (IsSoftWallRight) NumSoftWallsNear++;
//
//                            // Set the number of soft walls near this block
//                            _softWallNear[BlockX, BlockY] = NumSoftWallsNear;
//                        }
//                        else
//                        {
//                            // There is definitely no soft wall near here         
//                            _softWallNear[BlockX, BlockY] = -1;
//                        }
//                    }
//                }
//            }
//        }
//
//        private int GetSoftWallNear(int blockX, int blockY)
//        {
//            return _softWallNear[blockX, blockY];
//        }
//
//        /// <summary>
//        /// Get the nearest enemy.
//        /// </summary>
//        private void GetNearTarget()
//        {
//            // Get all players of the room.
//            var players = _player.Map.FindAllByType<Player>();
//            if (players?.Count <= 0)
//            {
//                CLog.F("We have a problem here, can't be 0.");
//                return;
//            }
//
//            int newDistance = 0;
//            int distance = int.MaxValue;
//            int dX = 0;
//            int dY = 0;
//
//            foreach (Player player in players)
//            {
//                if (player.UID != _player.UID && player.IsLive)
//                {
//                    dX = player.GridPos.x - _player.GridPos.x;
//                    dY = player.GridPos.y - _player.GridPos.y;
//                    newDistance = (dX * dX) + (dY * dY);
//                    if (newDistance < distance)
//                    {
//                        distance = newDistance;
//                        _targetUID = player.UID;
//                    }
//                }
//            }
//        }
//
//        private void SetState(AIState state)
//        {
//            // Set new state.
//            this._state = state;
//        }
//
//        public void Tick(float delta)
//        {
//            // Think and send commands to the bomber only if the bomber is alive.
//            if (!_player.IsLive)
//                return;
//
//            var room = _player.Map as Room;
//
//            if (!room.IsInitialized)
//                return;
//
//            if (_stopTimeLeft >= 0.3f)
//            {
//                _stopTimeLeft = 0.0f;
//
//                UpdateAccessibility();
//                UpdateDeadZone();
//                UpdateSoftWall();
//
//                // If the AI has to think.
//                if (_state == AIState.State_Think)
//                {
//                    ModeThink();
//                }
//
//                // Update the computer player according to its mode.
//                switch (_state)
//                {
//                    case AIState.State_Walk:
//                        ModeWalk(delta);
//                        break;
//                    case AIState.State_Defense:
//                        ModeDefense();
//                        break;
//                    case AIState.State_Item:
//                        ModeItem();
//                        break;
//                    case AIState.State_Attack:
//                        ModeAttack();
//                        break;
//                    default:
//                        break;
//                }
//            }
//            else
//            {
//                // Decrease time left before sending commands to the bomber.
//                _stopTimeLeft += delta;
//            }
//        }
//
//        private void ModeAttack()
//        {
//            // Reset the commands to send to the AI.
//            _move = EnemyMove.Move_None;
//
//            // Place a bomb.
//            _player.PlaceBomb();
//
//            SetState(AIState.State_Think);
//        }
//
//        private bool GoTo(int GoalBlockX, int GoalBlockY)
//        {
//            // If the block to go to is not accessible 
//            // or the bomber is already on this block
//            if (_accessible[GoalBlockX, GoalBlockY] == -1 ||
//                _accessible[GoalBlockX, GoalBlockY] == 0)
//            {
//                // Set no bomber move to send to the bomber
//                _move = EnemyMove.Move_None;
//            }
//            // If the block to go to is accessible and the bomber is not on this block
//            else
//            {
//                // Block coordinates used to go from the goal to the 
//                // bomber using the accessible array.
//                // Start from the goal.
//                int BlockX = GoalBlockX;
//                int BlockY = GoalBlockY;
//
//                // While we have not reached the block where the bomber is
//                // and determined what bomber move to set
//                while (true)
//                {
//                    // If going to the block above makes us go closer to the block where the bomber is
//                    if (_accessible[BlockX, BlockY - 1] == _accessible[BlockX, BlockY] - 1)
//                    {
//                        // If the block above is the block where the bomber is
//                        if (_accessible[BlockX, BlockY - 1] == 0)
//                        {
//                            // We reached the bomber. Therefore the bomber has to go down.
//                            _move = EnemyMove.Move_Up;
//
//                            // We're done with determining the bomber move to send to the bomber
//                            break;
//                        }
//
//                        // Go up
//                        BlockY--;
//                    }
//                    // If going to the block below makes us go closer to the block where the bomber is
//                    else if (_accessible[BlockX, BlockY + 1] == _accessible[BlockX, BlockY] - 1)
//                    {
//                        // If the block below is the block where the bomber is
//                        if (_accessible[BlockX, BlockY + 1] == 0)
//                        {
//                            // We reached the bomber. Therefore the bomber has to go up.
//                            _move = EnemyMove.Move_Down;
//
//                            // We're done with determining the bomber move to send to the bomber
//                            break;
//                        }
//
//                        // Go down
//                        BlockY++;
//                    }
//                    // If going to the block to the left makes us go closer to the block where the bomber is
//                    else if (_accessible[BlockX - 1, BlockY] == _accessible[BlockX, BlockY] - 1)
//                    {
//                        // If the block to the left is the block where the bomber is
//                        if (_accessible[BlockX - 1, BlockY] == 0)
//                        {
//                            // We reached the bomber. Therefore the bomber has to go right.
//                            _move = EnemyMove.Move_Right;
//
//                            // We're done with determining the bomber move to send to the bomber
//                            break;
//                        }
//
//                        // Go left
//                        BlockX--;
//                    }
//                    // If going to the block to the right makes us go closer to the block where the bomber is
//                    else if (_accessible[BlockX + 1, BlockY] == _accessible[BlockX, BlockY] - 1)
//                    {
//                        // If the block to the right is the block where the bomber is
//                        if (_accessible[BlockX + 1, BlockY] == 0)
//                        {
//                            // We reached the bomber. Therefore the bomber has to go left.
//                            _move = EnemyMove.Move_Left;
//
//                            // We're done with determining the bomber move to send to the bomber
//                            break;
//                        }
//
//                        // Go right
//                        BlockX++;
//                    }
//                } // while
//            }
//
//            // If the bomber move is a real move
//            if (_move != EnemyMove.Move_None)
//            {
//                // Call function for AI to move.
//                Move();
//
//                // Set current state now for to think.
//                SetState(AIState.State_Think);
//
//
//                // Send this bomber move to the bomber as many seconds as the time
//                // a bomber takes to walk through an entire block.
//                //m_BomberMoveTimeLeft = BLOCK_SIZE * 1.0f / m_pBomber->GetPixelsPerSecond();
//            }
//            // If the bomber move is not a real move
//            else
//            {
//                // Send this bomber move only on this update
//                //m_BomberMoveTimeLeft = 0.0f;
//            }
//
//            // Return whether the bomber is on the goal block
//            return _accessible[GoalBlockX, GoalBlockY] == 0;
//        }
//
//        private void ModeItem()
//        {
//            _move = EnemyMove.Move_None;
//            EnemyDirection dir = EnemyDirection.Direction_Unknown;
//
//            if ((EnemyNearAndFront(ref dir) && DropBombOk(_player.GridPos.x, _player.GridPos.y) && _rand.Next(100) < 70) ||
//                (_itemDropBomb && !DropBombOk(_goal.x, _goal.y)))
//            {
//                // Decide what to do
//                SetState(AIState.State_Think);
//
//                // reset method state variable
//                _callsOfModeItem = 0;
//
//                // Get out, mode is over
//                return;
//            }
//
//            // Assume the goal has not been reached yet
//            bool goalReach = false;
//
//            if (_accessible[_goal.x, _goal.y] != -1)
//            {
//                // Set the bomber move command so that the bomber goes to the block to go to
//                // and return if the goal has been reached.
//                goalReach = GoTo(_goal.x, _goal.y);
//
//                // we may pass here several times, so count the number of passes here
//                _callsOfModeItem++;
//            }
//            // If the block to go to is not accessible to the bomber
//            else
//            {
//                // There is a problem : we're trying to go to a block that is not accessible.
//                // Switch to think mode so as to decide what to do.
//                SetState(AIState.State_Think);
//
//                // reset method state variables
//                _callsOfModeItem = 0;
//
//                // Get out, no need to stay here
//                return;
//            }
//
//            // If the goal was reached and the bomber has to drop a bomb and the bomber is Able to drop a bomb now
//            if (goalReach && _itemDropBomb && _player.Attr.bombCount > 0)
//            {
//                // Set the bomber action to drop a bomb
//                _player.PlaceBomb();
//
//                // The goal is the block where the bomber is (so in fact no need to move)
//                _goal.x = (short)_player.GridPos.x;
//                _goal.y = (short)_player.GridPos.y;
//
//                // The bomb will now be dropped, no need to drop another bomb now
//                _itemDropBomb = false;
//            }
//            // If the goal was reached and the bomber does not have to drop a bomb
//            else if (goalReach && !_itemDropBomb)
//            {
//                // What was decided in the think mode has been entirely executed.
//                // Switch to think mode to decide what to do now.
//                SetState(AIState.State_Think);
//
//                // reset method state variable
//                _callsOfModeItem = 0;
//            }
//            else if (_callsOfModeItem > 5)
//            {
//                // if we are too long in Mode Item, think again
//                SetState(AIState.State_Think);
//
//                // reset method state variable
//                _callsOfModeItem = 0;
//            }
//        }
//
//        private bool EnemyNear(int BlockX, int BlockY)
//        {
//            // Get all players of the room.
//            var players = _player.Map.FindAllByType<Player>();
//            if (players?.Count <= 0)
//            {
//                CLog.F("We have a problem here, can't be 0.");
//                return false;
//            }
//
//            // Scan the players
//            foreach (Player player in players)
//            {
//                // If the current player is not the one we are controlling
//                // and the bomber of this player exists and is alive
//                // and the manhattan distance between him and the tested block is not too big
//                // and with big probability
//                if (player.UID != _player.UID && player.IsLive &&
//                    Math.Abs(player.GridPos.x - BlockX) +
//                    Math.Abs(player.GridPos.y - BlockY) <= 3
//                    && _rand.Next(100) < 92)
//                {
//                    // There is an enemy not far from the tested block
//                    return true;
//                }
//            }
//
//            // There is no enemy not far from the tested block
//            return false;
//        }
//
//        private bool EnemyNearAndFront(ref EnemyDirection direction, bool BeyondArenaFrontiers = false)
//        {
//            int BlockX;
//            int BlockY;
//
//            // variables to keep loop assertion semantics (they may be out of bounds)
//            int FakeBlockX;
//            int FakeBlockY;
//
//            bool beyondTheFrontier;
//
//            if (IsBomb(_player.GridPos.x, _player.GridPos.y))
//            {
//                direction = EnemyDirection.Direction_Unknown;
//                return false;
//            }
//
//            //-----------------------------------------
//            // Check if there is an enemy where we are
//            //-----------------------------------------
//
//            // Get all players of the room.
//            var players = _player.Map.FindAllByType<Player>();
//            if (players?.Count <= 0)
//            {
//                CLog.F("We have a problem here, can't be 0.");
//                return false;
//            }
//
//            // Scan the players
//            foreach (Player player in players)
//            {
//                // If the current player is not the one we are controlling
//                // and the bomber of this player exists and is alive
//                // and this bomber is where our bomber is
//                if (player.UID != _player.UID && player.IsLive &&
//                    player.GridPos.x == _player.GridPos.x &&
//                    player.GridPos.y == _player.GridPos.y)
//                {
//                    // There is an enemy near our bomber
//                    direction = EnemyDirection.Direction_Here;
//                    return true;
//                }
//            }
//
//            //---------------------------------------------------------
//            // Check if there is an enemy near our bomber to the right
//            //---------------------------------------------------------
//
//            // Start scanning one block to the right
//            BlockX = _player.GridPos.x + 1;
//            BlockY = _player.GridPos.y;
//
//            FakeBlockX = BlockX;
//            FakeBlockY = BlockY;
//
//            beyondTheFrontier = false;
//
//            // While we are still near our bomber
//            while (FakeBlockX <= _player.GridPos.x + MAX_NEAR_DISTANCE)
//            {
//                // If we are scanning out of the arena
//                // or if there is a wall or a bomb where we are scanning
//                if (!BeyondArenaFrontiers && (BlockX >= _player.Map.MapSize.x ||
//                    IsWall(BlockX, BlockY) || IsWallBreakable(BlockX, BlockY) ||
//                    IsBomb(BlockX, BlockY)))
//                {
//                    // Stop scanning, there is no enemy near and in front of our bomber in this direction
//                    break;
//                }
//                else if (BeyondArenaFrontiers && BlockX >= _player.Map.MapSize.x)
//                {
//                    // begin at the leftmost block. notice that the FakeBlock variable
//                    // is not being changed to keep the while condition semantic
//                    BlockX = 0;
//                    beyondTheFrontier = true; // we passed the frontier
//                }
//
//                // we are already looking at blocks to the left of us
//                if (BeyondArenaFrontiers && beyondTheFrontier &&
//                    BlockX >= _player.GridPos.x - MAX_NEAR_DISTANCE)
//                    break; // we have returned near us again.
//
//                // If there is a bomber where we are scanning
//                if (IsPlayer(BlockX, BlockY))
//                {
//                    // We have an enemy bomber to the right that is near
//                    // and in front of our bomber.
//                    direction = EnemyDirection.Direction_Right;
//                    return true;
//                }
//
//                // Continue scanning (go right)
//                if (BeyondArenaFrontiers)
//                {
//                    // only increase FakeBlock if current block is not a bomb or a wall
//                    if (!IsBomb(BlockX, BlockY) &&
//                        (!IsWall(BlockX, BlockY) || !IsWallBreakable(BlockX, BlockY)))
//                        FakeBlockX++;
//                }
//                else
//                {
//                    // increase normally
//                    FakeBlockX++;
//                }
//                BlockX++;
//            }
//
//            //---------------------------------------------------------
//            // Check if there is an enemy near our bomber to the left
//            //---------------------------------------------------------
//
//            // Start scanning one block to the left
//            BlockX = _player.GridPos.x - 1;
//            BlockY = _player.GridPos.y;
//
//            FakeBlockX = BlockX;
//            FakeBlockY = BlockY;
//
//            beyondTheFrontier = false;
//
//            // While we are still near our bomber
//            while (FakeBlockX >= _player.GridPos.x - MAX_NEAR_DISTANCE)
//            {
//                // If we are scanning out of the arena
//                // or if there is a wall or a bomb where we are scanning
//                if (!BeyondArenaFrontiers && (BlockX < 0 ||
//                    IsWall(BlockX, BlockY) || IsWallBreakable(BlockX, BlockY) ||
//                    IsBomb(BlockX, BlockY)))
//                {
//                    // Stop scanning, there is no enemy near and in front of our bomber in this direction
//                    break;
//                }
//                else if (BeyondArenaFrontiers && BlockX < 0)
//                {
//                    // begin at the rightmost block. notice that the FakeBlock variable
//                    // is not being changed to keep the while condition semantic
//                    BlockX = _player.Map.MapSize.x - 1;
//                    beyondTheFrontier = true;
//                }
//
//                // we are already looking at blocks to the right of us
//                if (BeyondArenaFrontiers && beyondTheFrontier &&
//                    BlockX <= _player.GridPos.x + MAX_NEAR_DISTANCE)
//                    break; // we have returned near us again.
//
//                // If there is a bomber where we are scanning
//                if (IsPlayer(BlockX, BlockY))
//                {
//                    // We have an enemy bomber to the left that is near
//                    // and in front of our bomber.
//                    direction = EnemyDirection.Direction_Left;
//                    return true;
//                }
//
//                // Continue scanning (go left)
//                if (BeyondArenaFrontiers)
//                {
//                    // only decrease FakeBlock if current block is not a bomb or a wall
//                    if (!IsBomb(BlockX, BlockY) &&
//                        (!IsWall(BlockX, BlockY) || !IsWallBreakable(BlockX, BlockY)))
//                        FakeBlockX--;
//                }
//                else
//                {
//                    // decrease normally
//                    FakeBlockX--;
//                }
//                BlockX--;
//            }
//
//            //---------------------------------------------------------
//            // Check if there is an enemy near our bomber above
//            //---------------------------------------------------------
//
//            // Start scanning one block above
//            BlockX = _player.GridPos.x;
//            BlockY = _player.GridPos.y - 1;
//
//            FakeBlockX = BlockX;
//            FakeBlockY = BlockY;
//
//            beyondTheFrontier = false;
//
//            // While we are still near our bomber
//            while (FakeBlockY >= _player.GridPos.y - MAX_NEAR_DISTANCE)
//            {
//                // If we are scanning out of the arena
//                // or if there is a wall or a bomb where we are scanning
//                if (!BeyondArenaFrontiers && (BlockY < 0 ||
//                    IsWall(BlockX, BlockY) || IsWallBreakable(BlockX, BlockY) ||
//                    IsBomb(BlockX, BlockY)))
//                {
//                    // Stop scanning, there is no enemy near and in front of our bomber in this direction
//                    break;
//                }
//                else if (BeyondArenaFrontiers && BlockY < 0)
//                {
//                    // begin at the block the most at the bottom. notice that the FakeBlock variable
//                    // is not being changed to keep the while condition semantic
//                    BlockY = _player.Map.MapSize.y - 1;
//                    beyondTheFrontier = true;
//                }
//
//                // we are already looking at blocks below us
//                if (BeyondArenaFrontiers && beyondTheFrontier &&
//                    BlockY <= _player.GridPos.y + MAX_NEAR_DISTANCE)
//                    break; // we have returned near us again.
//
//                // If there is a bomber != me where we are scanning
//                if (IsPlayer(BlockX, BlockY))
//                {
//                    // We have an enemy bomber above that is near
//                    // and in front of our bomber.            
//                    direction = EnemyDirection.Direction_Above;
//                    return true;
//                }
//
//                // Continue scanning (go up)
//                if (BeyondArenaFrontiers)
//                {
//                    // only decrease FakeBlock if current block is not a bomb or a wall
//                    if (!IsBomb(BlockX, BlockY) &&
//                        (!IsWall(BlockX, BlockY) || !IsWallBreakable(BlockX, BlockY)))
//                        FakeBlockY--;
//                }
//                else
//                {
//                    // decrease normally
//                    FakeBlockY--;
//                }
//                BlockY--;
//            }
//
//            //---------------------------------------------------------
//            // Check if there is an enemy near our bomber below
//            //---------------------------------------------------------
//
//            // Start scanning one block below
//            BlockX = _player.GridPos.x;
//            BlockY = _player.GridPos.y + 1;
//
//            FakeBlockX = BlockX;
//            FakeBlockY = BlockY;
//
//            beyondTheFrontier = false;
//
//            // While we are still near our bomber
//            while (FakeBlockY <= _player.GridPos.y + MAX_NEAR_DISTANCE)
//            {
//                // If we are scanning out of the arena
//                // or if there is a wall or a bomb where we are scanning
//                if (!BeyondArenaFrontiers && (BlockY >= _player.Map.MapSize.y ||
//                    IsWall(BlockX, BlockY) || IsWallBreakable(BlockX, BlockY) ||
//                    IsBomb(BlockX, BlockY)))
//                {
//                    // Stop scanning, there is no enemy near and in front of our bomber in this direction
//                    break;
//                }
//                else if (BeyondArenaFrontiers && BlockY >= _player.Map.MapSize.y)
//                {
//                    // begin at the topmost block. notice that the FakeBlock variable
//                    // is not being changed to keep the while condition semantic
//                    BlockY = 0;
//                    beyondTheFrontier = true;
//                }
//
//                // we are already looking at blocks above us
//                if (BeyondArenaFrontiers && beyondTheFrontier &&
//                    BlockY >= _player.GridPos.y - MAX_NEAR_DISTANCE)
//                    break; // we have returned near us again.
//
//                // If there is a bomber != me where we are scanning
//                if (IsPlayer(BlockX, BlockY))
//                {
//                    // We have an enemy bomber above that is near
//                    // and in front of our bomber.
//                    direction = EnemyDirection.Direction_Below;
//                    return true;
//                }
//
//                // Continue scanning (go down)
//                if (BeyondArenaFrontiers)
//                {
//                    // only increase FakeBlock if current block is not a bomb or a wall
//                    if (!IsBomb(BlockX, BlockY) &&
//                        (!IsWall(BlockX, BlockY) || !IsWallBreakable(BlockX, BlockY)))
//                        FakeBlockY++;
//                }
//                else
//                {
//                    // increase normally
//                    FakeBlockY++;
//                }
//                BlockY++;
//            }
//
//            // We scanned in every directions from the bomber.
//            // There is no enemy bomber near and in front of our bomber.
//            return false;
//        }
//
//        private bool DropBombOk(int blockX, int blockY)
//        {
//            int Depth;
//            int DangerBlockX;
//            int DangerBlockY;
//
//            // If the tested block is NOT accessible to our bomber
//            if (_accessible[blockX, blockY] == -1)
//            {
//                // Why would we drop a bomb here then??
//                return false;
//            }
//
//            if (IsBomb(blockX, blockY))
//            {
//                return false;
//            }
//
//            if (IsDanger(blockX, blockY))
//            {
//                if ((blockX - 1 < 0 || IsDanger(blockX - 1, blockY)) &&
//                    (blockX + 1 >= _player.Map.MapSize.x || IsDanger(blockX + 1, blockY)) &&
//                    (blockY - 1 < 0 || IsDanger(blockX, blockY - 1)) &&
//                    (blockX + 1 >= _player.Map.MapSize.y || IsDanger(blockX, blockY + 1)))
//                {
//                    return false;
//                }
//            }
//
//            // If a bomb is dropped on the tested block then of course one
//            // accessible block will be endangered since the tested block
//            // is accessible to our bomber.
//            int AccessibleEndangered = 1;
//
//            //-------------------------------------------------------------------------------------------
//            // Make a fuzzy estimation of the flame size of our bombs (more human than exact flame size)
//            //-------------------------------------------------------------------------------------------
//
//            // Get the exact flame size of our bombs
//            int FlameSize = (int)_player.Attr.bombArea;
//
//            // If it exceeds a certain size
//            if (FlameSize >= 4)
//            {
//                // According to the flame size, make the flame size bigger
//                switch (FlameSize)
//                {
//                    case 4: FlameSize = 5; break; // Flame size estimation error is low
//                    case 5: FlameSize = 7; break; // Flame size estimation error is medium
//                    case 6: FlameSize = 8; break; // Flame size estimation error is medium
//                    default: FlameSize = 99; break; // Flame size estimation error is high
//                }
//            }
//
//            // We are now going to simulate an explosion whose center
//            // would be on the tested block (function parameters).
//
//            //----------------------------------------------------------------------------------------
//            // Simulate the flame ray (right) of the explosion. Count how many accessible blocks 
//            // are endangered by this flame ray. Check if it won't hit a valuable item or a wall 
//            // that is burning or that will soon burn.
//            //----------------------------------------------------------------------------------------
//
//            // Start scanning one block to the right of the tested block
//            DangerBlockX = blockX + 1;
//            DangerBlockY = blockY;
//
//            // First block to scan
//            Depth = 0;
//
//            // While there is no obstacle (wall, bomb)
//            // and we didn't finish scanning the flame ray of the simulated explosion
//            while (!IsBomb(DangerBlockX, DangerBlockY) &&
//                   !IsWall(DangerBlockX, DangerBlockY) && !IsWallBreakable(DangerBlockX, DangerBlockY) &&
//                   Depth <= FlameSize)
//            {
//                // The block we are scanning is accessible and would be endangered by the bomb
//                AccessibleEndangered++;
//
//                // If there is an item on the block we are scanning
//                // and this item is at least a little bit interesting
//                //if (m_pArena->GetArena()->IsItem(DangerBlockX, DangerBlockY) &&
//                //    ItemMark(DangerBlockX, DangerBlockY) > 0)
//                //{
//                //    // We should not drop a bomb here, this item seems to be interesting
//                //    return false;
//                //}
//
//                // Continue scanning (go right)
//                DangerBlockX++;
//                Depth++;
//            }
//
//            // If the scan was stopped because of a wall
//            // and this wall is burning or will burn soon
//            //if (m_pArena->GetArena()->IsWall(DangerBlockX, DangerBlockY) &&
//            //    m_pArena->GetWallBurn(DangerBlockX, DangerBlockY))
//            //{
//            //    // We should not drop a bomb here, there could be an item under this wall!
//            //    return false;
//            //}
//
//            //----------------------------------------------------------------------------------------
//            // Simulate the flame ray (left) of the explosion. Count how many accessible blocks 
//            // are endangered by this flame ray. Check if it won't hit a valuable item or a wall 
//            // that is burning or that will soon burn.
//            //----------------------------------------------------------------------------------------
//
//            // Start scanning on this block
//            DangerBlockX = blockX - 1;
//            DangerBlockY = blockY;
//
//            // First block to scan
//            Depth = 0;
//
//            // While there is no obstacle (wall, bomb)
//            // and we didn't finish scanning the flame ray of the simulated explosion
//            while (!IsBomb(DangerBlockX, DangerBlockY) &&
//                   !IsWall(DangerBlockX, DangerBlockY) && !IsWallBreakable(DangerBlockX, DangerBlockY) &&
//                   Depth <= FlameSize)
//            {
//                // The block we are scanning is accessible and would be endangered by the bomb
//                AccessibleEndangered++;
//
//                // If there is an item on the block we are scanning
//                // and this item is at least a little bit interesting
//                //if (m_pArena->GetArena()->IsItem(DangerBlockX, DangerBlockY) &&
//                //    ItemMark(DangerBlockX, DangerBlockY) > 0)
//                //{
//                //    // We should not drop a bomb here, this item seems to be interesting
//                //    return false;
//                //}
//
//                // Continue scanning (go left)
//                DangerBlockX--;
//                Depth++;
//            }
//
//            // If the scan was stopped because of a wall
//            // and this wall is burning or will burn soon
//            //if (m_pArena->GetArena()->IsWall(DangerBlockX, DangerBlockY) &&
//            //    m_pArena->GetWallBurn(DangerBlockX, DangerBlockY))
//            //{
//            //    // We should not drop a bomb here, there could be an item under this wall!
//            //    return false;
//            //}
//
//            //----------------------------------------------------------------------------------------
//            // Simulate the flame ray (up) of the explosion. Count how many accessible blocks 
//            // are endangered by this flame ray. Check if it won't hit a valuable item or a wall 
//            // that is burning or that will soon burn.
//            //----------------------------------------------------------------------------------------
//
//            // Start scanning on this block
//            DangerBlockX = blockX;
//            DangerBlockY = blockY - 1;
//
//            // First block to scan
//            Depth = 0;
//
//            // While there is no obstacle (wall, bomb)
//            // and we didn't finish scanning the flame ray of the simulated explosion
//            while (!IsBomb(DangerBlockX, DangerBlockY) &&
//                   !IsWall(DangerBlockX, DangerBlockY) && !IsWallBreakable(DangerBlockX, DangerBlockY) &&
//                   Depth <= FlameSize)
//            {
//                // The block we are scanning is accessible and would be endangered by the bomb
//                AccessibleEndangered++;
//
//                // If there is an item on the block we are scanning
//                // and this item is at least a little bit interesting
//                //if (m_pArena->GetArena()->IsItem(DangerBlockX, DangerBlockY) &&
//                //    ItemMark(DangerBlockX, DangerBlockY) > 0)
//                //{
//                //    // We should not drop a bomb here, this item seems to be interesting
//                //    return false;
//                //}
//
//                // Continue scanning (go up)
//                DangerBlockY--;
//                Depth++;
//            }
//
//            // If the scan was stopped because of a wall
//            // and this wall is burning or will burn soon
//            //if (m_pArena->GetArena()->IsWall(DangerBlockX, DangerBlockY) &&
//            //    m_pArena->GetWallBurn(DangerBlockX, DangerBlockY))
//            //{
//            //    // We should not drop a bomb here, there could be an item under this wall!
//            //    return false;
//            //}
//
//            //----------------------------------------------------------------------------------------
//            // Simulate the flame ray (down) of the explosion. Count how many accessible blocks 
//            // are endangered by this flame ray. Check if it won't hit a valuable item or a wall 
//            // that is burning or that will soon burn.
//            //----------------------------------------------------------------------------------------
//
//            // Start scanning on this block
//            DangerBlockX = blockX;
//            DangerBlockY = blockY + 1;
//
//            // First block to scan
//            Depth = 0;
//
//            // While there is no obstacle (wall, bomb)
//            // and we didn't finish scanning the flame ray of the simulated explosion
//            while (!IsBomb(DangerBlockX, DangerBlockY) &&
//                   !IsWall(DangerBlockX, DangerBlockY) && !IsWallBreakable(DangerBlockX, DangerBlockY) &&
//                   Depth <= FlameSize)
//            {
//                // The block we are scanning is accessible and would be endangered by the bomb
//                AccessibleEndangered++;
//
//                // If there is an item on the block we are scanning
//                // and this item is at least a little bit interesting
//                //if (m_pArena->GetArena()->IsItem(DangerBlockX, DangerBlockY) &&
//                //    ItemMark(DangerBlockX, DangerBlockY) > 0)
//                //{
//                //    // We should not drop a bomb here, this item seems to be interesting
//                //    return false;
//                //}
//
//                // Continue scanning (go down)
//                DangerBlockY++;
//                Depth++;
//            }
//
//            // If the scan was stopped because of a wall
//            // and this wall is burning or will burn soon
//            //if (m_pArena->GetArena()->IsWall(DangerBlockX, DangerBlockY) &&
//            //    m_pArena->GetWallBurn(DangerBlockX, DangerBlockY))
//            //{
//            //    // We should not drop a bomb here, there could be an item under this wall!
//            //    return false;
//            //}
//
//            return (_numAccessible > AccessibleEndangered);
//        }
//
//        private void ModeDefense()
//        {
//            // If the AI is not in danger.
//            if (!IsDanger(_player.GridPos.x, _player.GridPos.y))
//            {
//                // Reset commands to send to the AI in order to stop moving when the AI is in a safe block.
//                _move = EnemyMove.Move_None;
//
//                // No need to defend, switch to think mode so as to decide what to do.
//                SetState(AIState.State_Think);
//
//                // Get out, this mode is over.
//                return;
//            }
//
//            // Assume we didn't find any good block to go to
//            bool Found = false;
//
//            // Coordinates of the best block to go to
//            int BestBlockX = -1;
//            int BestBlockY = -1;
//
//            int BlockX;
//            int BlockY;
//
//            // Distance of the best block to go to
//            int BestDistance = 999;
//
//            bool DeadEnd = true;
//
//            // Scan the blocks of the AI view
//            for (BlockX = _player.GridPos.x - AI_VIEW_SIZE; BlockX < _player.GridPos.x + AI_VIEW_SIZE; BlockX++)
//            {
//                for (BlockY = _player.GridPos.y - AI_VIEW_SIZE; BlockY < _player.GridPos.y + AI_VIEW_SIZE; BlockY++)
//                {
//                    // If the block is outside the arena
//                    if (BlockX < 0 || BlockX > _player.Map.MapSize.x - 1 ||
//                        BlockY < 0 || BlockY > _player.Map.MapSize.y - 1)
//                    {
//                        // Next block in the AI view
//                        continue;
//                    }
//                    // If the block is inside the arena
//                    else
//                    {
//                        // If this block is accessible
//                        // and this block is not in danger
//                        // and this block is closer than the closest good block we saved
//                        if (_accessible[BlockX, BlockY] != -1 &&
//                            (EnemyNear(BlockX, BlockY) ? GetDeadEnd(BlockX, BlockY) == -1 : (GetDeadEnd(BlockX, BlockY) != -1 || !DeadEnd)) &&
//                            !IsDanger(BlockX, BlockY)
//                            &&
//                                (
//                                    _accessible[BlockX, BlockY] < BestDistance
//                                    ||
//                                    (
//                                        _accessible[BlockX, BlockY] == BestDistance
//                                        &&
//                                        _rand.Next(100) >= 50
//                                    )
//                                )
//                            )
//                        {
//                            // We found a good block to go to
//                            Found = true;
//
//                            // Save the coordinates and the distance of this block
//                            BestBlockX = BlockX;
//                            BestBlockY = BlockY;
//                            BestDistance = _accessible[BlockX, BlockY];
//                            DeadEnd = (GetDeadEnd(BlockX, BlockY) != -1);
//                        }
//                    }
//                }
//            }
//
//            if (!Found)
//            {
//                //------------------------------------
//                // Determine the less dangerous block
//                //------------------------------------
//
//                // Scan the blocks of the AI view
//                for (BlockX = _player.GridPos.x - AI_VIEW_SIZE; BlockX < _player.GridPos.x + AI_VIEW_SIZE; BlockX++)
//                {
//                    for (BlockY = _player.GridPos.y - AI_VIEW_SIZE; BlockY < _player.GridPos.y + AI_VIEW_SIZE; BlockY++)
//                    {
//                        // If the block is outside the arena
//                        if (BlockX < 0 || BlockX > _player.Map.MapSize.x - 1 ||
//                            BlockY < 0 || BlockY > _player.Map.MapSize.y - 1)
//                        {
//                            // Next block in the AI view
//                            continue;
//                        }
//                        // If the block is inside the arena
//                        else
//                        {
//                            // If this block is accessible
//                            // and this block is not in danger
//                            // and this block is closer than the closest good block we saved
//                            if (_accessible[BlockX, BlockY] != -1 /*&&
//                                m_pArena->GetDangerTimeLeft(BlockX, BlockY) > BestDangerTimeLeft*/)
//                            {
//                                // We found a good block to go to
//                                Found = true;
//
//                                // Save the coordinates and the distance of this block
//                                BestBlockX = BlockX;
//                                BestBlockY = BlockY;
//                                BestDistance = _accessible[BlockX, BlockY];
//
//                                //BestDangerTimeLeft = m_pArena->GetDangerTimeLeft(BlockX, BlockY);
//                            }
//                        }
//                    }
//                }
//            }
//
//            // If a good block to go to was found
//            if (Found)
//            {
//                // Set the bomber move to send to the bomber so that the bomber goes to the best block
//                GoTo(BestBlockX, BestBlockY);
//            }
//            // If a good block to go to was not found
//            else
//            {
//                _move = EnemyMove.Move_None;
//            }
//        }
//
//        private void ModeThink()
//        {
//            // Get current position of the AI.
//            int blockX = _player.GridPos.x;
//            int blockY = _player.GridPos.y;
//
//            // Check if AI is in danger.
//            if (IsDanger(blockX, blockY) ||
//                IsDanger(blockX, blockY - 1) ||
//                IsDanger(blockX, blockY + 1) ||
//                IsDanger(blockX - 1, blockY) ||
//                IsDanger(blockX + 1, blockY))
//            {
//                SetState(AIState.State_Defense);
//                return;
//            }
//
//            EnemyDirection enemyDirection = EnemyDirection.Direction_Unknown;
//
//            //---------------------------------------------------------
//            // Check if we should to attack.
//            // Attack when there is an enemy near and in front of you.
//            //---------------------------------------------------------
//            // If there is an enemy near and in front of our bomber
//            // and it is ok to drop a bomb where our bomber is
//            // with quite big probability (not beyond the frontiers)
//            if (EnemyNearAndFront(ref enemyDirection, false) &&
//                DropBombOk(_player.GridPos.x, _player.GridPos.y) &&
//                _rand.Next(100) < 70)
//            {
//                // Switch to the attack mode to drop a bomb
//                SetState(AIState.State_Attack);
//
//                // OK, get out since we decided what to do
//                return;
//            }
//
//            // Check if has some item near in ai view.
//            for (int x = _player.GridPos.x - AI_VIEW_SIZE; x < _player.GridPos.x + AI_VIEW_SIZE; x++)
//            {
//                for (int y = _player.GridPos.y - AI_VIEW_SIZE; y < _player.GridPos.y + AI_VIEW_SIZE; y++)
//                {
//                    // If block is outside of the map.
//                    if (x < 0 || x > _player.Map.MapSize.x - 1 ||
//                        y < 0 || y > _player.Map.MapSize.y - 1)
//                    {
//                        continue;
//                    }
//
//                    if (IsItem(x, y))
//                    {
//                        // Found item in current position.
//                        _goal.x = (short)x;
//                        _goal.y = (short)y;
//
//                        SetState(AIState.State_Item);
//
//                        // Already found state for the AI, let to quit from here.
//                        return;
//                    }
//                }
//            }
//
//            /////////////////////////////
//            int bestMark = 0;
//            int bestGoalBlockX = 0;
//            int bestGoalBlockY = 0;
//
//            // Scan the blocks of the AI view.
//            for (var x = _player.GridPos.x - AI_VIEW_SIZE; x < _player.GridPos.x + AI_VIEW_SIZE; x++)
//            {
//                for (var y = _player.GridPos.y - AI_VIEW_SIZE; y < _player.GridPos.y + AI_VIEW_SIZE; y++)
//                {
//                    if (x < 0 || x >= _player.Map.MapSize.x - 1 ||
//                        y < 0 || y >= _player.Map.MapSize.y - 1)
//                    {
//                        continue;
//                    }
//                    else
//                    {
//                        if (GetSoftWallNear(x, y) != -1 &&
//                            GetSoftWallNear(x, y) > 0 &&
//                            GetSoftWallNear(x, y) < _burnMark.GetLength(0) &&
//
//                            _accessible[x, y] != -1 &&
//                            _accessible[x, y] <= 5 &&
//                            _accessible[x, y] < _burnMark.GetLength(1) &&
//                            !IsDanger(x, y)
//                            &&
//                                (
//                                    bestMark < _burnMark[GetSoftWallNear(x, y), _accessible[x, y]]
//                                    ||
//                                    (
//                                    bestMark == _burnMark[GetSoftWallNear(x, y), _accessible[x, y]]
//                                    &&
//                                    _rand.Next(100) >= 50
//                                    )
//                                ) &&
//                                DropBombOk(x, y))
//                        {
//                            // Save the coordinates of the best block
//                            bestGoalBlockX = x;
//                            bestGoalBlockY = y;
//
//                            bestMark = _burnMark[GetSoftWallNear(x, y), _accessible[x, y]];
//                        }
//                    }
//                }
//            }
//
//            // If we found a good block to go to
//            if (bestMark > 0)
//            {
//                _goal.x = (short)bestGoalBlockX;
//                _goal.y = (short)bestGoalBlockY;
//
//                // Drop a bomb when you reach the goal,
//                // because you have to burn some walls.
//                _itemDropBomb = true;
//
//                SetState(AIState.State_Item);
//
//                return;
//            }
//
//            // Nothing better to do than walking in random directions.
//            // Switch to walk mode.
//            SetState(AIState.State_Walk);
//        }
//
//        private void ModeWalk(float delta)
//        {
//            int blockX = 0;
//            int blockY = 0;
//
//            // Mark's
//            int markDownRight = 0;
//            int markDownLeft = 0;
//            int markUpLeft = 0;
//            int markUpRight = 0;
//
//            for (blockX = _player.GridPos.x; blockX < _player.Map.MapSize.x; blockX++)
//            {
//                for (blockY = _player.GridPos.y; blockY < _player.Map.MapSize.y; blockY++)
//                {
//                    // Ignore current position of the player.
//                    if (blockX == _player.GridPos.x &&
//                        blockY == _player.GridPos.y)
//                    {
//                        continue;
//                    }
//
//                    if (IsWallBreakable(blockX, blockY))
//                        markDownRight += 2;
//                    else if (IsItem(blockX, blockY))
//                        markDownRight += 10;
//                    else if (IsPlayer(blockX, blockY))
//                        markDownRight += 5;
//                }
//            }
//
//            for (blockX = _player.GridPos.x; blockX >= 0; blockX--)
//            {
//                for (blockY = _player.GridPos.y; blockY < _player.Map.MapSize.y; blockY++)
//                {
//                    // Ignore current position of the player.
//                    if (blockX == _player.GridPos.x &&
//                        blockY == _player.GridPos.y)
//                    {
//                        continue;
//                    }
//
//                    if (IsWallBreakable(blockX, blockY))
//                        markDownLeft += 2;
//                    else if (IsItem(blockX, blockY))
//                        markDownLeft += 10;
//                    else if (IsPlayer(blockX, blockY))
//                        markDownLeft += 5;
//                }
//            }
//
//            for (blockX = _player.GridPos.x; blockX >= 0; blockX--)
//            {
//                for (blockY = _player.GridPos.y; blockY >= 0; blockY--)
//                {
//                    // Ignore current position of the player.
//                    if (blockX == _player.GridPos.x &&
//                        blockY == _player.GridPos.y)
//                    {
//                        continue;
//                    }
//
//                    if (IsWallBreakable(blockX, blockY))
//                        markUpLeft += 2;
//                    else if (IsItem(blockX, blockY))
//                        markUpLeft += 10;
//                    else if (IsPlayer(blockX, blockY))
//                        markUpLeft += 5;
//                }
//            }
//
//            for (blockX = _player.GridPos.x; blockX < _player.Map.MapSize.x; blockX++)
//            {
//                for (blockY = _player.GridPos.y; blockY >= 0; blockY--)
//                {
//                    // Ignore current position of the player.
//                    if (blockX == _player.GridPos.x &&
//                        blockY == _player.GridPos.y)
//                    {
//                        continue;
//                    }
//
//                    if (IsWallBreakable(blockX, blockY))
//                        markUpRight += 2;
//                    else if (IsItem(blockX, blockY))
//                        markUpRight += 10;
//                    else if (IsPlayer(blockX, blockY))
//                        markUpRight += 5;
//                }
//            }
//
//            // Check if all mark are null, if is nulled, bye!
//            if (markDownRight == 0 &&
//                markDownLeft == 0 &&
//                markUpLeft == 0 &&
//                markUpRight == 0)
//            {
//                return;
//            }
//
//            // Update variables for current position of the AI.
//            blockX = _player.GridPos.x;
//            blockY = _player.GridPos.y;
//
//            // Is a move in a single direction possible?
//            bool canMoveUp = !IsWall(blockX, blockY + 1) && !IsWall(blockX, blockY + 1);
//            bool canMoveDown = !IsWall(blockX, blockY - 1) && !IsWall(blockX, blockY - 1);
//            bool canMoveRight = !IsWall(blockX + 1, blockY) && !IsWall(blockX + 1, blockY);
//            bool canMoveLeft = !IsWall(blockX - 1, blockY) && !IsWall(blockX - 1, blockY);
//
//            bool dangerUp = IsDanger(blockX, blockY + 1);
//            bool dangerDown = IsDanger(blockX, blockY - 1);
//            bool dangerRight = IsDanger(blockX + 1, blockY);
//            bool dangerLeft = IsDanger(blockX - 1, blockY);
//
//            if (markDownRight >= markDownLeft &&
//                markDownLeft >= markUpLeft &&
//                markDownLeft >= markUpRight)
//            {
//                if (_rand.Next(100) >= 50)
//                {
//                    if (!dangerDown && _move != EnemyMove.Move_Up && canMoveDown)
//                        _move = EnemyMove.Move_Down;
//                    else if (!dangerRight && _move != EnemyMove.Move_Left && canMoveRight)
//                        _move = EnemyMove.Move_Right;
//                    else if (!dangerUp && _move != EnemyMove.Move_Down && canMoveUp)
//                        _move = EnemyMove.Move_Up;
//                    else if (!dangerLeft && _move != EnemyMove.Move_Right && canMoveLeft)
//                        _move = EnemyMove.Move_Left;
//                    else
//                        _move = EnemyMove.Move_None;
//                }
//                else
//                {
//                    if (!dangerRight && _move != EnemyMove.Move_Left && canMoveRight)
//                        _move = EnemyMove.Move_Right;
//                    else if (!dangerDown && _move != EnemyMove.Move_Up && canMoveDown)
//                        _move = EnemyMove.Move_Down;
//                    else if (!dangerLeft && _move != EnemyMove.Move_Right && canMoveLeft)
//                        _move = EnemyMove.Move_Left;
//                    else if (!dangerUp && _move != EnemyMove.Move_Down && canMoveUp)
//                        _move = EnemyMove.Move_Up;
//                    else
//                        _move = EnemyMove.Move_None;
//                }
//            }
//            else if (markDownLeft >= markDownRight &&
//                markDownLeft >= markUpLeft &&
//                markDownLeft >= markUpRight)
//            {
//                if (_rand.Next(100) >= 50)
//                {
//                    if (!dangerDown && _move != EnemyMove.Move_Up && canMoveDown)
//                        _move = EnemyMove.Move_Down;
//                    else if (!dangerLeft && _move != EnemyMove.Move_Right && canMoveLeft)
//                        _move = EnemyMove.Move_Left;
//                    else if (!dangerUp && _move != EnemyMove.Move_Down && canMoveUp)
//                        _move = EnemyMove.Move_Up;
//                    else if (!dangerRight && _move != EnemyMove.Move_Left && canMoveRight)
//                        _move = EnemyMove.Move_Right;
//                    else
//                        _move = EnemyMove.Move_None;
//                }
//                else
//                {
//                    if (!dangerLeft && _move != EnemyMove.Move_Right && canMoveLeft)
//                        _move = EnemyMove.Move_Left;
//                    else if (!dangerDown && _move != EnemyMove.Move_Up && canMoveDown)
//                        _move = EnemyMove.Move_Down;
//                    else if (!dangerRight && _move != EnemyMove.Move_Left && canMoveLeft)
//                        _move = EnemyMove.Move_Right;
//                    else if (!dangerUp && _move != EnemyMove.Move_Down && canMoveUp)
//                        _move = EnemyMove.Move_Up;
//                    else
//                        _move = EnemyMove.Move_None;
//                }
//            }
//            else if (markUpLeft >= markDownRight &&
//                markUpLeft >= markDownLeft &&
//                markUpLeft >= markUpRight)
//            {
//                if (_rand.Next(100) >= 50)
//                {
//                    if (!dangerUp && _move != EnemyMove.Move_Down && canMoveUp)
//                        _move = EnemyMove.Move_Up;
//                    else if (!dangerLeft && _move != EnemyMove.Move_Right && canMoveLeft)
//                        _move = EnemyMove.Move_Left;
//                    else if (!dangerDown && _move != EnemyMove.Move_Up && canMoveDown)
//                        _move = EnemyMove.Move_Down;
//                    else if (!dangerRight && _move != EnemyMove.Move_Left && canMoveRight)
//                        _move = EnemyMove.Move_Right;
//                    else
//                        _move = EnemyMove.Move_None;
//                }
//                else
//                {
//                    if (!dangerLeft && _move != EnemyMove.Move_Right && canMoveLeft)
//                        _move = EnemyMove.Move_Left;
//                    else if (!dangerUp && _move != EnemyMove.Move_Down && canMoveUp)
//                        _move = EnemyMove.Move_Up;
//                    else if (!dangerRight && _move != EnemyMove.Move_Left && canMoveLeft)
//                        _move = EnemyMove.Move_Right;
//                    else if (!dangerDown && _move != EnemyMove.Move_Up && canMoveDown)
//                        _move = EnemyMove.Move_Down;
//                    else
//                        _move = EnemyMove.Move_None;
//                }
//            }
//            else
//            {
//                if (_rand.Next(100) >= 50)
//                {
//                    if (!dangerUp && _move != EnemyMove.Move_Down && canMoveUp)
//                        _move = EnemyMove.Move_Up;
//                    else if (!dangerRight && _move != EnemyMove.Move_Left && canMoveRight)
//                        _move = EnemyMove.Move_Right;
//                    else if (!dangerDown && _move != EnemyMove.Move_Up && canMoveDown)
//                        _move = EnemyMove.Move_Down;
//                    else if (!dangerLeft && _move != EnemyMove.Move_Right && canMoveUp)
//                        _move = EnemyMove.Move_Left;
//                    else
//                        _move = EnemyMove.Move_None;
//                }
//                else
//                {
//                    if (!dangerRight && _move != EnemyMove.Move_Left && canMoveRight)
//                        _move = EnemyMove.Move_Right;
//                    else if (!dangerUp && _move != EnemyMove.Move_Down && canMoveUp)
//                        _move = EnemyMove.Move_Up;
//                    else if (!dangerLeft && _move != EnemyMove.Move_Right && canMoveLeft)
//                        _move = EnemyMove.Move_Left;
//                    else if (!dangerDown && _move != EnemyMove.Move_Up && canMoveDown)
//                        _move = EnemyMove.Move_Down;
//                    else
//                        _move = EnemyMove.Move_None;
//                }
//            }
//
//            // Move the dummy.
//            if (_move != EnemyMove.Move_None)
//            {
//                // Call function for AI to move.
//                Move();
//
//                // Set current state now for to think.
//                SetState(AIState.State_Think);
//            }
//        }
//
//        private void SetTarget()
//        {
//            // AI already has a target selected.
//            if (_targetUID != 0)
//                return;
//
//            // Search a target near.
//            GetNearTarget();
//
//            // Check if AI already selected your target.
//            if (_targetUID != 0)
//            {
//
//            }
//        }
//
//        public void Move()
//        {
//            if (_move == EnemyMove.Move_None)
//                return;
//
//            switch (_move)
//            {
//                case EnemyMove.Move_Up:
//                    _player.Move(0, 1);
//                    break;
//                case EnemyMove.Move_Down:
//                    _player.Move(0, -1);
//                    break;
//                case EnemyMove.Move_Left:
//                    _player.Move(-1, 0);
//                    break;
//                case EnemyMove.Move_Right:
//                    _player.Move(1, 0);
//                    break;
//            }
//        }
//
//        private int ItemMark(int BlockX, int BlockY)
//        {
//            if (!IsItem(BlockX, BlockY) ||
//                _accessible[BlockX, BlockY] == -1 ||
//                (IsDanger(BlockX, BlockY) && _accessible[BlockX, BlockY] >= 3))
//            {
//                // Worst mark, this item is not interesting
//                return 0;
//            }
//
//            // Initialize item mark to return to zero.
//            int Mark = 0;
//
//            // Get all power's up of the map.
//            var powerUp = _player.Map.FindAllByType<PowerUp>();
//            if (powerUp?.Count <= 0)
//            {
//                // No have some power up in current map.
//                return 0;
//            }
//            else
//            {
//                foreach (PowerUp power in powerUp)
//                {
//                    if (power.IsLive && power.IsOnMap &&
//                        power.GridPos.x == BlockX &&
//                        power.GridPos.y == BlockY)
//                    {
//                        // TODO HERE.
//
//
//
//
//                        //--------------------------------------------------------------
//                        // Take other details of this item into account (distance, etc)
//                        //--------------------------------------------------------------
//
//                        // If the item is near our bomber
//                        if (_accessible[BlockX, BlockY] <= 3)
//                        {
//                            // This item is much more interesting, increase the mark
//                            Mark += 5;
//                        }
//                        // If the item is not very far away from our bomber
//                        else if (_accessible[BlockX, BlockY] <= 6)
//                        {
//                            // This item is quite interesting, increase the mark
//                            Mark += 3;
//                        }
//
//                        // If the item is not in a dead end
//                        if (GetDeadEnd(BlockX, BlockY) == -1)
//                        {
//                            // Increase the item mark
//                            Mark += 10;
//                        }
//
//                        // OK, we estimated how good it would be to pick up this item.
//                        // Return the item mark.
//                        return Mark;
//                    }
//                }
//            }
//
//
//            return 0;
//        }
//
//        /// <summary>
//        /// Check if current position of the grid is a Wall.
//        /// </summary>
//        /// <param name="x">Position X of the Grid.</param>
//        /// <param name="y">Position Y of the Grid.</param>
//        /// <returns>If current of the position x and y in grid is a wall, will return true.</returns>
//        private bool IsWall(int x, int y)
//        {
//            var cell = _player.Map[_player.Map.ToIndex(x, y)];
//            return cell.HasAttribute(CellAttributes.WALL);
//        }
//
//        /// <summary>
//        /// Check if current position of the grid is a Item.
//        /// </summary>
//        /// <param name="x">Position X of the Grid.</param>
//        /// <param name="y">Position Y of the Grid.</param>
//        /// <returns>If current of the position x and y in grid is an item, will return true.</returns>
//        private bool IsItem(int x, int y)
//        {
//            var cell = _player.Map[_player.Map.ToIndex(x, y)];
//            return cell.FindFirstByType(ObjectType.POWERUP) != null ? true : false;
//        }
//
//        /// <summary>
//        /// Check if current position of the grid is a Wall Breakable.
//        /// </summary>
//        /// <param name="x">Position X of the Grid.</param>
//        /// <param name="y">Position Y of the Grid.</param>
//        /// <returns>If current of the position x and y in grid is a wall breakable, will return true.</returns>
//        private bool IsWallBreakable(int x, int y)
//        {
//            var cell = _player.Map[_player.Map.ToIndex(x, y)];
//            return cell.HasAttribute(CellAttributes.BREAKABLE);
//        }
//
//        /// <summary>
//        /// Check if current position of the grid is a Bomb.
//        /// </summary>
//        /// <param name="x">Position X of the Grid.</param>
//        /// <param name="y">Position Y of the Grid.</param>
//        /// <returns>If current of the position x and y in grid is a bomb, will return true.</returns>
//        private bool IsBomb(int x, int y)
//        {
//            var cell = _player.Map[_player.Map.ToIndex(x, y)];
//            return cell.FindFirstByType(ObjectType.BOMB) != null ? true : false;
//        }
//
//        /// <summary>
//        /// Check if current position of the grid is a Player.
//        /// </summary>
//        /// <param name="x">Position X of the Grid.</param>
//        /// <param name="y">Position Y of the Grid.</param>
//        /// <returns>If current of the position x and y in grid is a player, will return true.</returns>
//        private bool IsPlayer(int x, int y)
//        {
//            if (_player.Map is Room room)
//            {
//                var players = room.FindAllByType<Player>();
//                if (players?.Count <= 0)
//                {
//                    return false;
//                }
//                else
//                {
//                    foreach (Player player in players)
//                    {
//                        if (player.UID != _player.UID && player.IsLive &&
//                            player.GridPos.x == x && player.GridPos.y == y)
//                        {
//                            return true;
//                        }
//                    }
//                }
//            }
//
//            return false;
//        }
//
//        /// <summary>
//        /// Check if in this current position, some bomb will explode and will reach this current position on the grid.
//        /// </summary>
//        /// <param name="x">Position X of the Grid.</param>
//        /// <param name="y">Position Y of the Grid.</param>
//        /// <returns>If some bomb will reach the current position of the grid, will return true.</returns>
//        private bool IsDanger(int blockX, int blockY)
//        {
//            var bombs = _player.Map.FindAllByType<Bomb>();
//
//            // No bomb found in map.
//            if (bombs.Count <= 0)
//            {
//                return false;
//            }
//            else
//            {
//                // Check all bombs.
//                foreach (Bomb bomb in bombs)
//                {
//                    int bombBlockX = bomb.GridPos.x;
//                    int bombBlockY = bomb.GridPos.y;
//
//                    for (var power = 0; power < bomb.Area; power++)
//                    {
//                        if (bombBlockX == blockX && bombBlockY - (1 * power) == blockY)
//                            return true;
//                        else if (bombBlockX == blockX && bombBlockY + (1 * power) == blockY)
//                            return true;
//                        else if (bombBlockX - (1 * power) == blockX && bombBlockY == blockY)
//                            return true;
//                        else if (bombBlockX + (1 * power) == blockX && bombBlockY == blockY)
//                            return true;
//                    }
//                }
//            }
//
//            return false;
//        }
//
//        private bool IsAccessible(int blockX, int blockY)
//        {
//            if (IsWall(blockX, blockY) ||
//               IsWallBreakable(blockX, blockY) ||
//               IsBomb(blockX, blockY))
//            {
//                return false;
//            }
//
//            return true;
//        }
//    }
//}
