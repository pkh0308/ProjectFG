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

        //기본값 60, 30 에서 절반으로 낮춤
        PN.SendRate = 30;
        PN.SerializationRate = 15;
    }

    // 서버 연결
    // Photon\PhotonUnityNetworking\Resources 폴더 내 PhotonNetworkSettings 옵션으로 연결
    public void ConnectToServer()
    {
        PN.ConnectUsingSettings();
    }

    // 플레이어 인덱스 확인
    // 해당 플레이어가 없으면 -1 반환
    public int GetMyIdx()
    {
        var playerArr = PN.PlayerList;
        for (int i = 0; i < playerArr.Length; i++)
        {
            if (playerArr[i] == PN.LocalPlayer)
                return i;
        }
        // 해당 플레이어가 없는 경우
        return -1;
    }   

    #region 포톤 콜백
    // 서버 연결 시
    // 로비로 바로 입장
    public override void OnConnectedToMaster()
    {
        PN.JoinLobby();
    }
    // 로비 입장 시 
    // 로비 씬 로딩 및 채팅방 입장
    public override void OnJoinedLobby()
    {
        // 이미 로비라면 패스
        if (GameManager.Instance.CurMode == GameManager.GameMode.Lobby)
            return;

        GameManager.Instance.SetMode(GameManager.GameMode.Lobby);
        SceneController.Instance.LoadLobby();
    }
    // 방 입장 시
    // 대기중인 유저 모두 업데이트 하도록 RPC로 실행 
    public override void OnJoinedRoom()
    {
        PV.RPC(nameof(MultiUIUpdate), RpcTarget.All);
    }
    //접속이 끊길 경우 재접속 시도
    public override void OnDisconnected(DisconnectCause cause)
    {
        PN.Reconnect(); Debug.Log("Reconnect...");
    }

    // 타 플레이어 방 입장 시
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        
    }
    // 타 플레이어 방 퇴장 시
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        // 다른 유저가 나갔을 경우 내 UI 업데이트
        MultiUIUpdate();
    }

    // 방 퇴장 시
    // 로비일 경우(멀티 대기중에 퇴장 시) 대기 UI 비활성화
    public override void OnLeftRoom()
    {
        if (GameManager.Instance.CurMode != GameManager.GameMode.Lobby)
            return;

        // 대기 UI 비활성화
        LobbyUIController.Instance.MultiWaitSetOff();
    }
    #endregion

    #region 오브젝트 생성
    // 캐릭터 생성
    // 플레이어의 인덱스에 따라 위치 조절
    public GameObject InstantiateCharacter(Transform[] positions)
    {
        int idx = GetMyIdx();
        if (idx < 0)
            return null;

        string playerName = "Player" + idx.ToString();
        return PN.Instantiate(playerName, positions[idx].position, Quaternion.identity);
    }

    // 해당 이름의 오브젝트 생성
    // 위치는 디폴트(0, 0, 0)
    public GameObject Instantiate(string name)
    {
        return PN.Instantiate(name, Vector3.zero, Quaternion.identity);
    }
    // 지정 위치에 해당 이름의 오브젝트 생성
    public GameObject Instantiate(string name, Vector3 pos)
    {
        return PN.Instantiate(name, pos, Quaternion.identity);
    }
    #endregion

    #region 멀티플레이 
    // 채팅방 퇴장 및 멀티방 입장
    // 멀티방 입장은 OnLeftRoom()에서 처리
    public void EnterRoom()
    {
        // 방이 있다면 랜덤 입장, 없다면 생성
        // 최대 인원수 2인 방에 참가, 없다면 최대 인원수 2인 방 생성
        PN.JoinRandomOrCreateRoom(
            expectedMaxPlayers: 2,
            roomOptions: new RoomOptions() { MaxPlayers = 2 });
    }
    // 멀티플레이 대기 방 퇴장
    // 퇴장 완료 후 채팅방 재입장
    public void LeaveRoom()
    {
        // 퇴장 시 curReadyUsers는 0으로 초기화
        curReadyUsers = 0;
        PN.LeaveRoom();
    }

    // 멀티플레이 UI 업데이트
    [PunRPC]
    void MultiUIUpdate()
    {
        LobbyUIController.Instance.MultiWaitStart(CurUsers, MaxUsers);
        // 누군가 퇴장한 상황이라면 curReadyusers 초기화
        if (CurUsers < MaxUsers)
            curReadyUsers = 0;
    }

    public void Ready(bool ready)
    {
        PV.RPC(nameof(CountReady), RpcTarget.All, ready);
    }

    [PunRPC]
    void CountReady(bool ready)
    {
        curReadyUsers = ready ? curReadyUsers + 1 : curReadyUsers - 1;
        // 인원이 꽉 찼다면 멀티플레이 시작
        if (curReadyUsers == PN.CurrentRoom.MaxPlayers) 
            LobbyUIController.Instance.ReadyToStart();
    }
    
    #endregion
}