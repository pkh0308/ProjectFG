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
            StartCoroutine(LoadingLobby());
    }

    //�������� ���� ��
    public void EnterStage(int stageIdx)
    {
        curStageIdx = stageIdx;
        loadingScreen.SetActive(true);
        SceneManager.UnloadSceneAsync(Convert.ToInt32(SceneIndex.Lobby));
        
        // �̱� ����
        if(!NetworkManager.Instance.InRoom)
            StartCoroutine(LoadingStage());
        // ��Ƽ ����(������ Ŭ���̾�Ʈ�� ȣ��)
        if (PN.IsMasterClient)
            StartCoroutine(LoadingStage_Network());
    }
    public void EnterRandomStage()
    {
        curStageIdx = GetRandomStageIdx();
        loadingScreen.SetActive(true);
        SceneManager.UnloadSceneAsync(Convert.ToInt32(SceneIndex.Lobby));

        // �̱� ����
        if (!NetworkManager.Instance.InRoom)
            StartCoroutine(LoadingStage());
        // ��Ƽ ����(������ Ŭ���̾�Ʈ�� ȣ��)
        if (PN.IsMasterClient)
            StartCoroutine(LoadingStage_Network());
    }
    // ���� �������� �����
    int GetRandomStageIdx()
    {
        return Random.Range(firstStageIdx, lastStageIdx + 1);
    }
    // �������� ����
    // �÷��̾� �� �������� �� ��ε�, ��� �� �ε�
    public void ExitStage()
    {
        loadingScreen.SetActive(true);
        SceneManager.UnloadSceneAsync(curStageIdx);
        SceneManager.UnloadSceneAsync(Convert.ToInt32(SceneIndex.Player));

        StartCoroutine(LoadingScene(Convert.ToInt32(SceneIndex.Result))); 
    }

    public void BackToLobby()
    {
        loadingScreen.SetActive(true);
        SceneManager.UnloadSceneAsync(Convert.ToInt32(SceneIndex.Result));

        StartCoroutine(LoadingScene(Convert.ToInt32(SceneIndex.Lobby)));
    }

    IEnumerator LoadingLobby()
    {
        //�κ� �� �ε� ���
        AsyncOperation op = SceneManager.LoadSceneAsync(Convert.ToInt32(SceneIndex.Lobby), LoadSceneMode.Additive);
        while (!op.isDone)
        {
            yield return WfsManager.Instance.GetWaitForSeconds(minInterval);
        }
        loadingScreen.SetActive(false);
        loadingCamera.SetActive(false);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(Convert.ToInt32(SceneIndex.Lobby)));
    }

    // �����ϴ� ���������� LoadSceneAsync�� �ε��ϸ� AsyncOperation ������ ����
    // �ε��� ���������� ����ϴٰ� ������ �ε� ��ũ�� ����
    IEnumerator LoadingStage()
    {
        //�÷��̾� �� �ε� ���
        AsyncOperation op = SceneManager.LoadSceneAsync(Convert.ToInt32(SceneIndex.Player), LoadSceneMode.Additive);
        while (!op.isDone)
        {
            yield return WfsManager.Instance.GetWaitForSeconds(minInterval);
        }
        //�������� �� �ε� ���
        op = SceneManager.LoadSceneAsync(curStageIdx, LoadSceneMode.Additive);
        while (!op.isDone)
        {
            yield return WfsManager.Instance.GetWaitForSeconds(minInterval);
        }
        
        loadingScreen.SetActive(false);
    }

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

    IEnumerator LoadingScene(int sceneIdx)
    {
        // �� �ε� ���
        AsyncOperation op = SceneManager.LoadSceneAsync((sceneIdx), LoadSceneMode.Additive);
        while (!op.isDone)
        {
            yield return WfsManager.Instance.GetWaitForSeconds(minInterval);
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(sceneIdx));

        loadingScreen.SetActive(false);
    }

    public void LoadingScreenOff()
    {
        loadingScreen.SetActive(false);
    }
}