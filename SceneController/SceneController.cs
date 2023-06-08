using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [Header("Loading")]
    [SerializeField] GameObject loadingScreen;
    [SerializeField] GameObject loadingCamera;

    public static SceneController Instance { get; private set; }

    // �ܺ� ȣ��� ������
    public static Action SetActiveSceneToCurStage;

    int curStageIdx;
    public int CurStageIdx { get { return curStageIdx; } }
    float minInterval = 0.5f;

    public enum SceneIndex
    {
        LOADING = 0,
        TITLE,
        LOBBY,
        PLAYER,
        STAGE_01
    }

    void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
        SetActiveSceneToCurStage = SetActiveScene;
    }

    void Start()
    {
        SceneManager.LoadSceneAsync((int)SceneIndex.TITLE, LoadSceneMode.Additive);
    }

    // �κ� ���� ��ư
    public void EnterLobby()
    {
        SceneManager.UnloadSceneAsync((int)SceneIndex.TITLE);
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
        if (SceneManager.GetActiveScene() != SceneManager.GetSceneByBuildIndex((int)SceneIndex.LOBBY))
            StartCoroutine(LoadingLobby());
    }

    //�������� ���� ��
    public void EnterStage(int stageIdx)
    {
        curStageIdx = stageIdx + 2;
        loadingScreen.SetActive(true);
        SceneManager.UnloadSceneAsync((int)SceneIndex.LOBBY);
        StartCoroutine(LoadingStage());
    }

    public void ExitStage()
    {
        loadingScreen.SetActive(true);
        SceneManager.UnloadSceneAsync((int)SceneIndex.STAGE_01); // ���� curStageIdx �̿��ϵ��� ������ ��
        SceneManager.UnloadSceneAsync((int)SceneIndex.PLAYER);
        SceneManager.LoadScene((int)SceneIndex.LOBBY, LoadSceneMode.Additive);
        loadingScreen.SetActive(false);
    }

    IEnumerator LoadingLobby()
    {
        //�κ� �� �ε� ���
        AsyncOperation op = SceneManager.LoadSceneAsync((int)SceneIndex.LOBBY, LoadSceneMode.Additive);
        while (!op.isDone)
        {
            yield return WfsManager.Instance.GetWaitForSeconds(minInterval);
        }
        loadingScreen.SetActive(false);
        loadingCamera.SetActive(false);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex((int)SceneIndex.LOBBY));
    }

    //�����ϴ� ���������� LoadSceneAsync�� �ε��ϸ� AsyncOperation ������ ����
    //�ε� �۾��� �Ϸ�ɶ����� ����� �� �ش� ���������� ��Ƽ�� ������ ����, ���� ������Ʈ ����
    IEnumerator LoadingStage()
    {
        //�������� �� �ε� ���
        // ���� curStageIdx �̿��ϵ��� ������ ��
        AsyncOperation op = SceneManager.LoadSceneAsync((int)SceneIndex.STAGE_01, LoadSceneMode.Additive);
        curStageIdx = (int)SceneIndex.STAGE_01;
        while (!op.isDone)
        {
            yield return WfsManager.Instance.GetWaitForSeconds(minInterval);
        }
        //�÷��̾� �� �ε� ���
        op = SceneManager.LoadSceneAsync((int)SceneIndex.PLAYER, LoadSceneMode.Additive);
        while (!op.isDone)
        {
            yield return WfsManager.Instance.GetWaitForSeconds(minInterval);
        }
        loadingScreen.SetActive(false);
    }
}