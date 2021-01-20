using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WaitingComponent : MonoBehaviour
{
    public UILabel _label;
    public UIButton _cancel;

    public void ShowCancel(bool show)
    {
        _cancel.gameObject.SetActive(show);
    }

    public void SetText(string text)
    {
        if (_label)
            _label.text = text;
    }

    public void SetOnCancel(EventDelegate.Callback callback)
    {
        _cancel.onClick.Clear();
        _cancel.onClick.Add(new EventDelegate(callback));
    }
}
