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

    // 외부 호출용 전달자
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

    // 로비 입장 버튼
    public void EnterLobby()
    {
        SceneManager.UnloadSceneAsync((int)SceneIndex.TITLE);
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
        if (SceneManager.GetActiveScene() != SceneManager.GetSceneByBuildIndex((int)SceneIndex.LOBBY))
            StartCoroutine(LoadingLobby());
    }

    //스테이지 입장 시
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
        SceneManager.UnloadSceneAsync((int)SceneIndex.STAGE_01); // 추후 curStageIdx 이용하도록 수정할 것
        SceneManager.UnloadSceneAsync((int)SceneIndex.PLAYER);
        SceneManager.LoadScene((int)SceneIndex.LOBBY, LoadSceneMode.Additive);
        loadingScreen.SetActive(false);
    }

    IEnumerator LoadingLobby()
    {
        //로비 씬 로드 대기
        AsyncOperation op = SceneManager.LoadSceneAsync((int)SceneIndex.LOBBY, LoadSceneMode.Additive);
        while (!op.isDone)
        {
            yield return WfsManager.Instance.GetWaitForSeconds(minInterval);
        }
        loadingScreen.SetActive(false);
        loadingCamera.SetActive(false);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex((int)SceneIndex.LOBBY));
    }

    //입장하는 스테이지를 LoadSceneAsync로 로딩하며 AsyncOperation 변수에 저장
    //로딩 작업이 완료될때까지 대기한 후 해당 스테이지를 액티브 씬으로 설정, 이후 오브젝트 생성
    IEnumerator LoadingStage()
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
        loadingScreen.SetActive(false);
    }
}