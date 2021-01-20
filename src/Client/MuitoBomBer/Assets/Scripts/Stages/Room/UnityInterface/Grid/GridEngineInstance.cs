using CommonLib.GridEngine;
using CommonLib.Util.Math;
using CommonLib.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

[ExecuteInEditMode]
public class GridEngineInstance : MonoBehaviour
{
    private MapManager _mapManager;
    public MapManager MapManager
    {
        get
        {
            if (_mapManager == null)
                _mapManager = new MapManager();

            return _mapManager;
        }
    }

    private Map _map;
    public Map Map
    {
        get
        {
            return _map;
        }
    }

    public readonly float cellSize = 1.0f;
    public Vector2 mapSize = new Vector2(60, 30);

    public string mapDataPath;

    private Vector2 _centerOffset;
    public Vector2 CenterOffset
    {
        get
        {
            return _centerOffset;
        }
    }

    private void Awake()
    {
        _mapManager = new MapManager();
    }

    private void Start()
    {
    }

    private void OnDestroy()
    {
        var components = GetComponentsInChildren<Transform>();
        foreach (var t in components)
        {
            Destroy(t.gameObject);
        }

        _mapManager.Destroy(_map);
        _map = null;
        _mapManager = new MapManager();
        _centerOffset = Vector2.zero;

        Destroy(GetComponent<GridMapRenderer>());
    }

    private void FixedUpdate()
    {
        if (_map == null)
            return;

        _map.Tick(Time.fixedDeltaTime);
    }

    public void LoadMap(byte[] data, List<Tuple<int, int>> typeList, uint background)
    {
        var configList = new List<CellConfig>(typeList.Count);

        foreach (Tuple<int, int> t in typeList)
        {
            configList.Add(new CellConfig(t.Item1, t.Item2));
        }

        LoadMap(data, configList, background);
    }

    public void LoadMap(byte[] data, List<CellConfig> configList, uint background)
    {
        _mapManager.Init(configList);

        if (_map != null)
        {
            _mapManager.Destroy(_map);
            _map = null;
        }

        _map = _mapManager.Create(cellSize, (ushort)mapSize.x, (ushort)mapSize.y, data);

        _map.Background = background;

        _centerOffset = new Vector2(mapSize.x / 2, mapSize.y / 2);
    }

    public Vec2 WorldToGrid(Vector3 pos)
    {
        var localPos = pos - transform.position;

        return _map.WorldToGrid(new Vec2f(localPos.x, localPos.y));
    }

    public Vec2 WorldToGrid(Vector3 pos, Vector2 centerOffset)
    {
        var offset = centerOffset * cellSize;
        var localPos = pos - transform.position + new Vector3(offset.x, offset.y);

        pos.x = (int)(localPos.x / cellSize);
        pos.y = (int)(localPos.y / cellSize);

        return new Vec2((int)pos.x, (int)pos.y);
    }

    public Vector3 GridToWorld(Vec2 pos)
    {
        Vector3 res = new Vector3(pos.x - CenterOffset.x, pos.y - CenterOffset.y, 0);

        res *= cellSize;
        res.x += (cellSize / 2);
        res.y += (cellSize / 2);

        return res;
    }
}

