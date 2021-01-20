using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageBox : UIComponent
{
    public delegate void MsgBoxEvent(bool result);

    public enum MSG_BOX_STYLE
    {
        YES_NO,
        OK,
        UNKNOWN
    }

    public UIButton _confirmButton;
    public UIButton _cancelButton;
    public UIButton _okButton;

    public UILabel _titleLabel;
    public UILabel _messageLabel;

    private MsgBoxEvent _boxEvent;

    private void Start()
    {
        var pos = transform.localPosition;
        pos.y = 60;

        transform.localPosition = pos;
    }

    public void CreateMsgBox(string title, string text, MSG_BOX_STYLE style, MsgBoxEvent boxEvent)
    {
        _titleLabel.text = title;
        _messageLabel.text = text;
        _boxEvent = boxEvent;

        _okButton.gameObject.SetActive(false);
        _confirmButton.gameObject.SetActive(false);
        _cancelButton.gameObject.SetActive(false);

        if (style == MSG_BOX_STYLE.OK)
        {
            _okButton.gameObject.SetActive(true);
        }
        else
        {
            _confirmButton.gameObject.SetActive(true);
            _cancelButton.gameObject.SetActive(true);
        }
    }

    public void OnBtnConfirm()
    {
        if(_boxEvent != null)
        {
            _boxEvent(true);
        }

        _parent.Destroy(WindowType.MSG_BOX);
    }

    public void OnBtnCancel()
    {
        if (_boxEvent != null)
        {
            _boxEvent(false);
        }

        _parent.Destroy(WindowType.MSG_BOX);
    }

    public void OnBtnOK()
    {
        if (_boxEvent != null)
        {
            _boxEvent(true);
        }

        _parent.Destroy(WindowType.MSG_BOX);
    }
}
