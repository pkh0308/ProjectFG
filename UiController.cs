using UnityEngine;
using TMPro;
using System.Collections;

public class UiController : MonoBehaviour
{
    public static UiController Instance { get; private set; }

    [Header("UI Set")]
    [SerializeField] GameObject goalSet;
    [SerializeField] GameObject gameOverSet;
    [SerializeField] GameObject timeOutSet;

    [Header("Timer")]
    [SerializeField] GameObject timerSet;
    [SerializeField] TextMeshProUGUI timerText;
    int timeLimit;

    [Header("UserCount")]
    [SerializeField] GameObject userCountSet;
    [SerializeField] TextMeshProUGUI userCountText;
    int curUsers;
    int maxUsers;

    [Header("Count Down")]
    [SerializeField] GameObject countDownSet;
    [SerializeField] TextMeshProUGUI countDownText;

    [Header("Exit")]
    [SerializeField] GameObject exitSet;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CursorOff();
    }

    #region 마우스 커서 온오프
    void CursorOn()
    {
        // 마우스 보이기
        Cursor.visible = true;
    }
    void CursorOff()
    {
        // 마우스 숨기기
        Cursor.visible = false;
    }

    void OnDestroy()
    {
        CursorOn();
    }
    #endregion

    #region 타이머
    // 시간 제한 설정
    // -1이 입력된다면 타이머 X
    public void SetTimeLimit(int timeLimit)
    {
        this.timeLimit = timeLimit;
    }

    // 타이머 설정
    // timeLimit이 0보다 작다면(서바이벌 스테이지라면)
    // 남은 인원 수 설정
    void SetTimer()
    {
        timerSet.SetActive(true);
        GameManager.Instance.StartTimer(timeLimit);
    }

    public void UpdateTimer(int time)
    {
        timerText.text = string.Format("{0:0}:{1:00}", time / 60, time % 60);
    }
    #endregion

    #region 게임 종료
    // 승자라면 골인 UI,
    // 패자라면 게임오버 UI 활성화
    public void ResultOn(bool isWinner = true)
    {
        if(isWinner)
            goalSet.SetActive(true);
        else
            gameOverSet.SetActive(true);
    }
    // 현재 활성화된 결과 UI 해제
    public void ResultOff()
    {
        if (goalSet.activeSelf)
            goalSet.SetActive(false);
        else
            gameOverSet.SetActive(false);
    }

    public void ActiveTimeOutSet(bool act)
    {
        timeOutSet.SetActive(act);
    }
    #endregion

    #region 카운트다운
    public void StartCountDown()
    {
        StartCoroutine(CountDown());
    }

    // 스테이지 보여주기 후 카운트다운
    // 카운트다운 종료와 동시에 일시정지 해제
    IEnumerator CountDown()
    {
        int count = 3;
        countDownSet.SetActive(true);

        while (count > 0)
        {
            countDownText.text = count.ToString();
            yield return WfsManager.Instance.GetWaitForSeconds(1.0f);
            count--;
        }
        countDownText.text = "Go!!!";
        // 게임 시작 
        GameManager.Instance.PauseOff();
        // 타이머 or 유저 카운트 세팅
        if (timeLimit > 0)
            SetTimer();
        else
            SetUserCount();

        yield return WfsManager.Instance.GetWaitForSeconds(2.0f);
        countDownSet.SetActive(false);
    }
    #endregion

    #region 유저 카운트
    void SetUserCount()
    {
        maxUsers = NetworkManager.Instance.MaxUsers;
        curUsers = maxUsers;

        userCountText.text = $"{curUsers} / {maxUsers}";
        userCountSet.SetActive(true);
    }

    public void MinusUserCount()
    {
        curUsers--;
        userCountText.text = $"{curUsers} / {maxUsers}";
    }
    #endregion

    #region 나가기 창
    public bool SetExitPopUp()
    {
        if (GameManager.Instance.IsPaused)
            return true;

        // 열린 상태라면 닫기
        if(exitSet.activeSelf)
        {
            exitSet.SetActive(false);
            CursorOff();
            return true;
        }
        // 닫힌 상태라면 열기
        else
        {
            exitSet.SetActive(true);
            CursorOn();
            return false;
        }
    }

    public void Btn_Exit()
    {
        GameManager.Instance.PlayerExit();
    }

    public void Btn_ExitCancle()
    {
        exitSet.SetActive(false);
        CursorOff();
    }

    #endregion
}