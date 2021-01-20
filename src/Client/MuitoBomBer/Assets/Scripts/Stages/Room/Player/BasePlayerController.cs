using Assets.Scripts.Engine.Logic.Object;
using CommonLib.GridEngine;
using Engine.Logic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerAnimation
{
    IDLE = 0,
    WALKING,
}

public class GridAnimator
{
    public delegate Animator GetAnimator();

    protected readonly GameObject _obj;

    protected Animator _animator;
    protected Animator Animator
    {
        get
        {
            if (_animator == null)
                _animator = _obj.GetComponent<Animator>();

            return _animator;
        }
    }

    protected GridObject _gridObject;
    protected GridObject GridObject
    {
        get
        {
            if (_gridObject == null)
                _gridObject = _obj.GetComponent<GridObjectInstance>()?.GridObject;

            return _gridObject;
        }
    }

    protected Transform _transform;
    protected Transform Transform
    {
        get
        {
            if (_transform == null)
            {
                _transform = _obj.transform.Find("Graphic");
            }

            return _transform;
        }
    }

    public GridAnimator(GameObject obj)
    {
#if DEBUG
        Debug.Assert(obj != null);
#endif
        _obj = obj;
    }

    protected void SetRotation(PlayerAnimation dir)
    {
        var facing = Global.GridDirToMoveDirection(GridObject.Facing);

        Transform.rotation = Global.GetRotationByDirection(facing);
    }

    public void Play(PlayerAnimation animation)
    {
        SetRotation(animation);

        switch (animation)
        {
            case PlayerAnimation.IDLE:
                {
                    Animator.SetBool("MOVING", false);
                }
                break;
            case PlayerAnimation.WALKING:
                {
                    Animator.SetBool("MOVING", true);
                }
                break;
        }
    }
}

[RequireComponent(typeof(GridObjectInstance))]
[RequireComponent(typeof(Animator))]
public class BasePlayerController : MonoBehaviour
{
    [Header("Base")]
    public float moveSpeed = 2f;

    [HideInInspector]
    public bool isMoving = false;

    protected GridObjectInstance _gridObj;
    protected GridAnimator _gridAnimator;

    protected RoomStage _stage;

    public Transform _graphicPlayer;

    protected virtual void Start()
    {
        _gridObj = GetComponent<GridObjectInstance>();
        _stage = StageManager.GetCurrent<RoomStage>();
        _gridAnimator = new GridAnimator(gameObject);
    }

    public void PlayAnimation(PlayerAnimation animation)
    {
        if (_gridAnimator == null)
        {
            Debug.LogError("Grid Animator is null.");
            return;
        }

        _gridAnimator.Play(animation);
    }

    public virtual bool Move(Global.MoveDirection direction)
    {
        isMoving = (direction != Global.MoveDirection.STAND) ? true : false;

        if (isMoving)
        {
            Vector3 directionVector = Global.GetDirectionVector(direction);
            directionVector.z = Mathf.RoundToInt(directionVector.y);

            var moveAmount = directionVector * moveSpeed * Time.smoothDeltaTime;
            var b4 = _gridObj.GridObject.WorldPos;
            _gridObj.Move(moveAmount.x, moveAmount.y);

            var moved = b4 != _gridObj.GridObject.WorldPos;

            // Check if player has attribute to kick bomb.
            if (!moved && _stage.ObjectManager.MainPlayer.Attr.kickBomb)
            {
                KickBomb(direction, directionVector);
            }

        }

        _gridAnimator.Play((isMoving) ? PlayerAnimation.WALKING : PlayerAnimation.IDLE);

        return isMoving;
    }

    public void KickBomb(Global.MoveDirection direction, Vector3 directionVector)
    {
        int blockX = _gridObj.GridObject.GridPos.x;
        int blockY = _gridObj.GridObject.GridPos.y;

        var map = _gridObj.GridObject.Map;
        if (map != null)
        {
            var bombs = map.FindAllByType<Bomb>();
            if (bombs?.Count > 0)
            {
                foreach (Bomb bomb in bombs)
                {
                    if (bomb.GridPos.x == blockX + directionVector.x &&
                        bomb.GridPos.y == blockY + directionVector.y)
                    {
                        GridDir dir = GridDir.UP;

                        switch (direction)
                        {
                            case Global.MoveDirection.RIGHT: dir = GridDir.RIGHT; break;
                            case Global.MoveDirection.LEFT: dir = GridDir.LEFT; break;
                            case Global.MoveDirection.DOWN: dir = GridDir.DOWN; break;
                            case Global.MoveDirection.UP: dir = GridDir.UP; break;
                        }

                        if (this is PlayerController)
                        {
                            //Before asking to kick, let's update our position on server.
                            (this as PlayerController).PosSync.SyncNow();
                        }
                        bomb.Kick(_gridObj.GridObject.UID, dir);
                    }
                }
            }
        }
    }
}
