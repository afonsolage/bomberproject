using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharInfoComponent : MonoBehaviour
{
    public UILabel _name;
    public UILabel _level;

    public void SetName(string name)
    {
        if(_name)
        {
            _name.text = name;
        }
    }

    public void SetLevel(uint level)
    {
        if(_level)
        {
            _level.text = "Lv. " + level;
        }
    }
}
