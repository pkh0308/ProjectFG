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

    // ���� ��Ʈ��ũ
    PhotonView PV;

    public static LobbyUIController Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        PV = GetComponent<PhotonView>();
    }

    void Start()
    {
        waitingText = "�ٸ� ������ ��ٸ��� ��...";
        readyToStartText = "�� ������ ���۵˴ϴ�!";
        
        curChatIdx = -1;
        curChatTime = 0;
        myName = "player" + Random.Range(1000, 10000).ToString();
    }

    #region ��Ƽ�÷���
    public void MultiWaitStart(int curPlayers, int maxPlayers)
    {
        multiWaitText.text = waitingText;
        multiPlayersText.text = $"{curPlayers} / {maxPlayers}";
        multiWaitSet.SetActive(true);

        // �ο��� á������ �غ� ��ư Ȱ��ȭ
        multiReadyBtn.gameObject.SetActive(curPlayers == maxPlayers);
    }

    public void MultiWaitSetOff()
    {
        multiWaitSet.SetActive(false);
    }

    // ���� �ο��� �� �ִ� �ο��� �������� �� ȣ��
    // ��Ƽ�÷��� ���� �ڷ�ƾ ����
    public void ReadyToStart()
    {
        multiReadyBtn.gameObject.SetActive(false);
        StartCoroutine(StartMultiPlay());
    }

    // ���� ��ư ��Ȱ��ȭ �� ī��Ʈ�ٿ� ����
    // 
    IEnumerator StartMultiPlay()
    {
        multiWaitText.text = readyToStartText;
        multiWaitExitBtn.gameObject.SetActive(false);

        // ī��Ʈ�ٿ�
        int count = 5;
        while (count > 0)
        {
            multiPlayersText.text = count.ToString();
            yield return WfsManager.Instance.GetWaitForSeconds(1.0f);
            count--;
        }
        // ��Ƽ�÷��� ����
        GameManager.Instance.StartMultiGame();
    }

    #endregion

    #region ä��
    // Enter �Է� ��
    // Player Input���� ����
    void OnChat()
    {
        // �Է¶��� Ȱ��ȭ�� ���°� �ƴ϶�� Ȱ��ȭ
        if (!inputField.isFocused)
        {
            inputField.ActivateInputField();
            return;
        }
        // �Է¶��� ���� ���̶�� ��Ȱ��ȭ
        if (inputField.text.Trim() == "")
        {
            inputField.DeactivateInputField();
            return;
        }

        // ä�� �Է� �� RPC�� ��ü �������� ���
        inputField.ActivateInputField();
        PV.RPC(nameof(Chat), RpcTarget.All, myName + ": " + inputField.text);
        inputField.text = "";
    }

    // ä�� ��ü ��¿� RPC �Լ�
    // ä��â ���� �ֱ� �ʱ�ȭ
    [PunRPC]
    void Chat(string chatValue)
    {
        curChatIdx++;
        chats[curChatIdx].text = chatValue;
        chats[curChatIdx].gameObject.SetActive(true);
        curChatTime = 0;

        // ä���� ���� ���¿��ٸ� ���ſ� �ڷ�ƾ ����
        if (chatRoutine == null)
            chatRoutine = StartCoroutine(RefreshChat());
    }

    // ä��â ���ſ� �ڷ�ƾ
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
        // ���� �˸�
        chatRoutine = null;
    }

    // ä�� ���� �� �������
    void RemoveOneChat()
    {
        // ���� ä���� ���ٸ� �н�
        if (curChatIdx < 0)
            return;

        // ä�� ��ĭ�� �������
        for(int i = 0; i < curChatIdx; i++)
        {
            if (!chats[i].gameObject.activeSelf)
                break;

            chats[i].text = chats[i + 1].text;
        }
        // ������ ä�� ����
        chats[curChatIdx].gameObject.SetActive(false);
        curChatIdx--;
    }

    #endregion
}