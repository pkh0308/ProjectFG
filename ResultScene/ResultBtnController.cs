using UnityEngine;

public class ResultBtnController : MonoBehaviour
{
    public void Btn_ResultExit()
    {
        // �κ� �̵�
        SceneController.Instance.BackToLobby();
        // ���� �ʱ�ȭ
        GameManager.Instance.Reinitialize();
    }
}