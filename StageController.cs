using System.Collections;
using UnityEngine;

// 스테이지마다 생성
// 플레이어 생성 및 초기 스테이지 뷰를 담당
public class StageController : MonoBehaviour
{
    [Header("플레이어 생성")]
    [SerializeField] GameObject playerPrefab;
    [SerializeField] Vector3 initialPlayerPos;

    [Header("카메라 연출")]
    [SerializeField] CameraMove mainCamera;
    [SerializeField] float stageShowInterval;
    [SerializeField] float beginningInterval;
    [SerializeField] float endInterval;
    Vector3 initialPos;
    [SerializeField] Vector3 goalPos;

    void Start()
    {
        // 현재 스테이지를 액티브 씬으로 설정한 후 플레이어 생성
        SceneController.SetActiveSceneToCurStage();
        GameObject p = Instantiate(playerPrefab, initialPlayerPos, Quaternion.identity);

        // 스테이지 보여주기 코루틴 실행
        initialPos = mainCamera.gameObject.transform.position;
        StartCoroutine(ShowStage(p.transform));
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
}
