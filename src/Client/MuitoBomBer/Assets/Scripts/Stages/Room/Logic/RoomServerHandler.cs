using System;
using CommonLib.Messaging.Client;
using UnityEngine;
using CommonLib.Messaging;
using CommonLib.Util;
using CommonLib.Util.Math;
using System.Collections.Generic;
using Assets.Scripts.Engine.Logic.Object;

public abstract class RoomServerHandler
{
    internal static void TokenRes(CX_TOKEN_RES res, RoomServerConnection client)
    {
        Debug.Assert(res.error == MessageError.NONE,
            "We can only receive a TokenRes from RoomServer is everything is right, else, we should be disconnected");

        //Now let's ask to join on desired room
        client.Send(new CR_JOIN_ROOM_REQ()
        {
            uid = client.Stage.RoomIndex,
        });
    }

    internal static void JoinRoomNfy(CR_JOIN_ROOM_NFY nfy, RoomServerConnection client)
    {
        var stage = client.Stage;
        var engine = stage.GridEngine;

        engine.mapSize = new Vector2(nfy.info.width, nfy.info.height);
        engine.LoadMap(nfy.info.data, nfy.typeList, nfy.info.background);

        stage.MapLoaded();
    }

    internal static void JoinRoomRes(CR_JOIN_ROOM_RES res, RoomServerConnection client)
    {
        if (res.error != MessageError.NONE)
        {
            CLog.E("Failed to create room! Error: {0}", res.error);
        }
        else
        {
            client.Stage.ObjectManager.MainPlayerUID = res.mainUID;

            // Initialize HUD.
            client.Stage.UIManager.FindInstance(WindowType.HUD_ROOT, true, true);
        }
    }

    internal static void WelcomeNfy(CR_WELCOME_NFY nfy, RoomServerConnection client)
    {
        CLog.I("Received welcome message from server: " + nfy.serverName);

        client.Stage.SessionID = nfy.uid;

        //var roomToJoin = client.Stage._roomToJoin;

        //if (roomToJoin == 0)
        //{
        //    client.Send(new CR_CREATE_ROOM_REQ());
        //}
        //else
        //{
        //    client.Send(new CR_JOIN_ROOM_REQ()
        //    {
        //        uid = roomToJoin,
        //    });
        //}
    }

    internal static void PlayerEnterNfy(CR_PLAYER_ENTER_NFY nfy, RoomServerConnection client)
    {
        var attributes = new PlayerAttributes(nfy.info.attr);

        client.Stage.ObjectManager.InstanciatePlayer(nfy.info.uid, 
            new Vec2((int)nfy.info.gridPos.x, (int)nfy.info.gridPos.y), 
            nfy.info.alive, 
            attributes,
            nfy.info.nick,
            nfy.info.gender);
    }

    internal static void PlayerUpdateAttributesRes(CR_PLAYER_UPDATE_ATTRIBUTES_RES res, RoomServerConnection client)
    {
        client.Stage.ObjectManager.PlayerUpdateAttributes(res.uid, new PlayerAttributes(res.attributes));
    }

    internal static void PlayerDiedNfy(CR_PLAYER_DIED_NFY nfy, RoomServerConnection client)
    {
        client.Stage.ObjectManager.PlayerDied(nfy.uid, nfy.killer);
    }

    internal static void PlayerHitNfy(CR_PLAYER_HIT_NFY nfy, RoomServerConnection client)
    {
        client.Stage.ObjectManager.PlayerHit(nfy.uid, nfy.hitter);
    }

    internal static void ImmunityNfy(CR_IMMUNITY_NFY nfy, RoomServerConnection client)
    {
        client.Stage.ObjectManager.SetImmune(nfy.uid, nfy.duration);
    }

    internal static void SpeedChangeNfy(CR_SPEED_CHANGE_NFY nfy, RoomServerConnection client)
    {
        client.Stage.ObjectManager.SetSpeed(nfy.uid, nfy.speed);
    }

    internal static void PlayerLeaveNfy(CR_PLAYER_LEAVE_NFY nfy, RoomServerConnection client)
    {
        var mainPlayer = client.Stage.ObjectManager.MainPlayer;

        if (mainPlayer != null && mainPlayer.UID == nfy.uid)
        {
            //TODO: Add to move to other scene when leave room.
            Application.Quit();
        }
        else
        {
            client.Stage.ObjectManager.DestroyPlayer(nfy.uid);
        }
    }

    internal static void HurryUpCellNfy(CR_HURRY_UP_CELL_NFY nfy, RoomServerConnection client)
    {
        client.Stage.ObjectManager.HurryUpCell(new Vec2((ushort)nfy.cell.x, (ushort)nfy.cell.y), nfy.replaceType);
    }

    internal static void BombPosNfy(CR_BOMB_POS_NFY nfy, RoomServerConnection client)
    {
        client.Stage.ObjectManager.SyncBombPos(nfy.uid, new Vec2f(nfy.worldPos.x, nfy.worldPos.y));
    }

    internal static void MatchEnd(CR_MATCH_END_NFY nfy, RoomServerConnection client)
    {
        client.Stage.MatchEnd(nfy.winner);
    }

    internal static void PowerUpAddNfy(CR_POWERUP_ADD_NFY nfy, RoomServerConnection client)
    {
        client.Stage.ObjectManager.PowerUpAdd(nfy.uid, new Vec2((int)nfy.cell.x, (int)nfy.cell.y), nfy.icon);
    }

    internal static void PowerUpRemoveNfy(CR_POWERUP_REMOVE_NFY nfy, RoomServerConnection client)
    {
        client.Stage.ObjectManager.PowerUpRemove(nfy.uid, nfy.collected);
    }

    internal static void PlayerPosNfy(CR_PLAYER_POS_NFY nfy, RoomServerConnection client)
    {
        client.Stage.ObjectManager.SyncPlayerPos(nfy.uid, new Vec2f(nfy.worldPos.x, nfy.worldPos.y));
    }

    internal static void PlaceBombRes(CR_PLACE_BOMB_RES res, RoomServerConnection client)
    {
        if (res.error != MessageError.NONE)
        {
            CLog.E("Failed to place bomb: " + res.error);
        }
    }

    internal static void BombPlacedNfy(CR_BOMB_PLACED_NFY nfy, RoomServerConnection client)
    {
        client.Stage.ObjectManager.PlaceBomb(nfy.uid, nfy.gridX, nfy.gridY, nfy.moveSpeed);
    }

    internal static void BombExplodedNfy(CR_BOMB_EXPLODED_NFY nfy, RoomServerConnection client)
    {
        var areaCount = nfy.area?.Count ?? 0;
        var area = new List<Vec2>(areaCount);

        if (areaCount > 0)
        {
            foreach (var pos in nfy.area)
            {
                area.Add(new Vec2((int)pos.x, (int)pos.y));
            }
        }

        client.Stage.ObjectManager.ExplodeBomb(nfy.uid, area);
    }

    internal static void BombExplodedObjectNfy(CR_BOMB_EXPLODED_OBJECT_NFY nfy, RoomServerConnection client)
    {
        var areaCount = nfy.area?.Count ?? 0;

        var area = new List<Vec2>(areaCount);

        if (areaCount > 0)
        {
            foreach (var pos in nfy.area)
            {
                area.Add(new Vec2((int)pos.x, (int)pos.y));
            }
        }

        client.Stage.ObjectManager.ExplodeObjectBomb(area);
    }
}
