using CommonLib.GridEngine;
using CommonLib.Util.Math;
using CommonLib.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
using CommonLib.Messaging.Common;
using Assets.Scripts.Engine.Logic.Object;
using CommonLib.Messaging;

public class ObjectManager : MonoBehaviour
{
    private RoomStage _stage;
    public RoomStage Stage
    {
        get
        {
            return _stage;
        }
    }

    private uint _mainPlayerUID;
    public uint MainPlayerUID
    {
        get
        {
            return _mainPlayerUID;
        }
        set
        {
            _mainPlayerUID = value;
        }
    }

    private PlayerRoom _mainPlayer;
    public PlayerRoom MainPlayer
    {
        get
        {
            return _mainPlayer;
        }
    }

    private Map _map;
    public Map Map
    {
        get
        {
            return _map;
        }
        set
        {
            _map = value;
        }
    }

    private GridMapRenderer _mapRenderer;
    public GridMapRenderer MapRenderer
    {
        get
        {
            if (_mapRenderer == null) _mapRenderer = _stage.GridEngine.gameObject.GetComponent<GridMapRenderer>();
            return _mapRenderer;
        }
    }

    public void Init(RoomStage stage)
    {
        _stage = stage;
    }

    public void InstanciatePlayer(uint uid, Vec2 pos, bool alive, PlayerAttributes attributes, string nick, PlayerGender gender)
    {
        var isMainPlayer = uid == MainPlayerUID;

        var player = new PlayerRoom(uid, this, attributes);
        player.Instanciate(isMainPlayer, nick, gender);

        player.Wrap(_map.GridToWorld(pos));
        player.EnterMap();

        if (!alive)
            player.GameObject.SetActive(false);

        if (isMainPlayer)
            _mainPlayer = player;
    }

    internal void DestroyPlayer(uint uid)
    {
        var obj = _map.FindObject(uid);

        if (obj == null)
            return;

        var player = obj as PlayerRoom;
        if (player != null)
        {
            if (player.FlashingFx != null)
                StopCoroutine(player.FlashingFx);

            player.LeaveMap();
            player.Destroy();
        }
    }

    internal void PlaceBomb(uint uid, ushort gridX, ushort gridY, uint moveSpeed)
    {
        var bomb = new Bomb(this, uid, _stage.GridEngine.Map, new Vec2(gridX, gridY), moveSpeed);
        bomb.EnterMap();
    }

    internal void ExplodeBomb(uint uid, List<Vec2> explosionArea)
    {
        var map = _stage.GridEngine.Map;
        var obj = map.FindObject(uid);

        if (obj != null)
        {
            var bomb = obj as Bomb;
            bomb.Explode(explosionArea);
        }
        else
        {
            CLog.D("Trying to explode a bomb that doesn't exists: {0}", uid);
        }
    }

    internal void PlayerUpdateAttributes(uint uid, PlayerAttributes attributes)
    {
        var obj = _map.FindObject(uid);

        if (obj == null || !(obj is PlayerRoom))
            return;

        var player = obj as PlayerRoom;
        player.UpdateAttributes(attributes);
    }

    internal void PlayerDied(uint uid, uint killer)
    {
        var obj = _map.FindObject(uid);

        if (obj == null || !(obj is PlayerRoom))
            return;

        var player = obj as PlayerRoom;

        player.Died(killer);
    }

    internal void SetSpeed(uint uid, uint speed)
    {
        var obj = _map.FindObject(uid);

        if (obj == null || !(obj is PlayerRoom))
            return;

        var player = obj as PlayerRoom;
        player.Attr.moveSpeed = speed;
        player.UpdateAttributes();
    }

    internal void PlayerHit(uint uid, uint hitter)
    {
        var obj = _map.FindObject(uid);

        if (obj == null || !(obj is PlayerRoom))
            return;

        var player = obj as PlayerRoom;

        player.OnHit(hitter);
    }

    internal void SetImmune(uint uid, float duration)
    {
        var obj = _map.FindObject(uid);

        if (obj == null || !(obj is PlayerRoom))
        {
            CLog.W("Trying to set immune an invalid object!");
            return;
        }

        var player = obj as PlayerRoom;
        player.FlashingFx = SpriteFx.FlashSprites(player.GameObject, (int)(duration * 10), 0.1f, true, false);
        StartCoroutine(player.FlashingFx);
    }

    internal void HurryUpCell(Vec2 cell, CellType replace)
    {
        MapRenderer.HurryUpCell(cell, replace);
    }

    internal void SyncPlayerPos(uint uid, Vec2f worldPos)
    {
        var obj = _map.FindObject(uid);

        if (obj == null)
            return;

        var player = obj as PlayerRoom;
        if (player == null)
        {
            return;
        }

        //If we received some sync message from server and we are the main player, this means our position is invalid.
        if (player.IsMainPlayer)
        {
            //We need to force fix it.
            player.ForceMoveTo(worldPos);
        }
        else
        {
            player.AddMoveDest(worldPos);
        }
    }

    internal void PowerUpAdd(uint uid, Vec2 pos, uint icon)
    {
        var powerup = new PowerUp(uid, icon, pos, _map);
        powerup.GameObject.transform.parent = gameObject.transform;
    }

    internal void PowerUpRemove(uint uid, bool collected)
    {
        var obj = _map.FindObject(uid);

        if (obj == null || !(obj is PowerUp))
            return;

        var powerUp = obj as PowerUp;
        powerUp.Destroy(collected);
    }

    internal void ExplodeObjectBomb(List<Vec2> explosionArea)
    {
        var map = _stage.GridEngine.Map;

        foreach (var pos in explosionArea)
        {
            map[pos.x, pos.y].UpdateCell(CommonLib.GridEngine.CellType.None);

            //MapRenderer.DestroySprite(pos.x, pos.y);
            MapRenderer.DestroyObject(pos.x, pos.y);
        }
    }

    internal void SyncBombPos(uint uid, Vec2f worldPos)
    {
        var obj = _map.FindObject(uid);

        if (obj == null)
            return;

        var bomb = obj as Bomb;
        if (bomb != null)
        {
            bomb.AddMoveDest(worldPos);
        }
    }

    internal void OnDestroy()
    {
        Reset();
        _stage = null;
    }

    internal void Reset()
    {
        if (gameObject != null)
        {
            foreach (var t in gameObject.GetComponentsInChildren<Transform>())
            {
                Destroy(t.gameObject);
            }
        }

        _map = null;
    }

    internal void Clear()
    {
        if (_stage == null)
            return;

        Reset();
    }
}
