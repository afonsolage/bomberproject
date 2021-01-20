using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSlotComponent : UIComponent
{
    private int _slotIndex;
    public int SlotIndex { get { return _slotIndex; } set { _slotIndex = value; } }

    private ulong _playerIndex = 0;
    public ulong PlayerIndex { get { return _playerIndex; } set { _playerIndex = value; } }

    public GameObject _entityObject;

    public UI2DSprite _offlineSprite;

    public GameObject _ownerObject;
    public UILabel _ownerLabel;

    public UILabel _nameLabel;
    public UILabel _lvlLabel;

    public GameObject _readyObject;
    public UILabel _readyLabel;

    public UI2DSprite _background;

    public PlayerSlotDragDropItem _dragDropItem;

    public List<GameObject> _wifiSprites = new List<GameObject>();

    public GameObject _availableStatus;
    public GameObject _closedStatus;

    private void Start()
    {
        if (_parent == null)
        {
            var uiRoot = GameObject.Find("UI Root");
            Setup(uiRoot?.GetComponent<UIManager>());
        }

        _dragDropItem.pressAndHoldDelay = 0.05f;
    }

    public void Show(string name, ulong playerIndex, uint level, bool isOwner)
    {
        _nameLabel.text = name;
        _lvlLabel.text = string.Format("Lv. {0}", level);
        _playerIndex = playerIndex;

        _entityObject.SetActive(true);

        _nameLabel.gameObject.SetActive(true);
        _lvlLabel.gameObject.SetActive(true);

        //_readyObject.SetActive(isOwner);
        _ownerObject.SetActive(isOwner);

        //_background.gameObject.SetActive(true);

        _availableStatus.SetActive(false);
        _closedStatus.SetActive(false);
    }

    public void Hide()
    {
        _entityObject.SetActive(false);

        _nameLabel.gameObject.SetActive(false);
        _nameLabel.text = "";

        _lvlLabel.gameObject.SetActive(false);
        _lvlLabel.text = string.Format("Lv. {0}", 0);

        _playerIndex = 0;

        _offlineSprite.gameObject.SetActive(false);
        _readyObject.SetActive(false);
        _ownerObject.SetActive(false);

        //_background.gameObject.SetActive(false);
        _availableStatus.SetActive(true);
        _closedStatus.SetActive(false);

        DisablePing();
    }

    internal void SetInteractableDragDropItem(bool interactable)
    {
        _dragDropItem.interactable = interactable;
    }

    public void Offline(bool val)
    {
        _offlineSprite.gameObject.SetActive(val);
    }

    public void Ready(bool val)
    {
        _readyObject.SetActive(val);
    }

    private void OpenMenu()
    {
        if (_playerIndex != 0)
        {
            if (_parent.FindInstance(WindowType.ROOM_PLAYER_MENU))
            {
                _parent.Destroy(WindowType.ROOM_PLAYER_MENU);
            }
            else
            {
                var menu = _parent.Instanciate(WindowType.ROOM_PLAYER_MENU) as PlayerMenuComponent;

                // Set position of last touch on screen.
                menu.SetLastTouch(UICamera.lastEventPosition.x, UICamera.lastEventPosition.y);
                menu.SetPlayerInfo(_playerIndex, _nameLabel.text);
            }
        }
        else
        {
            // Let to check menu is already instancied, if yes, let to destroy.
            _parent.FindInstance(WindowType.ROOM_PLAYER_MENU, false, true);
        }
    }

    public void OnClickedTap()
    {
        OpenMenu();
    }

    private float _onDraggedTime = 0;
    private readonly float DRAGGED_TIME_OPEN_MENU = 0.5f;

    private void Update()
    {
        if (_dragDropItem == null)
            return;

        if (_dragDropItem.IsPressed)
        {
            _onDraggedTime += Time.deltaTime;
        }
        else if (_onDraggedTime > 0f && _onDraggedTime < DRAGGED_TIME_OPEN_MENU && !_dragDropItem.IsPressed)
        {
            _onDraggedTime = 0;

            OpenMenu();
        }
        else if (_onDraggedTime > DRAGGED_TIME_OPEN_MENU && !_dragDropItem.IsPressed)
        {
            _onDraggedTime = 0;
        }
    }

    public void OnDragStart()
    {
        _dragDropItem?.OnDragStartByWidget();
    }

    public void OnDragEnd()
    {
        _onDraggedTime = 0;
    }

    public void DisablePing()
    {
        foreach (var g in _wifiSprites)
        {
            g.gameObject.SetActive(false);
        }
    }

    public void SetPing(long ping)
    {
        if (ping < 100)
        {
            _wifiSprites[0].SetActive(false);
            _wifiSprites[1].SetActive(true);
            _wifiSprites[2].SetActive(true);
            _wifiSprites[3].SetActive(true);
        }
        else if (ping >= 100 && ping < 150)
        {
            _wifiSprites[0].SetActive(false);
            _wifiSprites[1].SetActive(false);
            _wifiSprites[2].SetActive(true);
            _wifiSprites[3].SetActive(true);
        }
        else if (ping >= 150 && ping < 250)
        {
            _wifiSprites[0].SetActive(false);
            _wifiSprites[1].SetActive(false);
            _wifiSprites[2].SetActive(false);
            _wifiSprites[3].SetActive(true);
        }
        else if (ping >= 250)
        {
            _wifiSprites[0].SetActive(true);
            _wifiSprites[1].SetActive(false);
            _wifiSprites[2].SetActive(false);
            _wifiSprites[3].SetActive(false);
        }
    }
}
