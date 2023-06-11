using Photon.Pun;
using System.Collections;
using UnityEngine;

// 게임 전반을 총괄하는 매니저 클래스
// 오브젝트는 로딩 씬에 배치
public class GameManager : MonoBehaviour
{
    // 포톤 관련
    PhotonView PV;

    #region 싱글톤 구현
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        PV = GetComponent<PhotonView>();
    }
    #endregion

    [SerializeField] float goalInInterval;
    [SerializeField] float timeOutInterval;
    int curTime;

    // 일시정지
    bool isPaused;
    public bool IsPaused { get { return isPaused; } }

    // 코루틴 관리용 변수
    Coroutine timerRoutine;

    // 현재 게임 상태
    public enum GameMode
    {
        NotConnected,
        Lobby,
        SingleGame,
        MultiGame
    }
    GameMode curMode;
    public GameMode CurMode { get { return curMode; } }
    public bool IsSingleGame { get { return curMode == GameMode.SingleGame; } }

    public void SetMode(GameMode mode)
    {
        curMode = mode;
    }

    #region 게임 시작
    public void GameStart(GameMode mode, int stageIdx = 0)
    {
        switch (mode)
        {
            case GameMode.SingleGame:
                StartSingleGame(stageIdx);
                break;
            case GameMode.MultiGame:
                // 방 입장
                NetworkManager.Instance.EnterRoom();
                break;
        }
    }

    void StartSingleGame(int stageIdx)
    {
        curMode = GameMode.SingleGame;
        SceneController.Instance.EnterStage(stageIdx);
    }

    public void StartMultiGame()
    {
        curMode = GameMode.MultiGame;
        // 마스터 클라이언트에서 실행
        // 랜덤 스테이지 인덱스로 입장(RPC)
        if(NetworkManager.Instance.IsMaster)
        {
            int idx = SceneController.Instance.GetRandomStageIdx();
            PV.RPC(nameof(EnterRandomStage), RpcTarget.All, idx);
        }
    }

    [PunRPC]
    void EnterRandomStage(int stageIdx)
    {
        SceneController.Instance.EnterStage(stageIdx);
    }
    #endregion

    #region 일시정지
    public void PauseOn()
    {
        isPaused = true;
    }
    public void PauseOff()
    {
        isPaused = false;
    }
    #endregion

    #region 골인 시 처리
    public void Goal()
    {
        switch (curMode) 
        {
            case GameMode.SingleGame:
                 StartCoroutine(Goal_SingleGame());
                 break;
            // 승자는 Goal_MultiGame 직접 호출(isWinner = true)
            // 나머지 인원(패자)들은 RPC로 호출(isWinner = false)
            case GameMode.MultiGame:
                 StartCoroutine(Goal_MultiGame(true));
                 PV.RPC(nameof(Goal_Others), RpcTarget.Others); 
                 break;
        }
    }

    IEnumerator Goal_SingleGame()
    {
        // 일시정지
        isPaused = true;
        StopCoroutine(timerRoutine);

        StageSoundController.PlayBgm((int)StageSoundController.StageBgm.stopBgm);
        StageSoundController.PlaySfx((int)StageSoundController.StageSfx.stageClear);

        // goalInInterval만큼 결과 UI 노출 후 해제
        UiController.Instance.ResultOn();
        yield return WfsManager.Instance.GetWaitForSeconds(goalInInterval);
        UiController.Instance.ResultOff();

        // 로비 이동
        SceneController.Instance.ExitStage();
        curMode = GameMode.Lobby;

        // 일시정지 초기화
        isPaused = false;
    }

    [PunRPC]
    void Goal_Others()
    {
        StartCoroutine(Goal_MultiGame(false));
    }

    IEnumerator Goal_MultiGame(bool isWinner)
    {
        // 일시정지
        isPaused = true;
        StopCoroutine(timerRoutine);

        StageSoundController.PlayBgm((int)StageSoundController.StageBgm.stopBgm);
        StageSoundController.PlaySfx((int)StageSoundController.StageSfx.stageClear);

        // goalInInterval만큼 결과 UI 노출 후 해제
        UiController.Instance.ResultOn(isWinner);
        yield return WfsManager.Instance.GetWaitForSeconds(goalInInterval);
        UiController.Instance.ResultOff();

        // 로비 이동 및 룸 나가기
        SceneController.Instance.ExitStage();
        NetworkManager.Instance.LeaveRoom();

        // 일시정지 초기화
        isPaused = false;
    }
    #endregion

    #region 타이머 및 시간 초과
    public void StartTimer(int timeLimit)
    {
        timerRoutine = StartCoroutine(Timer(timeLimit));
    }

    // 타이머 코루틴
    IEnumerator Timer(int timeLimit)
    {
        curTime = timeLimit;

        while(curTime >= 0)
        {
            UiController.Instance.UpdateTimer(curTime);
            yield return WfsManager.Instance.GetWaitForSeconds(1.0f);
            curTime--;
        }

        // 시간 초과 시 타임아웃 로직 호출
        StartCoroutine(TimeOut());
    }

    IEnumerator TimeOut()
    {
        // 일시정지
        isPaused = true;

        StageSoundController.PlayBgm((int)StageSoundController.StageBgm.stopBgm);
        StageSoundController.PlaySfx((int)StageSoundController.StageSfx.gameOver);

        // goalInInterval만큼 결과 UI 노출 후 해제
        UiController.Instance.ActiveTimeOutSet(true);
        yield return WfsManager.Instance.GetWaitForSeconds(timeOutInterval);
        UiController.Instance.ActiveTimeOutSet(false);

        // 로비 이동 및 룸 나가기(멀티플레이)
        curMode = GameMode.Lobby;
        SceneController.Instance.ExitStage();
        if(curMode == GameMode.MultiGame)
            NetworkManager.Instance.LeaveRoom();
        

        // 일시정지 초기화
        isPaused = false;
    }
    #endregion
}