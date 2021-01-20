using CommonLib.GridEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMapReference : MonoBehaviour
{
    [HideInInspector]
    public GridMap Map;

    private GridEngineInstance instance;

    // Use this for initialization
    void Start()
    {
        FindEngineInstance();

        if (instance != null)
            Map = instance.Map;
    }

    private void FindEngineInstance()
    {
        var go = GameObject.Find("GridEngine");

        if (go != null)
        {
            instance = go.GetComponent<GridEngineInstance>();
        }
    }
}
