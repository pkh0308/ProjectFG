using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

// 게임 전반을 총괄하는 매니저 클래스
// 
public class GameManager : MonoBehaviour
{
    #region 싱글톤 구현
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

    // 일시정지
    bool isPaused;
    public bool IsPaused { get { return isPaused; } }

    // 코루틴 관리용 변수
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
                // ToDo - 멀티 게임 시작 처리

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

    public void Goal()
    {
        // 일시정지
        isPaused = true;

        switch (curMode) 
        {
            case GameMode.SingleGame:
                StopCoroutine(timerRoutine);
                StartCoroutine(GoalInSingleGame());
                break; 
            case GameMode.MultiGame:
                // ToDo - 멀티 게임 골인 시 처리
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

        // 일시정지 초기화
        isPaused = false;
    }

    //IEnumerator GoalInMultiGame()
    //{

    //}

    // 타이머 코루틴
    IEnumerator Timer()
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

        UiController.Instance.ActiveTimeOutSet(true);
        yield return WfsManager.Instance.GetWaitForSeconds(timeOutInterval);

        UiController.Instance.ActiveTimeOutSet(false);
        SceneController.Instance.ExitStage();
        curMode = GameMode.Lobby;

        // 일시정지 초기화
        isPaused = false;
    }
}