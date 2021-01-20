using CommonLib.GridEngine;
using CommonLib.Util.Math;
using CommonLib.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(GridObjectInstance))]
public class GridRemotePosSync : MonoBehaviour
{
    public float moveSpeed = 1f;

    private Queue<Vec2f> _moveQueue = new Queue<Vec2f>();
    private GridObject _obj;
    private GridAnimator _gridAnimator;

    private Vec2f _dest;
    private Vec2f _lastAddedDist;

    public bool PlayAnimation = true;

    // Use this for initialization
    void Start()
    {
        _obj = GetComponent<GridObjectInstance>().GridObject;
        _dest = Vec2f.INVALID;

        if (PlayAnimation)
            _gridAnimator = new GridAnimator(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        MoveToDest(_dest);
    }

    private void MoveToDest(Vec2f dest)
    {
        if (!dest.IsValid())
        {
            GetNextDestination();
            return;
        }

        var dir = dest - _obj.WorldPos;

        var distance = dir.Magnitude();
        var accumDist = (_obj.WorldPos - _lastAddedDist).Magnitude();
        
        if (distance < Vec2f.EPSILON //If the distance to travel is irrelevant
            || distance > 1 //If the distance to travel is higher than 1 m
            || (_dest != _lastAddedDist && accumDist > 1)) //If we have lag, wrap it to destination.
        {
            _obj.Wrap(dest);
            DestinationReached();
            return;
        }

        dir.Normalize();

        var speed = moveSpeed;

        //Increase the move speed based on the number of pending packets.
        //This will cause that effect of fast forwarding when there are a lot of pending packets (due to lag)
        speed += 0.5f * GetQueueSize();

        var move = dir * Time.deltaTime * speed;

        move = MoveClamp(move, dir, _obj.WorldPos, dest);

        _obj.ForceMove(move.x, move.y);

        if (PlayAnimation)
            _gridAnimator.Play(PlayerAnimation.WALKING);
    }

    private int GetQueueSize()
    {
        lock (_moveQueue)
        {
            return _moveQueue.Count;
        }
    }

    private void DestinationReached()
    {
        if (_moveQueue.Count == 0)
        {
            _dest = Vec2f.INVALID;

            if (PlayAnimation)
                _gridAnimator.Play(PlayerAnimation.IDLE);
        }
        else
        {
            GetNextDestination();
        }
    }

    private Vec2f MoveClamp(Vec2f move, Vec2f dir, Vec2f cur, Vec2f dest)
    {
        if (dir.x > 0 && cur.x + move.x > dest.x)
        {
            move.x = dest.x - cur.x;
        }
        else if (dir.x < 0 && cur.x + move.x < dest.x)
        {
            move.x = dest.x - cur.x;
        }

        if (dir.y > 0 && cur.y + move.y > dest.y)
        {
            move.y = dest.y - cur.y;
        }
        else if (dir.y < 0 && cur.y + move.y < dest.y)
        {
            move.y = dest.y - cur.y;
        }

        return move;
    }

    private void GetNextDestination()
    {
        lock (_moveQueue)
        {
            if (_moveQueue.Count == 0)
                return;
            else
                _dest = _moveQueue.Dequeue();
        }
    }

    public void AddMoveDest(Vec2f dest)
    {
        lock (_moveQueue)
        {
            _moveQueue.Enqueue(dest);

            _lastAddedDist = dest;
        }
    }
}
