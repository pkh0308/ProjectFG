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
            LoadLobbyScene();
    }
    public void SetStageIdx(int stageIdx)
    {
        curStageIdx = stageIdx;
    }

    //스테이지 입장 시
    public void EnterStage(int stageIdx)
    {
        curStageIdx = stageIdx;
        loadingScreen.SetActive(true);
        SceneManager.UnloadSceneAsync(Convert.ToInt32(SceneIndex.Lobby));
        
        // 싱글 게임
        if(!NetworkManager.Instance.InRoom)
            LoadPlayerScene();
        // 멀티 게임(마스터 클라이언트만 호출)
        if (PN.IsMasterClient)
            StartCoroutine(LoadingStage_Network());
    }
    // 랜덤 스테이지 입장
    // stageIdx는 SetStageIdx로 미리 설정(RPC로 호출)
    public void EnterRandomStage()
    {
        // LoadLevel()을 위해 true로 설정
        PN.AutomaticallySyncScene = true;

        loadingScreen.SetActive(true);
        SceneManager.UnloadSceneAsync(Convert.ToInt32(SceneIndex.Lobby));

        // 싱글 게임
        if (!NetworkManager.Instance.InRoom)
            LoadPlayerScene();
        // 멀티 게임(마스터 클라이언트만 호출)
        if (PN.IsMasterClient)
            StartCoroutine(LoadingStage_Network());
    }
    // 랜덤 스테이지 입장용
    public int GetRandomStageIdx()
    {
        return Random.Range(firstStageIdx, lastStageIdx);
    }
    // 스테이지 종료
    // 플레이어 및 스테이지 씬 언로드, 결과 씬 로드
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

    // 로비 씬 로드(비동기)
    // 완료 후 로딩 씬 카메라 비활성화
    void LoadLobbyScene()
    {
        //로비 씬 로드 대기
        AsyncOperation op = SceneManager.LoadSceneAsync(Convert.ToInt32(SceneIndex.Lobby), LoadSceneMode.Additive);
        op.completed += (operation) =>
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(Convert.ToInt32(SceneIndex.Lobby)));
            loadingCamera.SetActive(false);
            loadingScreen.SetActive(false);
        };
    }

    // 플레이어 씬 로드(비동기)
    // 완료 후 스테이지 씬 로드(LoadStageScene())
    void LoadPlayerScene()
    {
        //플레이어 씬 로드 대기
        AsyncOperation op = SceneManager.LoadSceneAsync(Convert.ToInt32(SceneIndex.Player), LoadSceneMode.Additive);
        op.completed += (operation) => { LoadStageScene(); };
    }

    // 스테이지 씬 로드(비동기)
    // 완료 후 로딩 스크린 해제
    void LoadStageScene()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(curStageIdx, LoadSceneMode.Additive);
        op.completed += (operation) => { loadingScreen.SetActive(false); };
    }

    // 플레이어 씬 로드(비동기)
    // 완료 후 스테이지 씬 로드(LoadStageScene())
    void LoadPlayerScene_Network()
    {
        AsyncOperation op = PN.AsyncLoadLevel(Convert.ToInt32(SceneIndex.Player));
        op.completed += (operation) => { LoadStageScene_Network(); };
    }

    // 스테이지 씬 로드(비동기)
    // 완료 후 로딩 스크린 해제
    void LoadStageScene_Network()
    {
        AsyncOperation op = PN.AsyncLoadLevel(curStageIdx);
    }

    // 멀티 게임용 로딩 코루틴
    // AsyncOperation을 반환하도록 새로 작성한 AsyncLoadLevel() 사용
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

    void LoadingScene(int sceneIdx)
    {
        // 씬 비동기 로드
        AsyncOperation op = SceneManager.LoadSceneAsync((sceneIdx), LoadSceneMode.Additive);
        op.completed += (operation) => { OnCompleted(sceneIdx); };
    }

    void OnCompleted(int sceneIdx)
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(sceneIdx));
        loadingScreen.SetActive(false);
    }

    // StageController에서 호출(Start())
    // 로딩 스크린 해제 및 씬 동기화 해제
    public void LoadingCompleted()
    {
        loadingScreen.SetActive(false);
        PN.AutomaticallySyncScene = false;
    }
}