using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// System.Random과 혼선 방지용
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

    // Build Index 상의 첫번째 스테이지와 마지막 스테이지
    // 스테이지 씬 추가 혹은 순서 변경 시 수정 요망
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

    // 로비 입장 버튼
    public void EnterLobby()
    {
        SceneManager.UnloadSceneAsync(Convert.ToInt32(SceneIndex.Title));
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
            != SceneManager.GetSceneByBuildIndex(Convert.ToInt32(SceneIndex.Lobby)))
            StartCoroutine(LoadingLobby());
    }

    //스테이지 입장 시
    public void EnterStage(int stageIdx)
    {
        curStageIdx = stageIdx;
        loadingScreen.SetActive(true);
        SceneManager.UnloadSceneAsync(Convert.ToInt32(SceneIndex.Lobby));
        
        // 싱글 게임
        if(!NetworkManager.Instance.InRoom)
            StartCoroutine(LoadingStage());
        // 멀티 게임(마스터 클라이언트만 호출)
        if (PN.IsMasterClient)
            StartCoroutine(LoadingStage_Network());
    }
    public void EnterRandomStage()
    {
        curStageIdx = GetRandomStageIdx();
        loadingScreen.SetActive(true);
        SceneManager.UnloadSceneAsync(Convert.ToInt32(SceneIndex.Lobby));

        // 싱글 게임
        if (!NetworkManager.Instance.InRoom)
            StartCoroutine(LoadingStage());
        // 멀티 게임(마스터 클라이언트만 호출)
        if (PN.IsMasterClient)
            StartCoroutine(LoadingStage_Network());
    }
    // 랜덤 스테이지 입장용
    int GetRandomStageIdx()
    {
        return Random.Range(firstStageIdx, lastStageIdx + 1);
    }
    // 스테이지 종료
    // 플레이어 및 스테이지 씬 언로드, 결과 씬 로드
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
        //로비 씬 로드 대기
        AsyncOperation op = SceneManager.LoadSceneAsync(Convert.ToInt32(SceneIndex.Lobby), LoadSceneMode.Additive);
        while (!op.isDone)
        {
            yield return WfsManager.Instance.GetWaitForSeconds(minInterval);
        }
        loadingScreen.SetActive(false);
        loadingCamera.SetActive(false);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(Convert.ToInt32(SceneIndex.Lobby)));
    }

    // 입장하는 스테이지를 LoadSceneAsync로 로딩하며 AsyncOperation 변수에 저장
    // 로딩이 끝날때까지 대기하다가 끝나면 로딩 스크린 해제
    IEnumerator LoadingStage()
    {
        //플레이어 씬 로드 대기
        AsyncOperation op = SceneManager.LoadSceneAsync(Convert.ToInt32(SceneIndex.Player), LoadSceneMode.Additive);
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

    IEnumerator LoadingStage_Network()
    {
        //플레이어 씬 로드 대기
        AsyncOperation op = PN.AsyncLoadLevel(Convert.ToInt32(SceneIndex.Player));
        while (!op.isDone)
        {
            yield return WfsManager.Instance.GetWaitForSeconds(minInterval);
        }
        //스테이지 씬 로드 대기
        op = PN.AsyncLoadLevel(curStageIdx);
        while (!op.isDone)
        {
            yield return WfsManager.Instance.GetWaitForSeconds(minInterval);
        }
    }

    IEnumerator LoadingScene(int sceneIdx)
    {
        // 씬 로드 대기
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