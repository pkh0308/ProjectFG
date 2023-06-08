using UnityEngine;

public class LobbyBtnController : MonoBehaviour
{
    [SerializeField] Btn_Ready btn_Ready;

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
        btn_Ready.ChangeReady();
    }
}