using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GOUtil
{
    public static GameObject FindChild(GameObject parent, string childName)
    {
        Transform[] trs = parent.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in trs)
        {
            if (t.name == childName)
            {
                return t.gameObject;
            }
        }

        return null;
    }
}
