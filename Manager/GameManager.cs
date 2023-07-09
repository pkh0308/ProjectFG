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

    // 시간 관련
    [SerializeField] float goalInInterval;
    [SerializeField] float timeOutInterval;
    int curTime;

    // 멀티플레이 관련
    int stageReadyCount;
    int curSurvivors;
    bool isOver;

    // 일시정지
    bool isPaused;
    public bool IsPaused { get { return isPaused; } }

    // 코루틴 관리용 변수
    Coroutine timerRoutine;

    // 승패 확인(결과 화면용)
    bool isWinner;
    public bool IsWinner { get { return isWinner; } }

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

    // 회전각 전달용
    float camAngle;
    public float CamAngle { get { return camAngle; } }

    public void SetCamAngle(float angle)
    {
        camAngle = angle;
    }

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
        isPaused = true;
        SceneController.Instance.EnterStage(stageIdx);
    }

    public void StartMultiGame()
    {
        curMode = GameMode.MultiGame;
        isPaused = true;
        curSurvivors = NetworkManager.Instance.CurUsers;
        
        SceneController.Instance.EnterRandomStage();
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
        isWinner = true;
        StopCoroutine(timerRoutine);

        StageSoundController.PlayBgm(StageSoundController.StageBgm.StopBgm);
        StageSoundController.PlaySfx(StageSoundController.StageSfx.StageClear);

        // goalInInterval만큼 결과 UI 노출 후 해제
        UiController.Instance.ResultOn();
        yield return WfsManager.Instance.GetWaitForSeconds(goalInInterval);
        UiController.Instance.ResultOff();

        // 결과 화면으로 이동
        SceneController.Instance.ExitStage();
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
        this.isWinner = isWinner;
        StopCoroutine(timerRoutine);

        StageSoundController.PlayBgm(StageSoundController.StageBgm.StopBgm);
        StageSoundController.PlaySfx(StageSoundController.StageSfx.StageClear);

        // goalInInterval만큼 결과 UI 노출 후 해제
        UiController.Instance.ResultOn(isWinner);
        yield return WfsManager.Instance.GetWaitForSeconds(goalInInterval);
        UiController.Instance.ResultOff();

        // 결과 화면으로 이동
        SceneController.Instance.ExitStage();
    }

    // 결과 화면 퇴장 시 호출
    // 승자 여부, 일시정지 여부 등 초기화
    public void Reinitialize()
    {
        isWinner = false;
        isPaused = false;
        stageReadyCount = 0;
        curSurvivors = 0;
    }
    #endregion

    #region 아웃 처리
    public void PlayerOut()
    {
        isOver = true;
        UiController.Instance.ResultOn(false);
        UiController.Instance.MinusUserCount();

        PV.RPC(nameof(Survive_Minus), RpcTarget.All);
    }

    [PunRPC]
    void Survive_Minus()
    {
        curSurvivors--;
        if(curSurvivors <= 1)
            StartCoroutine(Survive_MultiGame(!isOver)); // 게임오버 여부 반전해서 전달
    }

    IEnumerator Survive_MultiGame(bool isWinner)
    {
        // 일시정지
        isPaused = true;
        this.isWinner = isWinner;

        StageSoundController.PlayBgm(StageSoundController.StageBgm.StopBgm);
        StageSoundController.PlaySfx(StageSoundController.StageSfx.StageClear);

        // goalInInterval만큼 결과 UI 노출 후 해제
        UiController.Instance.ResultOn(isWinner);
        yield return WfsManager.Instance.GetWaitForSeconds(goalInInterval);
        UiController.Instance.ResultOff();

        // 로비 이동 및 룸 나가기
        SceneController.Instance.ExitStage();
    }
    #endregion

    #region 중도 퇴장
    public void PlayerExit()
    {
        // 멀티 시 처리
        // 나머지 플레이어들에게 생존자 -1 카운트
        if(curMode == GameMode.MultiGame)
            PV.RPC(nameof(Survive_Minus), RpcTarget.Others);
        
        // 로비 이동
        SceneController.Instance.ExitStage();
        if(curMode == GameMode.SingleGame)
            curMode = GameMode.Lobby;

        // 일시정지 및 변수 초기화
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
        isWinner = false;

        StageSoundController.PlayBgm(StageSoundController.StageBgm.StopBgm);
        StageSoundController.PlaySfx(StageSoundController.StageSfx.GameOver);

        // goalInInterval만큼 결과 UI 노출 후 해제
        UiController.Instance.ActiveTimeOutSet(true);
        yield return WfsManager.Instance.GetWaitForSeconds(timeOutInterval);
        UiController.Instance.ActiveTimeOutSet(false);

        // 결과 화면 이동
        SceneController.Instance.ExitStage();
        if (IsSingleGame)
            curMode = GameMode.Lobby;
    }
    #endregion
}