using UnityEngine;

public class ResultBtnController : MonoBehaviour
{
    public void Btn_ResultExit()
    {
        // ��Ƽ ����
        if(NetworkManager.Instance.InRoom)
        {
            // �κ� �̵� �� �� ������
            NetworkManager.Instance.LeaveRoom();
            SceneController.Instance.BackToLobby();
        }
        // �̱� ����
        else
        {
            // �κ� �̵�
            SceneController.Instance.BackToLobby();
        }
        // ���� �ʱ�ȭ
        GameManager.Instance.Reinitialize();
    }
}