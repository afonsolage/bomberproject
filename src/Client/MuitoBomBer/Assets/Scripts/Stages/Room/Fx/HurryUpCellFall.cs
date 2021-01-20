using CommonLib.GridEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public delegate void OnDone();

//[RequireComponent(typeof(SpriteRenderer))]
public class HurryUpCellFall : MonoBehaviour
{
    public float height = 15f;
    public float time = 0.3f;
    public CellType fallType = CellType.Anvil;
    //public Sprite sprite;
    public GameObject block;
    public OnDone Done;

    private float speed = 0;
    private float target = 0;

    // Use this for initialization
    void Start()
    {
        var pos = transform.position;

        target = pos.y;
        transform.position = new Vector3(pos.x, pos.y + height, pos.z);
        speed = height / time;

        //var renderer = GetComponent<SpriteRenderer>();
        //renderer.sprite = sprite;
    }

    public void InitBlock()
    {
        GameObject.Instantiate(block, transform);
    }

    // Update is called once per frame
    void Update()
    {
        var curPos = transform.position;

        curPos.y -= speed * Time.deltaTime;
        //curPos.z = curPos.y;

        if (curPos.y <= target)
        {
            if (Done != null)
                Done();

            Destroy(gameObject);
        }
        else
        {
            transform.position = curPos;
        }
    }
}
