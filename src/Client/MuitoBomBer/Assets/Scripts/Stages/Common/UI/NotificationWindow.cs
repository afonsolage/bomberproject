using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationWindow : MonoBehaviour
{
    public UILabel _label;
    private uint _currentValue = 0;

    internal void IncreaseNotification()
    {
        _currentValue++;

        if (_label)
        {
            if (_currentValue > 999)
            {
                _label.text = "999+";
            }
            else
            {
                _label.text = _currentValue.ToString();
            }
        }  
    }

    internal void ResetNotification()
    {
        _currentValue = 0;

        if (_label)
            _label.text = _currentValue.ToString();
    }
}
