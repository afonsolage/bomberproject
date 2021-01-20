using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomComponent : MonoBehaviour
{
    public UI2DSprite _background;

    public UILabel _id;
    public UILabel _title;
    public UILabel _quantity;
    public UI2DSprite _password;

    public UILabel _waitingSprite;
    public UILabel _fullSprite;
    public UILabel _playingSprite;

    public class RoomData : MonoBehaviour
    {
        public uint index;
        public string title;
        public uint playerCnt;
        public uint maxPlayer;
        public bool isPublic;
        public CommonLib.Messaging.RoomStage stage;
    }

    public void SetTitle(string title)
    {
        _title.text = title;
    }

    public void SetID(uint id)
    {
       _id.text = id.ToString("D3");
    }

    public void SetQuantity(uint value, uint maxValue)
    {
        _quantity.text = string.Format("{0}/{1}", value, maxValue);
    }

    public void SetStatusRoom(CommonLib.Messaging.RoomStage stage)
    {
        switch(stage)
        {
            case CommonLib.Messaging.RoomStage.Waiting:
                {
                    _waitingSprite.gameObject.SetActive(true);
                    _fullSprite.gameObject.SetActive(false);
                    _playingSprite.gameObject.SetActive(false);
                }
                break;
            case CommonLib.Messaging.RoomStage.Full:
                {
                    _waitingSprite.gameObject.SetActive(false);
                    _fullSprite.gameObject.SetActive(true);
                    _playingSprite.gameObject.SetActive(false);
                }
                break;
            case CommonLib.Messaging.RoomStage.Playing:
                {
                    _waitingSprite.gameObject.SetActive(false);
                    _fullSprite.gameObject.SetActive(false);
                    _playingSprite.gameObject.SetActive(true);
                }
                break;
        }
    }
}
