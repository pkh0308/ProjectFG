using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIController : MonoBehaviour
{
    [Header("User Name")]
    [SerializeField] GameObject nameEditSet;
    [SerializeField] TextMeshProUGUI userNameText;
    [SerializeField] TMP_InputField nameField;
    string myName;

    [Header("Single Play")]
    [SerializeField] GameObject stageSelectSet;
    [SerializeField] GameObject stageInfoSet;
    [SerializeField] TextMeshProUGUI stageNameText;
    [SerializeField] TextMeshProUGUI stageDescriptionText;
    [SerializeField] TextMeshProUGUI stageTypeText;
    int stageIdx = -1;
    public int StageIdx { get{ return stageIdx; } }
    List<StageData> stageDataList;

    [Header("Multi Play")]
    [SerializeField] GameObject multiWaitSet;
    [SerializeField] TextMeshProUGUI multiWaitText;
    [SerializeField] TextMeshProUGUI multiPlayersText;
    [SerializeField] Button multiWaitExitBtn;
    [SerializeField] Button multiReadyBtn;
    string waitingText;
    string readyToStartText;

    [Header("Chatting")]
    [SerializeField] TMP_InputField inputField;
    [SerializeField] TextMeshProUGUI[] chats;
    [SerializeField] int chatRefreshInterval;
    int curChatIdx;
    int curChatTime;
    Coroutine chatRoutine;

    // 포톤 네트워크
    PhotonView PV;

    public static LobbyUIController Instance { get; private set; }

    #region 초기화
    void Awake()
    {
        Instance = this;
        PV = GetComponent<PhotonView>();
    }

    void Start()
    {
        waitingText = "다른 유저를 기다리는 중...";
        readyToStartText = "곧 게임이 시작됩니다!";
        
        curChatIdx = -1;
        curChatTime = 0;
        // 임시 유저 네임
        myName = "player" + Random.Range(1000, 10000).ToString();
        userNameText.text = myName;

        // 스테이지 정보 읽어오기
        ReadStageDatas();
    }

    void ReadStageDatas()
    {
        stageDataList = new List<StageData>(10);

        //csv 파일에서 데이터 읽어오기
        TextAsset stageDatas = Resources.Load("StageDatas") as TextAsset;
        StringReader stageDataReader = new StringReader(stageDatas.text);

        if (stageDataReader == null)
        {
            Debug.Log("stageDataReader is null");
            return;
        }
        //첫줄 스킵(변수 이름 라인)
        string line = stageDataReader.ReadLine();
        if (line == null) return;

        string[] datas;
        line = stageDataReader.ReadLine();
        while (line.Length > 1)
        {
            datas = line.Split(',');
            stageDataList.Add(new StageData(datas[0], datas[1], datas[2], datas[3]));
            // 다음 줄 읽기
            line = stageDataReader.ReadLine();
        }
        stageDataReader.Close();
    }
    #endregion

    #region 유저 네임 변경
    public void NameEditSet()
    {
        nameEditSet.SetActive(!nameEditSet.activeSelf);
    }

    public void EditUserName()
    {
        myName = nameField.text;
        userNameText.text = myName;
        nameEditSet.SetActive(false);
    }
    #endregion

    #region 싱글플레이
    public void StageSelectSet(bool act)
    {
        stageSelectSet.SetActive(act);

        // 정보창 닫기 및 스테이지 인덱스 초기화
        if(act == false)
        {
            stageInfoSet.SetActive(false);
            stageIdx = -1;
        }
    }

    public void StageInfoSet(int idx)
    {
        stageIdx = idx;
        // 첫번째 스테이지 인덱스가 리스트 첫번째 인덱스(0)가 되도록 수정
        idx -= SceneController.Instance.FirstStageIdx;

        // 스테이지 이름
        stageNameText.text = stageDataList[idx].stageName;
        // 스테이지 설명
        stageDescriptionText.text = stageDataList[idx].stageDescription;
        // 스테이지 타입(타입 / 시간 제한)
        string timeLimit = stageDataList[idx].timeLimit > 0 ? stageDataList[idx].timeLimit.ToString() + "초" : "없음";
        stageTypeText.text = $"(스테이지 타입: {stageDataList[idx].stageType} / 제한 시간: {timeLimit})";

        stageInfoSet.SetActive(true);
    }

    public void EnterStage()
    {
        GameManager.Instance.GameStart(GameManager.GameMode.SingleGame, stageIdx);
    }
    #endregion

    #region 멀티플레이
    public void MultiWaitStart(int curPlayers, int maxPlayers)
    {
        multiWaitText.text = waitingText;
        multiPlayersText.text = $"{curPlayers} / {maxPlayers}";
        multiWaitSet.SetActive(true);

        // 인원이 찼을때만 준비 버튼 활성화
        multiReadyBtn.gameObject.SetActive(curPlayers == maxPlayers);
    }

    public void MultiWaitSetOff()
    {
        multiWaitSet.SetActive(false);
    }

    // 현재 인원이 방 최대 인원과 같아졌을 때 호출
    // 멀티플레이 시작 코루틴 실행
    public void ReadyToStart()
    {
        multiReadyBtn.gameObject.SetActive(false);
        StartCoroutine(StartMultiPlay());
    }

    // 퇴장 버튼 비활성화 및 카운트다운 시작
    // 
    IEnumerator StartMultiPlay()
    {
        multiWaitText.text = readyToStartText;
        multiWaitExitBtn.gameObject.SetActive(false);

        // 카운트다운
        int count = 5;
        while (count > 0)
        {
            multiPlayersText.text = count.ToString();
            yield return WfsManager.Instance.GetWaitForSeconds(1.0f);
            count--;
        }
        // 멀티플레이 시작
        GameManager.Instance.StartMultiGame();
    }

    #endregion

    #region 채팅
    // Enter 입력 시
    // Player Input으로 관리
    void OnChat()
    {
        // 입력란이 활성화된 상태가 아니라면 활성화
        if (!inputField.isFocused)
        {
            inputField.ActivateInputField();
            return;
        }
        // 입력란이 공백 뿐이라면 비활성화
        if (inputField.text.Trim() == "")
        {
            inputField.DeactivateInputField();
            return;
        }

        // 채팅 입력 시 RPC로 전체 유저에게 출력
        inputField.ActivateInputField();
        PV.RPC(nameof(Chat), RpcTarget.All, myName + ": " + inputField.text);
        inputField.text = "";
    }

    // 채팅 전체 출력용 RPC 함수
    // 채팅창 갱신 주기 초기화
    [PunRPC]
    void Chat(string chatValue)
    {
        // 채팅이 꽉 찬 경우
        // 오래된 채팅 제거 로직 바로 실행
        if(curChatIdx == chats.Length - 1)
            RemoveOneChat();
        else
            curChatIdx++;

        chats[curChatIdx].text = chatValue;
        chats[curChatIdx].gameObject.SetActive(true);
        curChatTime = 0;

        // 채팅이 없는 상태였다면 갱신용 코루틴 실행
        if (chatRoutine == null)
            chatRoutine = StartCoroutine(RefreshChat());
    }

    // 채팅창 갱신용 코루틴
    IEnumerator RefreshChat()
    {
        while (curChatIdx >= 0)
        {
            yield return WfsManager.Instance.GetWaitForSeconds(1.0f);
            curChatTime++;

            if(curChatTime >= chatRefreshInterval)
            {
                RemoveOneChat();
                curChatTime = 0;
            } 
        }
        // 종료 알림
        chatRoutine = null;
    }

    // 채팅 제거 및 끌어오기
    void RemoveOneChat()
    {
        // 현재 채팅이 없다면 패스
        if (curChatIdx < 0)
            return;

        // 채팅 한칸씩 끌어내리기
        for(int i = 0; i < curChatIdx; i++)
        {
            if (!chats[i].gameObject.activeSelf)
                break;

            chats[i].text = chats[i + 1].text;
        }
        // 오래된 채팅 제거
        chats[curChatIdx].gameObject.SetActive(false);
        curChatIdx--;
    }
    #endregion
}