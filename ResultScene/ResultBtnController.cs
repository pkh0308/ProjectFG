using UnityEngine;

public class ResultBtnController : MonoBehaviour
{
    public void Btn_ResultExit()
    {
        // ��Ƽ ����
        if(NetworkManager.Instance.InRoom)
        {
            // �κ� �̵� �� �� ������
            SceneController.Instance.BackToLobby();
            NetworkManager.Instance.LeaveRoom();
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