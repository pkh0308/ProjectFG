using UnityEngine;

public class ResultBtnController : MonoBehaviour
{
    public void Btn_ResultExit()
    {
        // 멀티 게임
        if(NetworkManager.Instance.InRoom)
        {
            // 로비 이동 및 룸 나가기
            SceneController.Instance.BackToLobby();
            NetworkManager.Instance.LeaveRoom();
        }
        // 싱글 게임
        else
        {
            // 로비 이동
            SceneController.Instance.BackToLobby();
        }
        // 변수 초기화
        GameManager.Instance.Reinitialize();
    }
}