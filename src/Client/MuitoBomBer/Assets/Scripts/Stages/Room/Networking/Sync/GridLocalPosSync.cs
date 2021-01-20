using CommonLib.GridEngine;
using CommonLib.Util.Math;
using CommonLib.Messaging.Client;
using CommonLib.Messaging.Common;
using CommonLib.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GridObjectInstance))]
public class GridLocalPosSync : MonoBehaviour
{
    public float syncThreshold = 0.25f;

    private GridObjectInstance _instance;
    private Vec2f _lastSyncPos = Vec2f.INVALID;

    private RoomStage _stage;

    // Use this for initialization
    void Start()
    {
        _instance = GetComponent<GridObjectInstance>();
        _stage = StageManager.GetCurrent<RoomStage>();
    }

    // Update is called once per frame
    void Update()
    {
        SyncPosition();
    }

    private void SyncPosition()
    {
        var obj = _instance.GridObject;

        if (obj == null)
            return;

        if (!_lastSyncPos.IsValid())
        {
            _lastSyncPos = obj.WorldPos;
            return;
        }

        var threshold = obj.WorldPos - _lastSyncPos;

        if (threshold.Magnitude() > syncThreshold)
        {
            SyncNow();
        }
    }

    internal void SyncNow()
    {
        var obj = _instance.GridObject;

        var force = obj.WorldPos - _lastSyncPos;

        if (force.x == force.y)
        {
            return;
        }

        _lastSyncPos = obj.WorldPos;

        if (Mathf.Abs(force.x) < Vec2f.EPSILON)
        {
            force.x = 0;
        }
        else if (Mathf.Abs(force.y) < Vec2f.EPSILON)
        {
            force.y = 0;
        }

        //If player moved on two directions, since last move, let's split the message in two movements.
        if (force.x != 0 && force.y != 0)
        {
            bool xAxisFirst = (obj.Facing == GridDir.UP || obj.Facing == GridDir.DOWN);

            if (xAxisFirst)
            {
                _stage.ServerConnection.Send(new CR_PLAYER_MOVE_SYNC_NFY()
                {
                    moveX = force.x,
                    moveY = 0,
                    currentWorldPos = new VEC2()
                    {
                        x = obj.WorldPos.x,
                        y = obj.WorldPos.y,
                    },
                });
                _stage.ServerConnection.Send(new CR_PLAYER_MOVE_SYNC_NFY()
                {
                    moveX = 0,
                    moveY = force.y,
                    currentWorldPos = new VEC2()
                    {
                        x = obj.WorldPos.x,
                        y = obj.WorldPos.y,
                    },
                });
            }
            else
            {
                _stage.ServerConnection.Send(new CR_PLAYER_MOVE_SYNC_NFY()
                {
                    moveX = 0,
                    moveY = force.y,
                    currentWorldPos = new VEC2()
                    {
                        x = obj.WorldPos.x,
                        y = obj.WorldPos.y,
                    },
                });
                _stage.ServerConnection.Send(new CR_PLAYER_MOVE_SYNC_NFY()
                {
                    moveX = force.x,
                    moveY = 0,
                    currentWorldPos = new VEC2()
                    {
                        x = obj.WorldPos.x,
                        y = obj.WorldPos.y,
                    },
                });
            }
        }
        else
        {
            _stage.ServerConnection.Send(new CR_PLAYER_MOVE_SYNC_NFY()
            {
                moveX = force.x,
                moveY = force.y,
                currentWorldPos = new VEC2()
                {
                    x = obj.WorldPos.x,
                    y = obj.WorldPos.y,
                },
            });
        }
    }
}
