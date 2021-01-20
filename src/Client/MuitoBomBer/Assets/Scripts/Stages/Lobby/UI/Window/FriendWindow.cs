using CommonLib.Messaging.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

internal class FriendWindow : UIComponent
{
    [SerializeField]
    private GameObject _friendSlotModel;

    [SerializeField]
    private UIGrid _grid;

    private List<GameObject> _slots = new List<GameObject>();

    public GameObject _noneFriendObject;
    public UILabel _noneFriendLabel;

    private void Start()
    {
        UpdateState();
    }

    public void UpdateState()
    {
        var lobby = StageManager.GetCurrent<LobbyStage>();
        var friendController = lobby.FriendController;

        foreach(var slot in _slots)
        {
            slot.gameObject.transform.SetParent(null);
            Destroy(slot);
        }

        _slots.Clear();

        foreach(var friend in friendController.Friends)
        {
            var model = GameObject.Instantiate(_friendSlotModel, _grid.gameObject.transform);
            var friendSlot = model.GetComponent<FriendSlotComponent>();

            friendSlot.UpdateAttributes(friend);
            friendSlot.OnDelete = () => RemoveFriend(model);
            friendSlot.OnView = () => ViewFriend(model);
            friendSlot.OnPlay = () => PlayFriend(model);
            friendSlot.OnApprove = () => ApproveFriend(model);

            _slots.Add(model);
            model.SetActive(true);
        }

        _grid.Reposition();
    }

    public void ReturnToMain()
    {
        _parent.Destroy(WindowType.FRIEND);

        var main = _parent.FindInstance(WindowType.MAIN) as MainWindow;
        main?.DisableComponents(false);
    }

    private void PlayFriend(GameObject model)
    {
        //TODO: Implement it.
    }

    private void ViewFriend(GameObject model)
    {
        //TODO: Implement it.
    }

    private void RemoveFriend(GameObject model)
    {
        var slot = model.GetComponent<FriendSlotComponent>();

        var stage = StageManager.GetCurrent<LobbyStage>();
        stage.ServerConnection.Send(new CL_FRIEND_REMOVE_REQ()
        {
            nick = slot.Nick,
        });

        stage.ShowWaiting("Removing friend...");
    }

    private void ApproveFriend(GameObject model)
    {
        var slot = model.GetComponent<FriendSlotComponent>();

        var stage = StageManager.GetCurrent<LobbyStage>();
        stage.ServerConnection.Send(new CL_FRIEND_RESPONSE_REQ()
        {
            nick = slot.Nick,
            accept = true,
        });

        stage.ShowWaiting("Approving friend request...");
    }

    public void OnAddFriendClick()
    {
        _parent.Instanciate(WindowType.FRIEND_ADD);
    }
}
