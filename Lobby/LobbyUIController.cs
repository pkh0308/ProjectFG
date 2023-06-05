using Photon.Pun;
using System.Collections;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIController : MonoBehaviour
{
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
    [SerializeField] GameObject chatText;
    [SerializeField] Transform chatField;
    [SerializeField] TextMeshProUGUI[] chats;
    int curChatIdx;
    [SerializeField] float chatRefreshInterval;
    float curChatTime;
    string myName;
    Coroutine chatRoutine;

    // 포톤 네트워크
    PhotonView PV;

    public static LobbyUIController Instance { get; private set; }

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
        myName = "player" + Random.Range(1000, 10000).ToString();
    }

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
            yield return WfsManager.Instance.GetWaitForSeconds(0.1f);
            curChatTime += 0.1f;

            if(curChatTime > chatRefreshInterval)
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