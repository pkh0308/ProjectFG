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

    // �ð� ����
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

    // ���� Ȯ��(��� ȭ���)
    bool isWinner;
    public bool IsWinner { get { return isWinner; } }

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

    // ȸ���� ���޿�
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
        isWinner = true;
        StopCoroutine(timerRoutine);

        StageSoundController.PlayBgm(StageSoundController.StageBgm.StopBgm);
        StageSoundController.PlaySfx(StageSoundController.StageSfx.StageClear);

        // goalInInterval��ŭ ��� UI ���� �� ����
        UiController.Instance.ResultOn();
        yield return WfsManager.Instance.GetWaitForSeconds(goalInInterval);
        UiController.Instance.ResultOff();

        // ��� ȭ������ �̵�
        SceneController.Instance.ExitStage();
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
        this.isWinner = isWinner;
        StopCoroutine(timerRoutine);

        StageSoundController.PlayBgm(StageSoundController.StageBgm.StopBgm);
        StageSoundController.PlaySfx(StageSoundController.StageSfx.StageClear);

        // goalInInterval��ŭ ��� UI ���� �� ����
        UiController.Instance.ResultOn(isWinner);
        yield return WfsManager.Instance.GetWaitForSeconds(goalInInterval);
        UiController.Instance.ResultOff();

        // ��� ȭ������ �̵�
        SceneController.Instance.ExitStage();
    }

    // ��� ȭ�� ���� �� ȣ��
    // ���� ����, �Ͻ����� ���� �� �ʱ�ȭ
    public void Reinitialize()
    {
        isWinner = false;
        isPaused = false;
        stageReadyCount = 0;
        curSurvivors = 0;
    }
    #endregion

    #region �ƿ� ó��
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
            StartCoroutine(Survive_MultiGame(!isOver)); // ���ӿ��� ���� �����ؼ� ����
    }

    IEnumerator Survive_MultiGame(bool isWinner)
    {
        // �Ͻ�����
        isPaused = true;
        this.isWinner = isWinner;

        StageSoundController.PlayBgm(StageSoundController.StageBgm.StopBgm);
        StageSoundController.PlaySfx(StageSoundController.StageSfx.StageClear);

        // goalInInterval��ŭ ��� UI ���� �� ����
        UiController.Instance.ResultOn(isWinner);
        yield return WfsManager.Instance.GetWaitForSeconds(goalInInterval);
        UiController.Instance.ResultOff();

        // �κ� �̵� �� �� ������
        SceneController.Instance.ExitStage();
    }
    #endregion

    #region �ߵ� ����
    public void PlayerExit()
    {
        // ��Ƽ �� ó��
        // ������ �÷��̾�鿡�� ������ -1 ī��Ʈ
        if(curMode == GameMode.MultiGame)
            PV.RPC(nameof(Survive_Minus), RpcTarget.Others);
        
        // �κ� �̵�
        SceneController.Instance.ExitStage();
        if(curMode == GameMode.SingleGame)
            curMode = GameMode.Lobby;

        // �Ͻ����� �� ���� �ʱ�ȭ
        isPaused = false;
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
        isWinner = false;

        StageSoundController.PlayBgm(StageSoundController.StageBgm.StopBgm);
        StageSoundController.PlaySfx(StageSoundController.StageSfx.GameOver);

        // goalInInterval��ŭ ��� UI ���� �� ����
        UiController.Instance.ActiveTimeOutSet(true);
        yield return WfsManager.Instance.GetWaitForSeconds(timeOutInterval);
        UiController.Instance.ActiveTimeOutSet(false);

        // ��� ȭ�� �̵�
        SceneController.Instance.ExitStage();
        if (IsSingleGame)
            curMode = GameMode.Lobby;
    }
    #endregion
}