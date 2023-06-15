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

    void Awake()
    {
        Instance = this;
    }

    // 시간 제한 설정
    // -1이 입력된다면 타이머 X
    public void SetTimeLimit(int timeLimit)
    {
        this.timeLimit = timeLimit;
    }

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

    public void UpdateTimer(int time)
    {
        timerText.text = string.Format("{0:0}:{1:00}", time / 60, time % 60);
    }

    public void StartCountDown()
    {
        StartCoroutine(CountDown());
    }

    // 타이머 설정
    // timeLimit이 0보다 작다면(서바이벌 스테이지라면)
    // 남은 인원 수 설정
    void SetTimer()
    {
        timerSet.SetActive(true);
        GameManager.Instance.StartTimer(timeLimit);
    }

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
}