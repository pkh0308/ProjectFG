using UnityEngine;

public class LobbyBtnController : MonoBehaviour
{
    [SerializeField] Btn_Ready btn_Ready;

    // �������� ����Ʈ â ����
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
        // ��Ʈ��ũ �� ����
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