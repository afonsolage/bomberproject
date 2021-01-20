using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatMessageSlotComponent : MonoBehaviour
{
    private readonly int SPACE_INCREASE_BALLON = 34;

    private ulong _playerIndex;
    private string _playerName;

    public UILabel _playerNameLabel;
    public UILabel _messageLabel;

    public GameObject _profileObject;
    public UI2DSprite _playerAvatarSprite;
    public UI2DSprite _ballonSprite;

    public void Message(string playerName, string msg)
    {
        _playerName = playerName;
        _playerNameLabel.text = string.Format("[{0}] :", _playerName);
        _messageLabel.text = msg;

        AdjustBallon();
    }

    public void Message(ulong playerIndex, string playerName, string msg, bool self = false)
    {
        _playerIndex = playerIndex;
        _playerName = playerName;

        _playerNameLabel.text = string.Format("[{0}] :", _playerName);
        _messageLabel.text = msg;

        if (self) AdjustSelf();

        AdjustBallon();
    }

    private void AdjustSelf()
    {
        // Ballon Adjust.
        _ballonSprite.pivot = UIWidget.Pivot.Right;
        _ballonSprite.flip = UIBasicSprite.Flip.Horizontally;

        var posBallon = _ballonSprite.gameObject.transform.localPosition;
        posBallon.x = 328f;

        _ballonSprite.gameObject.transform.localPosition = posBallon;

        // Name label adjust.
        _playerNameLabel.pivot = UIWidget.Pivot.Right;
        var posName = _playerNameLabel.gameObject.transform.localPosition;
        posName.x = 316f;
        _playerNameLabel.gameObject.transform.localPosition = posName;

        // Message adjust.
        _messageLabel.pivot = UIWidget.Pivot.Right;
        var posMsg = _messageLabel.gameObject.transform.localPosition;
        posMsg.x = 306f;
        _messageLabel.gameObject.transform.localPosition = posMsg;

        // Profile adjust.
        var posProfile = _profileObject.gameObject.transform.localPosition;
        posProfile.x = 378f;
        _profileObject.gameObject.transform.localPosition = posProfile;
    }

    private void AdjustBallon()
    {
        int messageWidth = (int)_messageLabel.printedSize.x;
        messageWidth += SPACE_INCREASE_BALLON;

        _ballonSprite.width = messageWidth;
    }
}
