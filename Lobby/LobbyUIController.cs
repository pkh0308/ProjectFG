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

    // ���� ��Ʈ��ũ
    PhotonView PV;

    public static LobbyUIController Instance { get; private set; }

    #region �ʱ�ȭ
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
        // �ӽ� ���� ����
        myName = "player" + Random.Range(1000, 10000).ToString();
        userNameText.text = myName;

        // �������� ���� �о����
        ReadStageDatas();
    }

    void ReadStageDatas()
    {
        stageDataList = new List<StageData>(10);

        //csv ���Ͽ��� ������ �о����
        TextAsset stageDatas = Resources.Load("StageDatas") as TextAsset;
        StringReader stageDataReader = new StringReader(stageDatas.text);

        if (stageDataReader == null)
        {
            Debug.Log("stageDataReader is null");
            return;
        }
        //ù�� ��ŵ(���� �̸� ����)
        string line = stageDataReader.ReadLine();
        if (line == null) return;

        string[] datas;
        line = stageDataReader.ReadLine();
        while (line.Length > 1)
        {
            datas = line.Split(',');
            stageDataList.Add(new StageData(datas[0], datas[1], datas[2], datas[3]));
            // ���� �� �б�
            line = stageDataReader.ReadLine();
        }
        stageDataReader.Close();
    }
    #endregion

    #region ���� ���� ����
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

    #region �̱��÷���
    public void StageSelectSet(bool act)
    {
        stageSelectSet.SetActive(act);

        // ����â �ݱ� �� �������� �ε��� �ʱ�ȭ
        if(act == false)
        {
            stageInfoSet.SetActive(false);
            stageIdx = -1;
        }
    }

    public void StageInfoSet(int idx)
    {
        stageIdx = idx;
        // ù��° �������� �ε����� ����Ʈ ù��° �ε���(0)�� �ǵ��� ����
        idx -= SceneController.Instance.FirstStageIdx;

        // �������� �̸�
        stageNameText.text = stageDataList[idx].stageName;
        // �������� ����
        stageDescriptionText.text = stageDataList[idx].stageDescription;
        // �������� Ÿ��(Ÿ�� / �ð� ����)
        string timeLimit = stageDataList[idx].timeLimit > 0 ? stageDataList[idx].timeLimit.ToString() + "��" : "����";
        stageTypeText.text = $"(�������� Ÿ��: {stageDataList[idx].stageType} / ���� �ð�: {timeLimit})";

        stageInfoSet.SetActive(true);
    }

    public void EnterStage()
    {
        GameManager.Instance.GameStart(GameManager.GameMode.SingleGame, stageIdx);
    }
    #endregion

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
        // ä���� �� �� ���
        // ������ ä�� ���� ���� �ٷ� ����
        if(curChatIdx == chats.Length - 1)
            RemoveOneChat();
        else
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
            yield return WfsManager.Instance.GetWaitForSeconds(1.0f);
            curChatTime++;

            if(curChatTime >= chatRefreshInterval)
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