using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    protected Vector3 _originalPosition = Vector3.zero;

    private void Start()
    {
        _originalPosition = transform.localPosition;
    }

    internal IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originPosition = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            bool xIncrease = (Random.Range(0, 2) == 0) ? true : false;
            bool yIncrease = (Random.Range(0, 2) == 0) ? true : false;

            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            x = (xIncrease) ? originPosition.x + x : originPosition.x - x;
            y = (yIncrease) ? originPosition.y + y : originPosition.y - y;

            transform.localPosition = new Vector3(x, y, originPosition.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = _originalPosition;
    }

    internal void BeginShake(float duration, float magnitude)
    {
        StartCoroutine(Shake(duration, magnitude));
    }

    /// Debug Purpose.
    //#if UNITY_EDITOR
    //    public void OnGUI()
    //    {
    //        if (GUILayout.Button("test"))
    //        {
    //            StartCoroutine(Shake(.10f, .2f));
    //        }
    //    }
    //#endif
}
