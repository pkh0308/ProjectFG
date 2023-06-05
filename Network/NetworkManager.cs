using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

using PN = Photon.Pun.PhotonNetwork;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance { get; private set; }

    public int CurUsers { get { return PN.CurrentRoom.Players.Count; } }
    public int MaxUsers { get { return PN.CurrentRoom.MaxPlayers; } }
    public bool IsMaster { get { return PN.IsMasterClient; } }
    public bool InRoom { get { return PN.InRoom; }}

    PhotonView PV;
    int curReadyUsers;

    void Awake()
    {
        Instance = this;
        PV = GetComponent<PhotonView>();

        //�⺻�� 60, 30 ���� �������� ����
        PN.SendRate = 30;
        PN.SerializationRate = 15;
    }

    // ���� ����
    // Photon\PhotonUnityNetworking\Resources ���� �� PhotonNetworkSettings �ɼ����� ����
    public void ConnectToServer()
    {
        PN.ConnectUsingSettings();
    }

    // �÷��̾� �ε��� Ȯ��
    // �ش� �÷��̾ ������ -1 ��ȯ
    public int GetMyIdx()
    {
        var playerArr = PN.PlayerList;
        for (int i = 0; i < playerArr.Length; i++)
        {
            if (playerArr[i] == PN.LocalPlayer)
                return i;
        }
        // �ش� �÷��̾ ���� ���
        return -1;
    }   

    #region ���� �ݹ�
    // ���� ���� ��
    // �κ�� �ٷ� ����
    public override void OnConnectedToMaster()
    {
        PN.JoinLobby();
    }
    // �κ� ���� �� 
    // �κ� �� �ε� �� ä�ù� ����
    public override void OnJoinedLobby()
    {
        SceneController.Instance.LoadLobby();
    }
    // �� ���� ��
    // ������� ���� ��� ������Ʈ �ϵ��� RPC�� ���� 
    public override void OnJoinedRoom()
    {
        PV.RPC(nameof(MultiUIUpdate), RpcTarget.All);
    }
    //������ ���� ��� ������ �õ�
    public override void OnDisconnected(DisconnectCause cause)
    {
        PN.Reconnect(); Debug.Log("Reconnect...");
    }

    // Ÿ �÷��̾��� �� ����, ���� ��
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log("�÷��̾� ����");
    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log("�÷��̾� ����");
    }

    // �� ���� ��
    public override void OnLeftRoom()
    {
        // ��Ƽ�� ����
        // ��� UI ��Ȱ��ȭ
        LobbyUIController.Instance.MultiWaitSetOff();
    }
    #endregion

    #region ������Ʈ ����
    // ĳ���� ����
    // �÷��̾��� �ε����� ���� ��ġ ����
    public GameObject InstantiateCharacter(Transform[] positions)
    {
        int idx = GetMyIdx();
        if (idx < 0)
            return null;

        string playerName = "Player" + idx.ToString();
        return PN.Instantiate(playerName, positions[idx].position, Quaternion.identity);
    }

    // �ش� �̸��� ������Ʈ ����
    // ��ġ�� ����Ʈ(0, 0, 0)
    public GameObject Instantiate(string name)
    {
        return PN.Instantiate(name, Vector3.zero, Quaternion.identity);
    }
    // ���� ��ġ�� �ش� �̸��� ������Ʈ ����
    public GameObject Instantiate(string name, Vector3 pos)
    {
        return PN.Instantiate(name, pos, Quaternion.identity);
    }
    #endregion

    #region ��Ƽ�÷��� 
    // ä�ù� ���� �� ��Ƽ�� ����
    // ��Ƽ�� ������ OnLeftRoom()���� ó��
    public void EnterRoom()
    {
        PN.JoinOrCreateRoom("room", new RoomOptions() { MaxPlayers = 2 }, null);
    }
    // ��Ƽ�÷��� ��� �� ����
    // ���� �Ϸ� �� ä�ù� ������
    public void LeaveRoom()
    {
        PN.LeaveRoom();
    }
    // ��Ƽ�÷��� UI ������Ʈ
    [PunRPC]
    void MultiUIUpdate()
    {
        LobbyUIController.Instance.MultiWaitStart(CurUsers, MaxUsers);
    }

    public void Ready(bool ready)
    {
        PV.RPC(nameof(CountReady), RpcTarget.All, ready);
    }

    [PunRPC]
    void CountReady(bool ready)
    {
        curReadyUsers = ready ? curReadyUsers + 1 : curReadyUsers - 1;
        // �ο��� �� á�ٸ� ��Ƽ�÷��� ����
        if (curReadyUsers == PN.CurrentRoom.MaxPlayers) 
            LobbyUIController.Instance.ReadyToStart();
    }
    
    #endregion
}