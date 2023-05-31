using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

// ���� ������ �Ѱ��ϴ� �Ŵ��� Ŭ����
// 
public class GameManager : MonoBehaviour
{
    #region �̱��� ����
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
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

    public enum GameMode
    {
        Lobby,
        SingleGame,
        MultiGame
    }
    GameMode curMode;

    public void GameStart(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.SingleGame:
                StartSingleGame();
                break;
            case GameMode.MultiGame:
                // ToDo - ��Ƽ ���� ���� ó��

                break;
        }
    }

    void StartSingleGame()
    {
        SceneController.Instance.EnterStage(0);
        curMode = GameMode.SingleGame;
    }

    public void StartTimer()
    {
        timerRoutine = StartCoroutine(Timer());
    }

    //IEnumerator StartMultiGame()
    //{

    //}

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

    public void Goal()
    {
        // �Ͻ�����
        isPaused = true;

        switch (curMode) 
        {
            case GameMode.SingleGame:
                StopCoroutine(timerRoutine);
                StartCoroutine(GoalInSingleGame());
                break; 
            case GameMode.MultiGame:
                // ToDo - ��Ƽ ���� ���� �� ó��
                break;
        }
    }

    IEnumerator GoalInSingleGame()
    {
        UiController.Instance.ActiveGoalSet(true);
        yield return WfsManager.Instance.GetWaitForSeconds(goalInInterval);

        UiController.Instance.ActiveGoalSet(false);
        SceneController.Instance.ExitStage();
        curMode = GameMode.Lobby;

        // �Ͻ����� �ʱ�ȭ
        isPaused = false;
    }

    //IEnumerator GoalInMultiGame()
    //{

    //}

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

        UiController.Instance.ActiveTimeOutSet(true);
        yield return WfsManager.Instance.GetWaitForSeconds(timeOutInterval);

        UiController.Instance.ActiveTimeOutSet(false);
        SceneController.Instance.ExitStage();
        curMode = GameMode.Lobby;

        // �Ͻ����� �ʱ�ȭ
        isPaused = false;
    }
}