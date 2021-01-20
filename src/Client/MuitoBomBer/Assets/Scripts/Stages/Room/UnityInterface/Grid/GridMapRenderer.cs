using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CommonLib.GridEngine;
using DG.Tweening;
using CommonLib.Util.Math;

[RequireComponent(typeof(GridEngineInstance))]
public class GridMapRenderer : MonoBehaviour
{
    public Sprite[] typeSprites;
    public GameObject[] typeBlocksObjects;

    private GridEngineInstance _instance;
    private GridMap _map;

    private SpriteRenderer[] _mapSprites;
    private GameObject[] _mapObjects;

    // Use this for initialization
    void Start()
    {
        _instance = GetComponent<GridEngineInstance>();
        _map = _instance.Map;

        _mapSprites = new SpriteRenderer[_map.MapSize.x * _map.MapSize.y];

        _mapObjects = new GameObject[_map.MapSize.x * _map.MapSize.y];

        LoadMapObjects();
    }

    private void LoadMapObjects()
    {
        var mapSize = _map.MapSize;

        // Add background.

        // 2D Graphics.
        //var go = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Map/Background/" + _instance.Map.Background), new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        //go.transform.parent = transform;

        var go = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Map/Background/" + "Map"), new Vector3(8.5f, 0, -5.2f), Quaternion.identity) as GameObject;
        go.transform.parent = transform;

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                // 2D Graphics.
                //AddSprite(new Vec2(x, y));

                AddBlockObject(new Vec2(x, y));
            }
        }
    }

    public void AddBlockObject(Vec2 pos)
    {
        var idx = ToIndex(pos.x, pos.y);

        if (_mapObjects[idx] != null)
            Destroy(_mapObjects[idx].gameObject);

        var cell = _map[pos.x, pos.y];

        if (cell == null || cell.Type == CellType.Invalid || cell.Type == CellType.None)
            return;

        var blockObj = GetBlockObject(cell.Type);
        var go = GameObject.Instantiate(blockObj);
        go.name = cell.Type + " [" + pos.x + ", " + pos.y + "]";

        var worldPos = _map.GridToWorld(pos);
        go.transform.position = new Vector3(worldPos.x, 0/*worldPos.y*/, worldPos.y);
        go.transform.parent = transform;
        //go.transform.localScale = new Vector3(1.5f, 1.5f, 1);

        //_mapSprites[idx] = sprite;
        _mapObjects[idx] = go;
    }

    public void AddSprite(Vec2 pos)
    {
        var idx = ToIndex(pos.x, pos.y);

        if (_mapSprites[idx] != null)
            Destroy(_mapSprites[idx].gameObject);

        var cell = _map[pos.x, pos.y];

        if (cell == null || cell.Type == CellType.Invalid || cell.Type == CellType.None)
            return;

        GameObject go = new GameObject(cell.Type + " [" + pos.x + ", " + pos.y + "]");
        var sprite = go.AddComponent<SpriteRenderer>();

        sprite.sprite = GetSprite(cell.Type);

        var worldPos = _map.GridToWorld(pos);
        go.transform.position = new Vector3(worldPos.x, worldPos.y, worldPos.y);
        go.transform.parent = transform;
        go.transform.localScale = new Vector3(1.5f, 1.5f, 1);

        _mapSprites[idx] = sprite;
    }

    public int ToIndex(int x, int y)
    {
        return (x * _map.MapSize.y) + y;
    }

    protected Sprite GetSprite(CellType type)
    {
        return typeSprites[(int)type - 1];
    }

    protected GameObject GetBlockObject(CellType type)
    {
        return typeBlocksObjects[(int)type - 1];
    }

    public void DestroyObject(int x, int y)
    {
        var mapObject = _mapObjects[ToIndex(x, y)];
        if (mapObject != null)
        {
            //StartCoroutine(SpriteFx.FlashSprites(mapObject, 2, 0.1f));

            GameObject.Destroy(mapObject);

            _mapObjects[(x * _map.MapSize.y) + y] = null;
        }
    }

    public void DestroySprite(int x, int y)
    {
        var mapSprite = _mapSprites[ToIndex(x, y)];
        if (mapSprite != null)
        {
            StartCoroutine(SpriteFx.FlashSprites(mapSprite.gameObject, 2, 0.1f));
            _mapSprites[(x * _map.MapSize.y) + y] = null;
        }

        //Destroy(mapSprite.gameObject);
    }

    internal void Reset()
    {
        foreach (SpriteRenderer s in _mapSprites)
        {
            if (s != null)
            {
                Destroy(s.gameObject);
            }
        }

        foreach (GameObject obj in _mapObjects)
        {
            if (obj != null)
            {
                Destroy(obj.gameObject);
            }
        }
    }

    internal void HurryUpCell(Vec2 cell, CellType replace)
    {
        if (replace == CellType.Invalid)
        {
            Debug.LogWarningFormat("Trying to replace with an invalid cell type.");
            return;
        }

        if (replace == CellType.None)
        {
            _map[cell.x, cell.y]?.UpdateCell(replace);
        }
        else
        {
            if (!GridCell.Types.Contains(replace))
            {
                Debug.LogWarningFormat("Trying to replace with cell type that wasn't configured: {0}", replace);
                return;
            }

            var go = new GameObject("HurryUp-" + cell);
            var hurryUp = go.AddComponent<HurryUpCellFall>();
            hurryUp.fallType = replace;
            //hurryUp.sprite = GetSprite(replace);
            hurryUp.block = GetBlockObject(replace);
            hurryUp.InitBlock();

            hurryUp.Done = () =>
            {
                _map[cell.x, cell.y]?.UpdateCell(replace);
                //AddSprite(cell);
                AddBlockObject(cell);
            };

            var world = _map.GridToWorld(cell);
            go.transform.position = new Vector3(world.x, 0, world.y);
            go.transform.parent = transform;
        }
    }
}
