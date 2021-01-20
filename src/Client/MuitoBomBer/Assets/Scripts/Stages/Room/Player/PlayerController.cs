using CommonLib.GridEngine;
using CommonLib.Messaging.Client;
using CommonLib.Util;
using Engine.Logic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GridLocalPosSync))]
public class PlayerController : BasePlayerController
{
    public bool reverseDirection;
    private GridLocalPosSync _posSync;

    public GridLocalPosSync PosSync { get { return _posSync; } }

    protected override void Start()
    {
        base.Start();

        _posSync = GetComponent<GridLocalPosSync>();
    }

    void Update()
    {
        ProcessInput();
    }

    private void ProcessInput()
    {
        // Get axis.
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Check if horizonal or vertical is be clicked.
        bool isWalking = h != 0f || v != 0f;

        // Let to move.
        if (isWalking)
        {
            if (h != 0f)
            {
                Move((h > 0f) ? Global.MoveDirection.RIGHT : Global.MoveDirection.LEFT);
            }
            else if (v != 0f)
            {
                Move((v > 0f) ? Global.MoveDirection.UP : Global.MoveDirection.DOWN);
            }
        }

        // Place bomb
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlaceBomb();
        }

        if (isMoving && !isWalking /*&& Animator.playing == true*/)
        {
            PlayAnimation(PlayerAnimation.IDLE);
        }
    }

    private bool PlaceBomb()
    {
        // Get current position on grid.
        var gridPos = _gridObj.GridObject.GridPos;

        // Info about current map.
        var map = _stage.GridEngine.Map;

        // Check if already has some bomb in current position from grid.
        var obj = map[gridPos.x, gridPos.y].FindFirstByType(ObjectType.BOMB);
        if (obj != null)
        {
            // Already has a bomb here, you can't put another bomb. Bye.
            return false;
        }

        _posSync.SyncNow();

        // Request for server to place bomb.
        _stage.ServerConnection.Send(new CR_PLACE_BOMB_REQ());

        return true;
    }

    public override bool Move(Global.MoveDirection direction)
    {
        return base.Move(direction);
    }
}
