using UnityEngine;

public class LobbyBtnController : MonoBehaviour
{
    [SerializeField] Btn_Ready btn_Ready;

    // ���� �ִϸ��̼�
    public void Btn_RandomAnim()
    {
        LobbyUIController.Instance.RandomAnimation();
    }

    // ���� ���� ����
    public void Btn_UserInfoSet()
    {
        LobbyUIController.Instance.UserInfoSet();
    }

    // �̱��÷���
    // �������� ����Ʈ â ����
    public void Btn_SinglePlay()
    {
        LobbyUIController.Instance.StageSelectSet(true);
    }
    public void Btn_StageSelectOff()
    {
        LobbyUIController.Instance.StageSelectSet(false);
    }
    public void Btn_StageSelect(int stageIdx)
    {
        LobbyUIController.Instance.StageInfoSet(stageIdx);
    }
    public void Btn_EnterStage()
    {
        int stageIdx = LobbyUIController.Instance.StageIdx;

        if(stageIdx > 0)
            GameManager.Instance.GameStart(GameManager.GameMode.SingleGame, stageIdx);
    }

    // ��Ƽ�÷���
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
}