using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// System.Random과 혼선 방지용
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
        STAGE_RUN_02,
        STAGE_RUN_03,
        STAGE_SURVIVE_01
    }

    // Build Index 상의 첫번째 스테이지와 마지막 스테이지
    // 스테이지 씬 추가 혹은 순서 변경 시 수정 요망
    int firstStageIdx = Convert.ToInt32(SceneIndex.STAGE_RUN_01);
    int lastStageIdx = Convert.ToInt32(SceneIndex.STAGE_SURVIVE_01);
    public int FirstStageIdx { get { return firstStageIdx; } }
    public int LastStageIdx { get { return lastStageIdx; } }

    void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        SceneManager.LoadSceneAsync(Convert.ToInt32(SceneIndex.TITLE), LoadSceneMode.Additive);
    }

    // 로비 입장 버튼
    public void EnterLobby()
    {
        SceneManager.UnloadSceneAsync(Convert.ToInt32(SceneIndex.TITLE));
        // 서버 연결 요청
        NetworkManager.Instance.ConnectToServer();
    }

    // 액티브 씬 설정용 함수
    // Action 변수를 통해 대리자로 사용
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

    //스테이지 입장 시
    public void EnterStage(int stageIdx)
    {
        curStageIdx = stageIdx;
        loadingScreen.SetActive(true);
        SceneManager.UnloadSceneAsync(Convert.ToInt32(SceneIndex.LOBBY));
        StartCoroutine(LoadingStage());
    }
    // 랜덤 스테이지 입장용
    public int GetRandomStageIdx()
    {
        return Random.Range(firstStageIdx, lastStageIdx + 1);
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
        //로비 씬 로드 대기
        AsyncOperation op = SceneManager.LoadSceneAsync(Convert.ToInt32(SceneIndex.LOBBY), LoadSceneMode.Additive);
        while (!op.isDone)
        {
            yield return WfsManager.Instance.GetWaitForSeconds(minInterval);
        }
        loadingScreen.SetActive(false);
        loadingCamera.SetActive(false);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(Convert.ToInt32(SceneIndex.LOBBY)));
    }

    // 입장하는 스테이지를 LoadSceneAsync로 로딩하며 AsyncOperation 변수에 저장
    // 로딩이 끝날때까지 대기하다가 끝나면 로딩 스크린 해제
    IEnumerator LoadingStage()
    {
        //플레이어 씬 로드 대기
        AsyncOperation op = SceneManager.LoadSceneAsync(Convert.ToInt32(SceneIndex.PLAYER), LoadSceneMode.Additive);
        while (!op.isDone)
        {
            yield return WfsManager.Instance.GetWaitForSeconds(minInterval);
        }

        //스테이지 씬 로드 대기
        op = SceneManager.LoadSceneAsync(curStageIdx, LoadSceneMode.Additive);
        while (!op.isDone)
        {
            yield return WfsManager.Instance.GetWaitForSeconds(minInterval);
        }
        
        loadingScreen.SetActive(false);
    }
}