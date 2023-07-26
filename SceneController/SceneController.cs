using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// System.Random�� ȥ�� ������
using Random = UnityEngine.Random;
using PN = Photon.Pun.PhotonNetwork;

public class SceneController : MonoBehaviour
{
    [Header("Loading")]
    [SerializeField] GameObject loadingScreen;
    [SerializeField] GameObject loadingCamera;

    public static SceneController Instance { get; private set; }

    int curStageIdx;
    public int CurStageIdx { get { return curStageIdx; } }
    float minInterval = 0.5f;

    public enum SceneIndex
    {
        Loading = 0,
        Title,
        Lobby,
        Player,
        Stage_Run_01,
        Stage_Run_02,
        Stage_Run_03,
        Stage_Survival_01,
        Result
    }

    // Build Index ���� ù��° ���������� ������ ��������
    // �������� �� �߰� Ȥ�� ���� ���� �� ���� ���
    int firstStageIdx = Convert.ToInt32(SceneIndex.Stage_Run_01);
    int lastStageIdx = Convert.ToInt32(SceneIndex.Result);
    public int FirstStageIdx { get { return firstStageIdx; } }
    public int LastStageIdx { get { return lastStageIdx; } }

    void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        SceneManager.LoadSceneAsync(Convert.ToInt32(SceneIndex.Title), LoadSceneMode.Additive);
    }

    // �κ� ���� ��ư
    public void EnterLobby()
    {
        SceneManager.UnloadSceneAsync(Convert.ToInt32(SceneIndex.Title));
        // ���� ���� ��û
        NetworkManager.Instance.ConnectToServer();
    }

    // ��Ƽ�� �� ������ �Լ�
    // Action ������ ���� �븮�ڷ� ���
    public void SetActiveScene()
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(curStageIdx));
    }

    public void LoadLobby()
    {
        if (SceneManager.GetActiveScene()
            != SceneManager.GetSceneByBuildIndex(Convert.ToInt32(SceneIndex.Lobby)))
            LoadLobbyScene();
    }
    public void SetStageIdx(int stageIdx)
    {
        curStageIdx = stageIdx;
    }

    //�������� ���� ��
    public void EnterStage(int stageIdx)
    {
        curStageIdx = stageIdx;
        loadingScreen.SetActive(true);
        SceneManager.UnloadSceneAsync(Convert.ToInt32(SceneIndex.Lobby));
        
        // �̱� ����
        if(!NetworkManager.Instance.InRoom)
            LoadPlayerScene();
        // ��Ƽ ����(������ Ŭ���̾�Ʈ�� ȣ��)
        if (PN.IsMasterClient)
            StartCoroutine(LoadingStage_Network());
    }
    // ���� �������� ����
    // stageIdx�� SetStageIdx�� �̸� ����(RPC�� ȣ��)
    public void EnterRandomStage()
    {
        // LoadLevel()�� ���� true�� ����
        PN.AutomaticallySyncScene = true;

        loadingScreen.SetActive(true);
        SceneManager.UnloadSceneAsync(Convert.ToInt32(SceneIndex.Lobby));

        // �̱� ����
        if (!NetworkManager.Instance.InRoom)
            LoadPlayerScene();
        // ��Ƽ ����(������ Ŭ���̾�Ʈ�� ȣ��)
        if (PN.IsMasterClient)
            StartCoroutine(LoadingStage_Network());
    }
    // ���� �������� �����
    public int GetRandomStageIdx()
    {
        return Random.Range(firstStageIdx, lastStageIdx);
    }
    // �������� ����
    // �÷��̾� �� �������� �� ��ε�, ��� �� �ε�
    public void ExitStage()
    {
        loadingScreen.SetActive(true);
        SceneManager.UnloadSceneAsync(curStageIdx);
        SceneManager.UnloadSceneAsync(Convert.ToInt32(SceneIndex.Player));

        LoadingScene(Convert.ToInt32(SceneIndex.Result)); 
    }

    public void BackToLobby()
    {
        loadingScreen.SetActive(true);
        SceneManager.UnloadSceneAsync(Convert.ToInt32(SceneIndex.Result));

        LoadingScene(Convert.ToInt32(SceneIndex.Lobby));
    }

    // �κ� �� �ε�(�񵿱�)
    // �Ϸ� �� �ε� �� ī�޶� ��Ȱ��ȭ
    void LoadLobbyScene()
    {
        //�κ� �� �ε� ���
        AsyncOperation op = SceneManager.LoadSceneAsync(Convert.ToInt32(SceneIndex.Lobby), LoadSceneMode.Additive);
        op.completed += (operation) =>
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(Convert.ToInt32(SceneIndex.Lobby)));
            loadingCamera.SetActive(false);
            loadingScreen.SetActive(false);
        };
    }

    // �÷��̾� �� �ε�(�񵿱�)
    // �Ϸ� �� �������� �� �ε�(LoadStageScene())
    void LoadPlayerScene()
    {
        //�÷��̾� �� �ε� ���
        AsyncOperation op = SceneManager.LoadSceneAsync(Convert.ToInt32(SceneIndex.Player), LoadSceneMode.Additive);
        op.completed += (operation) => { LoadStageScene(); };
    }

    // �������� �� �ε�(�񵿱�)
    // �Ϸ� �� �ε� ��ũ�� ����
    void LoadStageScene()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(curStageIdx, LoadSceneMode.Additive);
        op.completed += (operation) => { loadingScreen.SetActive(false); };
    }

    // �÷��̾� �� �ε�(�񵿱�)
    // �Ϸ� �� �������� �� �ε�(LoadStageScene())
    void LoadPlayerScene_Network()
    {
        AsyncOperation op = PN.AsyncLoadLevel(Convert.ToInt32(SceneIndex.Player));
        op.completed += (operation) => { LoadStageScene_Network(); };
    }

    // �������� �� �ε�(�񵿱�)
    // �Ϸ� �� �ε� ��ũ�� ����
    void LoadStageScene_Network()
    {
        AsyncOperation op = PN.AsyncLoadLevel(curStageIdx);
    }

    // ��Ƽ ���ӿ� �ε� �ڷ�ƾ
    // AsyncOperation�� ��ȯ�ϵ��� ���� �ۼ��� AsyncLoadLevel() ���
    IEnumerator LoadingStage_Network()
    {
        //�÷��̾� �� �ε� ���
        AsyncOperation op = PN.AsyncLoadLevel(Convert.ToInt32(SceneIndex.Player)); 
        while (!op.isDone)
        {
            yield return WfsManager.Instance.GetWaitForSeconds(minInterval);
        }      

        //�������� �� �ε� ���
        op = PN.AsyncLoadLevel(curStageIdx);
        while (!op.isDone)
        {
            yield return WfsManager.Instance.GetWaitForSeconds(minInterval);
        }
    }

    void LoadingScene(int sceneIdx)
    {
        // �� �񵿱� �ε�
        AsyncOperation op = SceneManager.LoadSceneAsync((sceneIdx), LoadSceneMode.Additive);
        op.completed += (operation) => { OnCompleted(sceneIdx); };
    }

    void OnCompleted(int sceneIdx)
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(sceneIdx));
        loadingScreen.SetActive(false);
    }

    // StageController���� ȣ��(Start())
    // �ε� ��ũ�� ���� �� �� ����ȭ ����
    public void LoadingCompleted()
    {
        loadingScreen.SetActive(false);
        PN.AutomaticallySyncScene = false;
    }
}