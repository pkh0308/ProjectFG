using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// System.Random�� ȥ�� ������
using Random = UnityEngine.Random;

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
        LOADING = 0,
        TITLE,
        LOBBY,
        PLAYER,
        STAGE_RUN_01,
        STAGE_SURVIVE_01
    }

    void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        SceneManager.LoadSceneAsync(Convert.ToInt32(SceneIndex.TITLE), LoadSceneMode.Additive);
    }

    // �κ� ���� ��ư
    public void EnterLobby()
    {
        SceneManager.UnloadSceneAsync(Convert.ToInt32(SceneIndex.TITLE));
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
            != SceneManager.GetSceneByBuildIndex(Convert.ToInt32(SceneIndex.LOBBY)))
            StartCoroutine(LoadingLobby());
    }

    //�������� ���� ��
    public void EnterStage(int stageIdx)
    {
        curStageIdx = stageIdx;
        loadingScreen.SetActive(true);
        SceneManager.UnloadSceneAsync(Convert.ToInt32(SceneIndex.LOBBY));
        StartCoroutine(LoadingStage());
    }
    // ���� �������� �����
    public int GetRandomStageIdx()
    {
        return Random.Range(Convert.ToInt32(SceneIndex.STAGE_RUN_01),
                            Convert.ToInt32(SceneIndex.STAGE_SURVIVE_01) + 1);
    }

    public void ExitStage()
    {
        loadingScreen.SetActive(true);
        SceneManager.UnloadSceneAsync(curStageIdx);
        SceneManager.UnloadSceneAsync(Convert.ToInt32(SceneIndex.PLAYER));
        SceneManager.LoadScene(Convert.ToInt32(SceneIndex.LOBBY), LoadSceneMode.Additive);
        loadingScreen.SetActive(false);
    }

    IEnumerator LoadingLobby()
    {
        //�κ� �� �ε� ���
        AsyncOperation op = SceneManager.LoadSceneAsync(Convert.ToInt32(SceneIndex.LOBBY), LoadSceneMode.Additive);
        while (!op.isDone)
        {
            yield return WfsManager.Instance.GetWaitForSeconds(minInterval);
        }
        loadingScreen.SetActive(false);
        loadingCamera.SetActive(false);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(Convert.ToInt32(SceneIndex.LOBBY)));
    }

    //�����ϴ� ���������� LoadSceneAsync�� �ε��ϸ� AsyncOperation ������ ����
    //�ε� �۾��� �Ϸ�ɶ����� ����� �� �ش� ���������� ��Ƽ�� ������ ����, ���� ������Ʈ ����
    IEnumerator LoadingStage()
    {
        //�÷��̾� �� �ε� ���
        AsyncOperation op = SceneManager.LoadSceneAsync(Convert.ToInt32(SceneIndex.PLAYER), LoadSceneMode.Additive);
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
}