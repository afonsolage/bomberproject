using CommonLib.GridEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Engine.Logic
{
    public class Global
    {
        #region ENUMERATIONS
        public enum MoveDirection
        {
            STAND = 0,
            UP,
            RIGHT,
            DOWN,
            LEFT,

            MAX
        }
        #endregion // ENUMERATIONS

        public static MoveDirection GridDirToMoveDirection(GridDir dir)
        {
            switch (dir)
            {
                case GridDir.RIGHT: return MoveDirection.RIGHT;
                case GridDir.LEFT: return MoveDirection.LEFT;
                case GridDir.UP: return MoveDirection.UP;
                case GridDir.DOWN: return MoveDirection.DOWN;
                default: return MoveDirection.DOWN;
            }
        }

        public static Vector3 GetDirectionVector(MoveDirection direction)
        {
            switch (direction)
            {
                case MoveDirection.RIGHT: return Vector3.right;
                case MoveDirection.LEFT: return Vector3.left;
                case MoveDirection.DOWN: return Vector3.down;
                case MoveDirection.UP: return Vector3.up;
                default: return Vector3.zero;
            }
        }

        public static Quaternion GetRotationByDirection(MoveDirection direction)
        {
            switch(direction)
            {
                case MoveDirection.UP: return Quaternion.Euler(0, 0, 0);
                case MoveDirection.DOWN: return Quaternion.Euler(0, 180, 0);
                case MoveDirection.RIGHT: return Quaternion.Euler(0, 90, 0);
                case MoveDirection.LEFT: return Quaternion.Euler(0, -90, 0);
            }

            return Quaternion.Euler(0, 0, 0);
        }
    }
}

