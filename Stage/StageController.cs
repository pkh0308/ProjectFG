using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;

// 스테이지마다 생성
// 플레이어 생성 및 초기 스테이지 뷰를 담당
public class StageController : MonoBehaviour
{
    protected PhotonView PV;

    [Header("시간 제한")]
    [Tooltip("서바이벌 스테이지라면 -1로 설정")]
    [SerializeField] int timeLimit;

    [Header("플레이어 생성")]
    [SerializeField] GameObject playerPrefab;
    [SerializeField] Vector3 initialPlayerPos;
    [SerializeField] Transform[] startPos;

    [Header("카메라 연출")]
    [SerializeField] CameraMove mainCamera;
    [SerializeField] float stageShowInterval;
    [SerializeField] float beginningInterval;
    [SerializeField] float endInterval;
    Vector3 initialPos;
    [SerializeField] Vector3 goalPos;

    // 싱글/멀티 여부
    bool isSingleGame;

    void Awake()
    {
        isSingleGame = GameManager.Instance.IsSingleGame;
        PV = GetComponent<PhotonView>();

        // 싱글게임/멀티게임 초기화 구분
        if (NetworkManager.Instance.InRoom)
            Initialize_Multi();
        else
            Initialize_Single();
    }

    // 상속받는 클래스에서 구현
    // 싱글 플레이용 초기화 함수
    protected virtual void Initialize_Single()
    {

    }
    // 멀티 플레이용 초기화 함수
    protected virtual void Initialize_Multi()
    {

    }
    // 일시정지가 해제된 시점에 호출되는 함수
    protected virtual void OnGameStart()
    {

    }

    void Start()
    {
        // 현재 스테이지를 액티브 씬으로 설정 후 캐릭터 생성
        SceneController.Instance.SetActiveScene();
        // null을 반환받았을 경우 예외 발생시킴
        GameObject p = InstantiatePlayer() ?? throw new Exception("캐릭터 생성 실패");

        // 스테이지 보여주기 코루틴 실행
        initialPos = mainCamera.gameObject.transform.position;
        StartCoroutine(ShowStage(p.transform));
        StartCoroutine(PauseCheck());

        // 타이머 설정
        UiController.Instance.SetTimeLimit(timeLimit);
    }

    // 캐릭터 생성
    // 멀티플레이의 경우 NetworkManager를 통해 생성
    GameObject InstantiatePlayer()
    {
        GameObject p;

        if (isSingleGame)
            p = Instantiate(playerPrefab, initialPlayerPos, Quaternion.identity);
        else
            p = NetworkManager.Instance.InstantiateCharacter(startPos);

        return p;
    }

    // 최초 스테이지 보여주기용 함수
    // 카메라 움직임 컨트롤
    IEnumerator ShowStage(Transform target)
    {
        GameManager.Instance.PauseOn();
        yield return WfsManager.Instance.GetWaitForSeconds(beginningInterval);

        float count = 0;
        // stageShowInterval동안 목표지점으로 이동
        while (count < stageShowInterval)
        {
            yield return null;
            mainCamera.gameObject.transform.position = 
                Vector3.Lerp(initialPos, goalPos, count / stageShowInterval);
            count += Time.deltaTime;
        }
        yield return WfsManager.Instance.GetWaitForSeconds(endInterval);

        mainCamera.SetTarget(target);
        UiController.Instance.StartCountDown();
    }

    IEnumerator PauseCheck()
    {
        bool pause = false;
        // GameManager의 일시정지가 풀릴때까지 루프
        while (!pause)
        {
            if(GameManager.Instance.IsPaused)
            {
                pause = true;
                break;
            }
            yield return null;
        }
        OnGameStart();
    }
}
