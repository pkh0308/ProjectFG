using UnityEngine;

public class LobbyBtnController : MonoBehaviour
{
    [SerializeField] Btn_Ready btn_Ready;

    // 유저 네임 변경
    public void Btn_NameEditSet()
    {
        LobbyUIController.Instance.NameEditSet();
    }
    public void Btn_NameEdit()
    {
        LobbyUIController.Instance.EditUserName();
    }

    // 싱글플레이
    // 스테이지 셀렉트 창 오픈
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
        GameManager.Instance.GameStart(GameManager.GameMode.SingleGame, stageIdx);
    }

    // 멀티플레이
    public void Btn_MultiPlay()
    {
        GameManager.Instance.GameStart(GameManager.GameMode.MultiGame);
    }
    public void Btn_MultiWaitCancle()
    {
        // 네트워크 룸 퇴장
        NetworkManager.Instance.LeaveRoom();
    }
    public void Btn_MultiReady()
    {
        btn_Ready.ChangeReady();
    }
}