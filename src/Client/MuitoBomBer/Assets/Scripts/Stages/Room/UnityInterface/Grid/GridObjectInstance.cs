using CommonLib.GridEngine;
using CommonLib.Util.Math;
using System.Collections;
using UnityEngine;

public class GridObjectInstance : MonoBehaviour
{
    private GridEngineInstance _instance;
    private GridObject _obj;

    public GridObject GridObject
    {
        get
        {
            return _obj;
        }
    }

    public GridDir FacingDir
    {
        get
        {
            return _obj.Facing;
        }
    }

    private void Start()
    {
        FindEngineInstance();
    }

    public void Setup(GridObject obj)
    {
        _obj = obj;
    }

    private void FindEngineInstance()
    {
        var go = GameObject.Find("GridEngine");

        if (go != null)
        {
            _instance = go.GetComponent<GridEngineInstance>();
        }
    }

    private void Update()
    {
        if (_instance == null || _obj == null || !_obj.GridPos.IsValid())
        {
            return;
        }

        transform.position = GridToUnity();
    }

    public void Move(float x, float y)
    {
        _obj.Move(x, y);
    }

    private Vector3 GridToUnity()
    {
        //return new Vector3(_obj.WorldPos.x, _obj.WorldPos.y, _obj.WorldPos.y);
        return new Vector3(_obj.WorldPos.x, 0/*_obj.WorldPos.y*/, _obj.WorldPos.y);
    }

    private Vec2f UnityToGrid()
    {
        return new Vec2f(transform.position.x, transform.position.y);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && _obj != null && _obj.IsOnMap && _obj.GridPos.IsValid() && _instance.Map != null)
        {
            var surrounding = _obj.Surroundings;

            DrawSurroundingRange(surrounding.LeftRange());
            DrawSurroundingRange(surrounding.RightRange());
            DrawSurroundingRange(surrounding.BottomRange(false));
            DrawSurroundingRange(surrounding.UpRange(false));

            DrawGridCell(_obj.GridPos.x, _obj.GridPos.y, _instance.Map[_obj.GridPos.x, _obj.GridPos.y]);
        }
    }

    private void DrawGridCell(int x, int y, GridCell val)
    {
        var color = ComputeCellColor(val);
        var world = _instance.GridToWorld(new Vec2(x, y));

        world.x -= _instance.Map.CellSize / 2;
        world.y -= _instance.Map.CellSize / 2;

        UnityEditor.Handles.DrawSolidRectangleWithOutline(new Rect(world, new Vector2(_instance.cellSize, _instance.cellSize)), color, Color.yellow);
        //Gizmos.DrawCube(instance.GridToWorld(new Vec2(x, y)), new Vector3(instance.Map.CellSize, instance.Map.CellSize, 0.01f));
    }

    private void DrawSurroundingRange(Rang2 rang)
    {
        for (int x = rang.beg.x; x <= rang.end.x; x++)
        {
            for (int y = rang.beg.y; y <= rang.end.y; y++)
            {
                DrawGridCellSurrounding(x, y, _instance.Map[x, y]);
            }
        }
    }

    private void DrawGridCellSurrounding(int x, int y, GridCell val)
    {
        if (val == null || val.Type == CellType.Invalid)
            return;

        var color = ComputeCellColor(val);
        var world = _instance.GridToWorld(new Vec2(x, y));

        world.x -= _instance.Map.CellSize / 2;
        world.y -= _instance.Map.CellSize / 2;

        UnityEditor.Handles.DrawSolidRectangleWithOutline(new Rect(world, new Vector2(_instance.cellSize, _instance.cellSize)), color, Color.cyan);
        //Gizmos.DrawCube(instance.GridToWorld(new Vec2(x, y)), new Vector3(instance.Map.CellSize, instance.Map.CellSize, 0.01f));
    }

    private static readonly Color NONE_COLOR = new Color(0, 0, 0, 0.1f);
    private static readonly Color WALL_COLOR = new Color(1, 0, 0, 0.1f);
    private static readonly Color BREAKABLE_COLOR = new Color(0, 0.5f, 0, 0.1f);
    private static readonly Color COLLECTABLE_COLOR = new Color(0, 0, 0.5f, 0.1f);
    private static readonly Color INVALID_COLOR = new Color(0, 0, 1, 0.1f);

    Color ComputeCellColor(GridCell cell)
    {
        var res = NONE_COLOR;

        if (cell.HasAttribute(CellAttributes.INVALID))
        {
            return INVALID_COLOR;
        }

        if (cell.HasAttribute(CellAttributes.BREAKABLE))
        {
            res += BREAKABLE_COLOR;
        }
        if (cell.HasAttribute(CellAttributes.COLLECTABLE))
        {
            res += COLLECTABLE_COLOR;
        }
        if (cell.HasAttribute(CellAttributes.WALL))
        {
            res += WALL_COLOR;
        }

        return res;
    }
#endif
}

