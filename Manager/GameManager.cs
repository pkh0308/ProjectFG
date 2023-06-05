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
    [SerializeField] int timeLimit;
    int curTime;

    // �Ͻ�����
    bool isPaused;
    public bool IsPaused { get { return isPaused; } }

    // �ڷ�ƾ ������ ����
    Coroutine timerRoutine;

    // ���� ���� ����
    public enum GameMode
    {
        Lobby,
        SingleGame,
        MultiGame
    }
    GameMode curMode;
    public bool IsSingleGame { get { return curMode == GameMode.SingleGame; } }

    #region ���� ����
    public void GameStart(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.SingleGame:
                StartSingleGame();
                break;
            case GameMode.MultiGame:
                WaitForMultiGame();
                break;
        }
    }

    void StartSingleGame()
    {
        curMode = GameMode.SingleGame;
        SceneController.Instance.EnterStage(0);
    }

    void WaitForMultiGame()
    {
        NetworkManager.Instance.EnterRoom();
    }

    public void StartMultiGame()
    {
        curMode = GameMode.MultiGame;
        SceneController.Instance.EnterStage(0);
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
        // �Ͻ�����
        isPaused = true;

        switch (curMode) 
        {
            case GameMode.SingleGame:
                StopCoroutine(timerRoutine);
                StartCoroutine(Goal_SingleGame());
                break;
            // ���ڴ� Goal_MultiGame ���� ȣ��(isWinner = true)
            // ������ �ο�(����)���� RPC�� ȣ��(isWinner = false)
            case GameMode.MultiGame:
                StartCoroutine(Goal_MultiGame(true));
                PV.RPC(nameof(Goal_M), RpcTarget.Others);
                break;
        }
    }

    IEnumerator Goal_SingleGame()
    {
        StageSoundController.PlayBgm((int)StageSoundController.StageBgm.stopBgm);
        StageSoundController.PlaySfx((int)StageSoundController.StageSfx.stageClear);

        // goalInInterval��ŭ ��� UI ���� �� ����
        UiController.Instance.ResultOn(true);
        yield return WfsManager.Instance.GetWaitForSeconds(goalInInterval);
        UiController.Instance.ResultOff();

        // �κ� �̵�
        SceneController.Instance.ExitStage();
        curMode = GameMode.Lobby;

        // �Ͻ����� �ʱ�ȭ
        isPaused = false;
    }

    // ������ ��� �ڷ�ƾ ȣ���
    [PunRPC]
    void Goal_M()
    {
        StartCoroutine(Goal_MultiGame(false));
    }

    IEnumerator Goal_MultiGame(bool isWinner)
    {
        StageSoundController.PlayBgm((int)StageSoundController.StageBgm.stopBgm);
        StageSoundController.PlaySfx((int)StageSoundController.StageSfx.stageClear);

        // goalInInterval��ŭ ��� UI ���� �� ����
        UiController.Instance.ResultOn(isWinner);
        yield return WfsManager.Instance.GetWaitForSeconds(goalInInterval);
        UiController.Instance.ResultOff();

        // �κ� �̵� �� �� ������
        SceneController.Instance.ExitStage();
        NetworkManager.Instance.LeaveRoom();
        curMode = GameMode.Lobby;

        // �Ͻ����� �ʱ�ȭ
        isPaused = false;
    }
    #endregion

    #region Ÿ�̸� �� �ð� �ʰ�
    public void StartTimer()
    {
        timerRoutine = StartCoroutine(Timer());
    }

    // Ÿ�̸� �ڷ�ƾ
    IEnumerator Timer()
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
        SceneController.Instance.ExitStage();
        if(curMode == GameMode.MultiGame)
            NetworkManager.Instance.LeaveRoom();
        curMode = GameMode.Lobby;

        // �Ͻ����� �ʱ�ȭ
        isPaused = false;
    }
    #endregion
}