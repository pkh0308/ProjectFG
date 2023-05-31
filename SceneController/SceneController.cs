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

    // 액티브 씬 설정용 함수
    // Action 변수를 통해 대리자로 사용
    public void SetActiveScene()
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(curStageIdx));
    }

    public void LoadLobby()
    {
        if (SceneManager.GetActiveScene() != SceneManager.GetSceneByBuildIndex((int)SceneIndex.LOBBY))
            StartCoroutine(LoadingLobby());
    }

    //스테이지 입장 시
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
        SceneManager.UnloadSceneAsync((int)SceneIndex.STAGE_01); // 추후 curStageIdx 이용하도록 수정할 것
        SceneManager.UnloadSceneAsync((int)SceneIndex.PLAYER);
        SceneManager.LoadScene((int)SceneIndex.LOBBY, LoadSceneMode.Additive);
        loadingScene.SetActive(false);
    }

    IEnumerator LoadingLobby()
    {
        //로비 씬 로드 대기
        AsyncOperation op = SceneManager.LoadSceneAsync((int)SceneIndex.LOBBY, LoadSceneMode.Additive);
        while (!op.isDone)
        {
            yield return WfsManager.Instance.GetWaitForSeconds(minInterval);
        }
        loadingScene.SetActive(false);
        loadingCamera.SetActive(false);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex((int)SceneIndex.LOBBY));
    }

    //입장하는 스테이지를 LoadSceneAsync로 로딩하며 AsyncOperation 변수에 저장
    //로딩 작업이 완료될때까지 대기한 후 해당 스테이지를 액티브 씬으로 설정, 이후 오브젝트 생성
    IEnumerator Loading()
    {
        //스테이지 씬 로드 대기
        // 추후 curStageIdx 이용하도록 수정할 것
        AsyncOperation op = SceneManager.LoadSceneAsync((int)SceneIndex.STAGE_01, LoadSceneMode.Additive);
        curStageIdx = (int)SceneIndex.STAGE_01;
        while (!op.isDone)
        {
            yield return WfsManager.Instance.GetWaitForSeconds(minInterval);
        }
        //플레이어 씬 로드 대기
        op = SceneManager.LoadSceneAsync((int)SceneIndex.PLAYER, LoadSceneMode.Additive);
        while (!op.isDone)
        {
            yield return WfsManager.Instance.GetWaitForSeconds(minInterval);
        }
        loadingScene.SetActive(false);
    }
}