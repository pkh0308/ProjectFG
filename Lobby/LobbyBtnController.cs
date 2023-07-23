using UnityEngine;

public class LobbyBtnController : MonoBehaviour
{
    [SerializeField] Btn_Ready btn_Ready;

    // 랜덤 애니메이션
    public void Btn_RandomAnim()
    {
        LobbyUIController.Instance.RandomAnimation();
    }

    // 유저 네임 변경
    public void Btn_UserInfoSet()
    {
        LobbyUIController.Instance.UserInfoSet();
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
        LobbyUIController.Instance.StageInfoSet(stageIdx);
    }
    public void Btn_EnterStage()
    {
        int stageIdx = LobbyUIController.Instance.StageIdx;

        if(stageIdx > 0)
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