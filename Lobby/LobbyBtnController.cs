using UnityEngine;

public class LobbyBtnController : MonoBehaviour
{
    bool curReady = false;

    public void Btn_SinglePlay()
    {
        GameManager.Instance.GameStart(GameManager.GameMode.SingleGame);
    }

    public void Btn_MultiPlay()
    {
        GameManager.Instance.GameStart(GameManager.GameMode.MultiGame);
    }

    public void Btn_MultiWaitCancle()
    {
        // ³×Æ®¿öÅ© ·ë ÅðÀå
        NetworkManager.Instance.LeaveRoom();
    }

    public void Btn_MultiReady()
    {
        curReady = !curReady;
        NetworkManager.Instance.Ready(curReady);
    }
}