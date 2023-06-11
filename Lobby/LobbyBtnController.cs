using UnityEngine;

public class LobbyBtnController : MonoBehaviour
{
    [SerializeField] Btn_Ready btn_Ready;

    // ½ºÅ×ÀÌÁö ¼¿·ºÆ® Ã¢ ¿ÀÇÂ
    public void Btn_SinglePlay()
    {
        LobbyUIController.Instance.StageSelectSet(true);
    }

    public void Btn_StageSelectOff()
    {
        LobbyUIController.Instance.StageSelectSet(false);
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

    public void Btn_StageSelect(int stageIdx)
    {
        GameManager.Instance.GameStart(GameManager.GameMode.SingleGame, stageIdx);
    }
}