using UnityEngine;

public class WinnerWindow : UIComponent
{
    public UILabel winnerLabel;

    public void SetMessage(string text)
    {
        winnerLabel.text = text;
    }

    public void GobackToLobby()
    {
        StageManager.ChangeStage(StageType.Lobby);
    }
}