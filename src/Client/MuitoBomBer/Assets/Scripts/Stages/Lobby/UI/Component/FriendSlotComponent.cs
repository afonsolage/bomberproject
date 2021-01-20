using CommonLib.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class FriendSlotComponent : MonoBehaviour
{
    [SerializeField]
    private UIButton _deleteBtn;
    [SerializeField]
    private UIButton _viewBtn;
    [SerializeField]
    private UIButton _playBtn;
    [SerializeField]
    private UIButton _approveBtn;

    [SerializeField]
    private UILabel _level;

    [SerializeField]
    private UILabel _name;

    [SerializeField]
    private UILabel[] _status;

    [HideInInspector]
    public EventDelegate.Callback OnDelete;

    [HideInInspector]
    public EventDelegate.Callback OnView;

    [HideInInspector]
    public EventDelegate.Callback OnPlay;

    [HideInInspector]
    public EventDelegate.Callback OnApprove;


    public string Nick { get { return _name.text; } }

    public void OnClickDelete()
    {
        if (OnDelete != null)
            OnDelete();
    }

    public void OnClickView()
    {
        if (OnView != null)
            OnView();
    }

    public void OnClickPlay()
    {
        if (OnPlay != null)
            OnPlay();
    }

    public void OnClickApprove()
    {
        if (OnApprove != null)
            OnApprove();
    }

    public void UpdateAttributes(Friend friend)
    {
        _name.text = friend.Nick;

        foreach (var obj in _status)
        {
            obj.gameObject.SetActive(false);
        }

        _status[(int)friend.State].gameObject.SetActive(true);

        //TODO: Maybe also move buttons or just disable (not hide) some?
        switch(friend.State)
        {
            case FriendState.Online:
                _approveBtn.gameObject.SetActive(false);
                _playBtn.gameObject.SetActive(true);
                _deleteBtn.gameObject.SetActive(true);
                _viewBtn.gameObject.SetActive(true);
                break;
            case FriendState.Offline:
                _approveBtn.gameObject.SetActive(false);
                _playBtn.gameObject.SetActive(false);
                _deleteBtn.gameObject.SetActive(true);
                _viewBtn.gameObject.SetActive(true);
                break;
            case FriendState.Requested:
                _approveBtn.gameObject.SetActive(false);
                _playBtn.gameObject.SetActive(false);
                _deleteBtn.gameObject.SetActive(true);
                _viewBtn.gameObject.SetActive(false);
                break;
            case FriendState.WaitingApproval:
                _approveBtn.gameObject.SetActive(true);
                _playBtn.gameObject.SetActive(false);
                _deleteBtn.gameObject.SetActive(true);
                _viewBtn.gameObject.SetActive(false);
                break;
        }
    }
}
