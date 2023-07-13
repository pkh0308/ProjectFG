using UnityEngine;

public class ResultBtnController : MonoBehaviour
{
    public void Btn_ResultExit()
    {
        // 로비 이동
        SceneController.Instance.BackToLobby();
        // 변수 초기화
        GameManager.Instance.Reinitialize();
    }
}