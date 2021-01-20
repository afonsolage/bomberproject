using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputArrowManager : MonoBehaviour
{
    private bool _isChecked = true;
    //private bool _isDrag = false;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnPress(bool isPressed)
    {
        Debug.Log("OnPress");

        if (_isChecked)
        {
            Vector3 lastWorldPosition = UICamera.lastWorldPosition;

            if (isPressed)
            {
                PointerDown(lastWorldPosition);
            }
            else
            {
                PointerUp(lastWorldPosition);
            }
        }
    }

    private void PointerDown(Vector3 pos)
    {
        Debug.Log("PointerDown");
    }

    private void OnDrag(Vector2 delta)
    {
        Debug.Log("OnDrag");

        if (_isChecked)
        {
        }
    }

    private void PointerUp(Vector3 eventData)
    {
        Debug.Log("PointerUp");
    }
}
