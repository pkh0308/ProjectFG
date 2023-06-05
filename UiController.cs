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

    [Header("Count Down")]
    [SerializeField] GameObject countDownSet;
    [SerializeField] TextMeshProUGUI countDownText;

    void Awake()
    {
        Instance = this;
    }

    // ���ڶ�� ���� UI,
    // ���ڶ�� ���ӿ��� UI Ȱ��ȭ
    public void ResultOn(bool isWinner)
    {
        if(isWinner)
            goalSet.SetActive(true);
        else
            gameOverSet.SetActive(true);
    }
    // ���� Ȱ��ȭ�� ��� UI ����
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
        // ���� ����
        GameManager.Instance.PauseOff();
        // Ÿ�̸� ����
        timerSet.SetActive(true);
        GameManager.Instance.StartTimer();

        yield return WfsManager.Instance.GetWaitForSeconds(2.0f);
        countDownSet.SetActive(false);
    }
}