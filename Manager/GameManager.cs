using Photon.Pun;
using System.Collections;
using UnityEngine;

// ���� ������ �Ѱ��ϴ� �Ŵ��� Ŭ����
// ������Ʈ�� �ε� ���� ��ġ
public class GameManager : MonoBehaviour
{
    // ���� ����
    PhotonView PV;

    #region �̱��� ����
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

    // ��Ƽ�÷��� ����
    int stageReadyCount;
    int curSurvivors;
    bool isOver;

    // �Ͻ�����
    bool isPaused;
    public bool IsPaused { get { return isPaused; } }

    // �ڷ�ƾ ������ ����
    Coroutine timerRoutine;

    // ���� ���� ����
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

    #region ���� ����
    public void GameStart(GameMode mode, int stageIdx = 0)
    {
        switch (mode)
        {
            case GameMode.SingleGame:
                StartSingleGame(stageIdx);
                break;
            case GameMode.MultiGame:
                // �� ����
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
        isPaused = true;
        curSurvivors = NetworkManager.Instance.CurUsers;
        // ������ Ŭ���̾�Ʈ���� ����
        // ���� �������� �ε����� ����(RPC)
        if (NetworkManager.Instance.IsMaster)
        {
            int idx = SceneController.Instance.GetRandomStageIdx();
            PV.RPC(nameof(EnterRandomStage), RpcTarget.All, idx);
        }
    }

    [PunRPC]
    void EnterRandomStage(int stageIdx)
    {
        //SceneController.Instance.EnterStage(stageIdx);
        SceneController.Instance.EnterStage(6);
    }

    public void PlayerReady()
    {
        stageReadyCount++;
        if(stageReadyCount == NetworkManager.Instance.CurUsers)
            UiController.Instance.StartCountDown();
    }
    #endregion

    #region �Ͻ�����
    public void PauseOn()
    {
        isPaused = true;
    }
    public void PauseOff()
    {
        isPaused = false;
    }
    #endregion

    #region ���� �� ó��
    public void Goal()
    {
        switch (curMode) 
        {
            case GameMode.SingleGame:
                 StartCoroutine(Goal_SingleGame());
                 break;
            // ���ڴ� Goal_MultiGame ���� ȣ��(isWinner = true)
            // ������ �ο�(����)���� RPC�� ȣ��(isWinner = false)
            case GameMode.MultiGame:
                 StartCoroutine(Goal_MultiGame(true));
                 PV.RPC(nameof(Goal_Others), RpcTarget.Others);
                 break;
        }
    }

    IEnumerator Goal_SingleGame()
    {
        // �Ͻ�����
        isPaused = true;
        StopCoroutine(timerRoutine);

        StageSoundController.PlayBgm((int)StageSoundController.StageBgm.stopBgm);
        StageSoundController.PlaySfx((int)StageSoundController.StageSfx.stageClear);

        // goalInInterval��ŭ ��� UI ���� �� ����
        UiController.Instance.ResultOn();
        yield return WfsManager.Instance.GetWaitForSeconds(goalInInterval);
        UiController.Instance.ResultOff();

        // �κ� �̵�
        SceneController.Instance.ExitStage();
        curMode = GameMode.Lobby;

        // �Ͻ����� �� ���� �ʱ�ȭ
        isPaused = false;
        stageReadyCount = 0;
    }

    [PunRPC]
    void Goal_Others()
    {
        StartCoroutine(Goal_MultiGame(false));
    }

    IEnumerator Goal_MultiGame(bool isWinner)
    {
        // �Ͻ�����
        isPaused = true;
        StopCoroutine(timerRoutine);

        StageSoundController.PlayBgm((int)StageSoundController.StageBgm.stopBgm);
        StageSoundController.PlaySfx((int)StageSoundController.StageSfx.stageClear);

        // goalInInterval��ŭ ��� UI ���� �� ����
        UiController.Instance.ResultOn(isWinner);
        yield return WfsManager.Instance.GetWaitForSeconds(goalInInterval);
        UiController.Instance.ResultOff();

        // �κ� �̵� �� �� ������
        SceneController.Instance.ExitStage();
        NetworkManager.Instance.LeaveRoom();

        // �Ͻ����� �� ���� �ʱ�ȭ
        isPaused = false;
        stageReadyCount = 0;
    }
    #endregion

    #region �ƿ� ó��
    public void PlayerOut()
    {
        isOver = true;
        UiController.Instance.ResultOn(false);
        UiController.Instance.MinusUserCount();

        PV.RPC(nameof(Survive_Count), RpcTarget.All);
    }

    [PunRPC]
    void Survive_Count()
    {
        curSurvivors--;
        if(curSurvivors <= 1)
            StartCoroutine(Survive_MultiGame(!isOver)); // ���ӿ��� ���� �����ؼ� ����
    }

    IEnumerator Survive_MultiGame(bool isWinner)
    {
        // �Ͻ�����
        isPaused = true;

        StageSoundController.PlayBgm((int)StageSoundController.StageBgm.stopBgm);
        StageSoundController.PlaySfx((int)StageSoundController.StageSfx.stageClear);

        // goalInInterval��ŭ ��� UI ���� �� ����
        UiController.Instance.ResultOn(isWinner);
        yield return WfsManager.Instance.GetWaitForSeconds(goalInInterval);
        UiController.Instance.ResultOff();

        // �κ� �̵� �� �� ������
        SceneController.Instance.ExitStage();
        NetworkManager.Instance.LeaveRoom();

        // �Ͻ����� �� ���� �ʱ�ȭ
        isPaused = false;
        curSurvivors = 0;
        stageReadyCount = 0;
        isOver = false;
    }
    #endregion

    #region Ÿ�̸� �� �ð� �ʰ�
    public void StartTimer(int timeLimit)
    {
        timerRoutine = StartCoroutine(Timer(timeLimit));
    }

    // Ÿ�̸� �ڷ�ƾ
    IEnumerator Timer(int timeLimit)
    {
        curTime = timeLimit;

        while(curTime >= 0)
        {
            UiController.Instance.UpdateTimer(curTime);
            yield return WfsManager.Instance.GetWaitForSeconds(1.0f);
            curTime--;
        }

        // �ð� �ʰ� �� Ÿ�Ӿƿ� ���� ȣ��
        StartCoroutine(TimeOut());
    }

    IEnumerator TimeOut()
    {
        // �Ͻ�����
        isPaused = true;

        StageSoundController.PlayBgm((int)StageSoundController.StageBgm.stopBgm);
        StageSoundController.PlaySfx((int)StageSoundController.StageSfx.gameOver);

        // goalInInterval��ŭ ��� UI ���� �� ����
        UiController.Instance.ActiveTimeOutSet(true);
        yield return WfsManager.Instance.GetWaitForSeconds(timeOutInterval);
        UiController.Instance.ActiveTimeOutSet(false);

        // �κ� �̵� �� �� ������(��Ƽ�÷���)
        curMode = GameMode.Lobby;
        SceneController.Instance.ExitStage();
        if(curMode == GameMode.MultiGame)
            NetworkManager.Instance.LeaveRoom();
        

        // �Ͻ����� �ʱ�ȭ
        isPaused = false;
    }
    #endregion
}