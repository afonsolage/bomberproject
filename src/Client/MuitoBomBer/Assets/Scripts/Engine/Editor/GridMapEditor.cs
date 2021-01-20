using CommonLib.GridEngine;
using CommonLib.Util.Math;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridEngineInstance))]
public class GridMapEditor : Editor
{
    private static readonly int MAP_UPDATE_INTERVAL = 500;

    private static readonly Color NONE_COLOR = new Color(0, 0, 0, 0.1f);
    private static readonly Color WALL_COLOR = new Color(1, 0, 0, 0.1f);
    private static readonly Color BREAKABLE_COLOR = new Color(0, 0.5f, 0, 0.1f);
    private static readonly Color COLLECTABLE_COLOR = new Color(0, 0, 0.5f, 0.1f);
    private static readonly Color INVALID_COLOR = new Color(0, 0, 1, 0.1f);

    //private static readonly Color PF_ORIGIN_COLOR = new Color(0, 1, 0, 0.1f);
    //private static readonly Color PF_DEST_COLOR = new Color(1, 0, 0.3f, 0.1f);

    SerializedProperty size;
    SerializedProperty mapData;

    public CellType cellType;

    private GridEngineInstance instance;
    private GridCell[] data;

    private long nextMapUpdate;

    //TODO: Find a better way to setup types.
    private List<CellConfig> configList = new List<CellConfig>
    {
        new CellConfig(0, 0),
        new CellConfig(1, 2),
        new CellConfig(2, 1),
        new CellConfig(3, 3)
    };

    void OnEnable()
    {

        size = serializedObject.FindProperty("mapSize");
        mapData = serializedObject.FindProperty("mapDataPath");

        instance = target as GridEngineInstance;

        instance.MapManager.Init(configList);

        if (Application.isPlaying)
        {
            nextMapUpdate = DateTime.Now.Ticks;
        }
        else
        {
            LoadData();
        }

        instance.LoadMap(SerializeData(), configList, 0);
    }

    private bool editing = false;
    private bool b4Editing = false;
    private int playerSpawnCount = 0;
    private Vec2[] playerSpawnPlaces;
    private int spawnPlaceSelecting = -1;

    //Path Finding Test
    //private Vec2 pfOrigin = new Vec2(1, 1);
    //private Vec2 pfDest = new Vec2(1, 10);
    //private List<Vec2> pathList;

    //Closest Position Test
    //private Vec2 cpOrigin = new Vec2(1, 1);
    //private Vec2 cpDest = new Vec2(0, 0);

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (!editing)
            EditorGUI.BeginDisabledGroup(true);

        EditorGUILayout.PropertyField(size);

        if (editing)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Apply", GUILayout.Width(50), GUILayout.Height(15)))
            {
                NewData();
            }

            GUILayout.EndHorizontal();
        }

        EditorGUILayout.PropertyField(mapData);

        if (!editing)
            EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();

        if (!Application.isPlaying)
        {
            var text = (editing) ? "Disable Edit Mode" : "Enable Edit Mode";

            editing = GUILayout.Toggle(editing, text, "Button");

            if (editing != b4Editing)
            {
                SceneView.RepaintAll();
                b4Editing = editing;
            }

            if (editing)
            {
                EditorGUILayout.Space();

                var num = EditorGUILayout.TextField("Spawn Slots", playerSpawnCount.ToString());

                if (num != null && num.Length > 0)
                {
                    int qtd = 0;
                    var success = int.TryParse(num, out qtd);

                    if (success)
                    {
                        if (playerSpawnCount != qtd)
                        {
                            var oldPlaces = playerSpawnPlaces;
                            playerSpawnPlaces = new Vec2[qtd];

                            if (oldPlaces != null)
                            {
                                var min = Mathf.Min(playerSpawnCount, qtd);
                                for (int i = 0; i < min; i++)
                                    playerSpawnPlaces[i] = oldPlaces[i];
                            }

                            playerSpawnCount = qtd;
                        }
                    }
                }

                for (int i = 0; i < playerSpawnCount; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(string.Format("Spawn Slot {0} {1}", i, playerSpawnPlaces[i]));

                    var isSelected = spawnPlaceSelecting == i;
                    var sel = GUILayout.Toggle(isSelected, "Select", "Button");

                    if (sel)
                        spawnPlaceSelecting = i;
                    else if (isSelected && !sel)
                        spawnPlaceSelecting = -1;

                    EditorGUILayout.EndHorizontal();

                }
                EditorGUILayout.EndFadeGroup();

                EditorGUILayout.Space();

                cellType = (CellType)EditorGUILayout.EnumPopup("Cell Type: ", cellType);

                EditorGUILayout.Space();

                if (GUILayout.Button("Clear"))
                {
                    ClearData();
                }

                if (GUILayout.Button("Save"))
                {
                    SaveData();
                }

                if (GUILayout.Button("Reload"))
                {
                    LoadData();
                    SceneView.RepaintAll();
                }

                if (GUILayout.Button("Generate Script"))
                {
                    GenerateScript();
                }


                //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                ////Path Finding test
                //EditorGUILayout.LabelField("Path Finding");
                //EditorGUILayout.Space();

                //EditorGUILayout.BeginHorizontal();
                //EditorGUILayout.LabelField("Origin: ");
                //ushort.TryParse(EditorGUILayout.TextField(pfOrigin.x.ToString()), out pfOrigin.x);
                //ushort.TryParse(EditorGUILayout.TextField(pfOrigin.y.ToString()), out pfOrigin.y);
                //EditorGUILayout.EndHorizontal();

                //EditorGUILayout.BeginHorizontal();
                //EditorGUILayout.LabelField("Destination: ");
                //ushort.TryParse(EditorGUILayout.TextField(pfDest.x.ToString()), out pfDest.x);
                //ushort.TryParse(EditorGUILayout.TextField(pfDest.y.ToString()), out pfDest.y);
                //EditorGUILayout.EndHorizontal();

                //if (GUILayout.Button("Find"))
                //{
                //    PathFindTest();
                //}

                ////Closest Position test
                //EditorGUILayout.LabelField("Find Closest Position");
                //EditorGUILayout.Space();

                //EditorGUILayout.BeginHorizontal();
                //EditorGUILayout.LabelField("Origin: ");
                //ushort.TryParse(EditorGUILayout.TextField(cpOrigin.x.ToString()), out cpOrigin.x);
                //ushort.TryParse(EditorGUILayout.TextField(cpOrigin.y.ToString()), out cpOrigin.y);
                //EditorGUILayout.EndHorizontal();

                //if (GUILayout.Button("Closest"))
                //{
                //    ClosestTest();
                //}
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    //private void ClosestTest()
    //{
    //    if (cpOrigin == Vec2.ZERO)
    //    {
    //        return;
    //    }

    //    Debug.Log("Finding Closest Position of : " + cpOrigin);

    //    instance.LoadMap(SerializeData(), configList, 0);

    //    var map = instance.Map;
    //    cpDest = map.FindClosestPos(
    //        cpOrigin,
    //        c => c.HasAttribute(CellAttributes.BREAKABLE),
    //        c => !c.HasAttribute(CellAttributes.WALL) && c.FindFirstByType(GridObject.WALL_TYPES) == null);

    //    if (cpDest == Vec2.INVALID)
    //    {
    //        Debug.LogWarningFormat("Failed to find closest position of {0}", cpOrigin);
    //    }
    //    else
    //    {
    //        Repaint();
    //    }
    //}

    //private void PathFindTest()
    //{
    //    if (pfOrigin == pfDest)
    //    {
    //        pathList = null;
    //        return;
    //    }

    //    Debug.Log("Finding path: " + pfOrigin + " -> " + pfDest);

    //    instance.LoadMap(SerializeData(), configList, 0);

    //    var map = instance.Map;
    //    pathList = map.PathFind(pfOrigin, pfDest, c => !c.HasAttribute(CellAttributes.WALL) && c.FindFirstByType(GridObject.WALL_TYPES) == null);

    //    Repaint();
    //}

    private void GenerateScript()
    {
        string query = "INSERT INTO map (name, width, height, player_cnt, data) VALUES ('{0}', {1}, {2}, {3}, X'{4}');";

        string formattedData = string.Format("{0:X2}", playerSpawnCount);


        foreach (var v in playerSpawnPlaces)
        {
            formattedData += string.Format("{0:X2}{1:X2}", (byte)v.x, (byte)v.y);
        }

        foreach (var cell in data)
        {
            formattedData += string.Format("{0:X2}", (int)cell.Type);
        }

        query = string.Format(query, "NoNameYet", (int)size.vector2Value.x, (int)size.vector2Value.y, playerSpawnCount, formattedData);

        OutputWindow.Show(query);
    }

    void NewData()
    {
        data = new GridCell[(int)(size.vector2Value.x * size.vector2Value.y)];

        for (int i = 0; i < data.Length; i++)
        {
            data[i] = new GridCell(CellType.None, FromIndex(i));
        }

        SaveData();
    }

    void LoadData()
    {
        var parent = PrefabUtility.GetPrefabParent(target);

        if (parent == null)
            return;

        var path = AssetDatabase.GetAssetPath(parent);

        if (string.IsNullOrEmpty(path))
        {
            Debug.Log("Prefab not found for object: " + target);
            return;
        }

        path = path.Substring(0, path.LastIndexOf(".")) + ".map";

        if (File.Exists(path))
        {
            var rawData = File.ReadAllBytes(path);

            int i = 0;
            playerSpawnCount = rawData[i++];
            playerSpawnPlaces = new Vec2[playerSpawnCount];

            for (int c = 0; c < playerSpawnCount; c++)
            {
                playerSpawnPlaces[c] = new Vec2(rawData[i++], rawData[i++]);
            }

            data = new GridCell[rawData.Length - i];

            int k = 0;
            foreach (GridCell cell in data)
            {
                data[k] = GridCell.Deserialize(rawData[i], FromIndex(k));
                i++;
                k++;
            }

            if (data == null || data.Length == 0)
            {
                data = new GridCell[(int)instance.mapSize.x * (int)instance.mapSize.y];
            }
        }
    }

    private byte[] SerializeData()
    {
        var rawData = new byte[data.Length + playerSpawnCount * 2 + 1];

        int i = 0;
        rawData[i++] = (byte)playerSpawnCount;

        for (int k = 0; k < playerSpawnCount; k++)
        {
            rawData[i++] = (byte)playerSpawnPlaces[k].x;
            rawData[i++] = (byte)playerSpawnPlaces[k].y;
        }

        foreach (GridCell cell in data)
        {
            if (cell != null)
                rawData[i++] = cell.Serialize();
        }

        return rawData;
    }

    void SaveData()
    {
        var parent = PrefabUtility.GetPrefabParent(target);

        if (parent == null)
        {
            Debug.LogWarning("Could not save data for grid map " + target + ". Prefab not found. Consider creating a prefab before saving.");
            return;
        }

        var path = AssetDatabase.GetAssetPath(parent);

        if (string.IsNullOrEmpty(path))
        {
            Debug.Log("Prefab not found for object: " + target);
            return;
        }

        path = path.Substring(0, path.LastIndexOf(".")) + ".map";

        var rawData = SerializeData();

        File.WriteAllBytes(path, rawData);

        mapData.stringValue = path;
    }

    void ClearData()
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = new GridCell(CellType.None, FromIndex(i));
        }
    }

    int ToIndex(int x, int y)
    {
        return (int)(x * instance.mapSize.y + y);
    }

    Vec2 FromIndex(int index)
    {
        return new Vec2(index / (int)instance.mapSize.y, index % (int)instance.mapSize.y);
    }

    void DrawGizmo(int x, int y)
    {
        var centerOffset = instance.mapSize / 2;

        var offset = centerOffset * instance.cellSize;
        Vector3 center = instance.transform.position - new Vector3(offset.x, offset.y);

        center.x += x * instance.cellSize;
        center.y += y * instance.cellSize;

        var cellValue = data[ToIndex(x, y)];
        var fillColor = ComputeCellColor(cellValue);

        Handles.DrawSolidRectangleWithOutline(new Rect(new Vector2(center.x, center.y), new Vector2(instance.cellSize, instance.cellSize)), fillColor, Color.yellow);

        var idx = Array.IndexOf(playerSpawnPlaces, new Vec2(x, y));

        if (idx >= 0)
        {
            GUIStyle style = new GUIStyle
            {
                fontSize = 12
            };

            style.normal.textColor = Color.yellow;
            style.fixedHeight = 10;
            style.fixedWidth = 30;
            style.fontStyle = FontStyle.BoldAndItalic;

            Handles.Label(new Vector3(center.x + 0.25f, center.y + 0.9f), string.Format("P{0}", idx), style);
        }
    }

    Color ComputeCellColor(GridCell cell)
    {
        var res = NONE_COLOR;

        //if (cell.Pos == pfOrigin || cell.Pos == cpOrigin)
        //    return PF_ORIGIN_COLOR;
        //else if (cell.Pos == pfDest || cell.Pos == cpDest)
        //    return PF_DEST_COLOR;


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

    void CellClick(int x, int y)
    {
        data[ToIndex(x, y)] = new GridCell(cellType, new Vec2(x, y));
    }

    private Vec2 lastPos;

    void HandleMouseClick(Vector2 pos, bool isDrag = false)
    {
        if (!isDrag)
            lastPos = Vec2.ZERO;

        var worldPos = HandleUtility.GUIPointToWorldRay(pos).origin;
        Vec2 gridPos;
        if (Application.isPlaying)
        {
            gridPos = instance.WorldToGrid(worldPos);
        }
        else
        {
            gridPos = instance.WorldToGrid(worldPos, instance.mapSize / 2);
        }

        if (!gridPos.IsOnBounds((int)instance.mapSize.x, (int)instance.mapSize.y))
        {
            return;
        }

        if (spawnPlaceSelecting >= 0 && spawnPlaceSelecting < (playerSpawnPlaces?.Length ?? 0))
        {
            playerSpawnPlaces[spawnPlaceSelecting] = gridPos;
        }

        if (isDrag)
        {
            if (lastPos == gridPos)
                return;
            else
                lastPos = gridPos;
        }

        CellClick(gridPos.x, gridPos.y);
    }

    void OnSceneGUI()
    {
        if (data != null && data.Length > 0)
        {
            var sz = size.vector2Value;

            var sizeX = (int)sz.x;
            var sizeY = (int)sz.y;

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    DrawGizmo(x, y);
                }
            }
        }

        if (!Application.isPlaying)
        {
            if (!editing)
                return;

            //if (pathList != null)
            //{
            //    DrawPath();
            //}

            Event e = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);

            switch (e.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    if (e.button == 0 && !e.control && !e.shift && !e.alt)
                    {
                        GUIUtility.hotControl = controlID;
                        HandleMouseClick(e.mousePosition);
                        e.Use();
                    }
                    break;
                case EventType.MouseUp:

                    break;
                case EventType.MouseDrag:
                    if (e.button == 0 && !e.control && !e.shift && !e.alt)
                    {
                        GUIUtility.hotControl = controlID;
                        HandleMouseClick(e.mousePosition, true);
                        e.Use();
                    }
                    break;
                case EventType.KeyDown:
                    if (e.shift)
                    {
                        if (e.keyCode == KeyCode.Alpha1)
                        {
                            cellType = CellType.None;
                        }
                        else if (e.keyCode == KeyCode.Alpha2)
                        {
                            cellType = CellType.Invisible;
                        }
                        else if (e.keyCode == KeyCode.Alpha3)
                        {
                            cellType = CellType.Rock;
                        }
                        else if (e.keyCode == KeyCode.Alpha4)
                        {
                            cellType = CellType.Plant;
                        }
                        else if (e.keyCode == KeyCode.Alpha5)
                        {
                            cellType = CellType.Wooden;
                        }
                        else
                        {
                            break;
                        }

                        Repaint();
                        e.Use();
                    }

                    break;
            }
        }
        else
        {
            if (nextMapUpdate * TimeSpan.TicksPerMillisecond < DateTime.Now.Ticks)
            {
                nextMapUpdate += MAP_UPDATE_INTERVAL * TimeSpan.TicksPerMillisecond;

                data = instance.Map.GetDump();
            }
        }
    }

    //private void DrawPath()
    //{
    //    if (pathList == null)
    //        return;

    //    for (var i = 0; i < pathList.Count - 1; i++)
    //    {
    //        var p = pathList[i]; //Current pos
    //        var n = pathList[i + 1]; //Next pos

    //        DrawPathLine(p, n);
    //    }
    //}

    private void DrawPathLine(Vec2 p, Vec2 n)
    {
        var centerOffset = instance.mapSize / 2;

        var offset = centerOffset * instance.cellSize;
        Vector3 center = instance.transform.position - new Vector3(offset.x, offset.y);

        var cellOffset = instance.cellSize / 2;


        Vector3 pCenter = new Vector3(
            center.x + (p.x * instance.cellSize) + cellOffset,
            center.y + (p.y * instance.cellSize) + cellOffset,
            0);

        Vector3 nCenter = new Vector3(
            center.x + (n.x * instance.cellSize) + cellOffset,
            center.y + (n.y * instance.cellSize) + cellOffset,
            0);


        Handles.DrawLine(pCenter, nCenter);
    }
}
