using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [SerializeField] GameObject loadingScene;
    [SerializeField] GameObject loadingCamera;

    public static SceneController Instance { get; private set; }

    public static Action SetActiveSceneToCurStage;

    int curStageIdx;
    public int CurStageIdx { get { return curStageIdx; } }
    float minInterval = 0.5f;

    public enum SceneIndex
    {
        LOADING = 0,
        LOBBY,
        PLAYER,
        STAGE_01
    }

    void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
        SetActiveSceneToCurStage = SetActiveScene;

        loadingScene.SetActive(true);
        LoadLobby();
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
        loadingScene.SetActive(true);
        SceneManager.UnloadSceneAsync((int)SceneIndex.LOBBY);
        StartCoroutine(Loading());
    }

    public void ExitStage()
    {
        loadingScene.SetActive(true);
        SceneManager.UnloadSceneAsync((int)SceneIndex.STAGE_01); // ���� curStageIdx �̿��ϵ��� ������ ��
        SceneManager.UnloadSceneAsync((int)SceneIndex.PLAYER);
        SceneManager.LoadScene((int)SceneIndex.LOBBY, LoadSceneMode.Additive);
        loadingScene.SetActive(false);
    }

    IEnumerator LoadingLobby()
    {
        //�κ� �� �ε� ���
        AsyncOperation op = SceneManager.LoadSceneAsync((int)SceneIndex.LOBBY, LoadSceneMode.Additive);
        while (!op.isDone)
        {
            yield return WfsManager.Instance.GetWaitForSeconds(minInterval);
        }
        loadingScene.SetActive(false);
        loadingCamera.SetActive(false);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex((int)SceneIndex.LOBBY));
    }

    //�����ϴ� ���������� LoadSceneAsync�� �ε��ϸ� AsyncOperation ������ ����
    //�ε� �۾��� �Ϸ�ɶ����� ����� �� �ش� ���������� ��Ƽ�� ������ ����, ���� ������Ʈ ����
    IEnumerator Loading()
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
        loadingScene.SetActive(false);
    }
}