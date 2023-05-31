using System.Collections;
using UnityEngine;

// ������������ ����
// �÷��̾� ���� �� �ʱ� �������� �並 ���
public class StageController : MonoBehaviour
{
    [Header("�÷��̾� ����")]
    [SerializeField] GameObject playerPrefab;
    [SerializeField] Vector3 initialPlayerPos;

    [Header("ī�޶� ����")]
    [SerializeField] CameraMove mainCamera;
    [SerializeField] float stageShowInterval;
    [SerializeField] float beginningInterval;
    [SerializeField] float endInterval;
    Vector3 initialPos;
    [SerializeField] Vector3 goalPos;

    void Start()
    {
        // ���� ���������� ��Ƽ�� ������ ������ �� �÷��̾� ����
        SceneController.SetActiveSceneToCurStage();
        GameObject p = Instantiate(playerPrefab, initialPlayerPos, Quaternion.identity);

        // �������� �����ֱ� �ڷ�ƾ ����
        initialPos = mainCamera.gameObject.transform.position;
        StartCoroutine(ShowStage(p.transform));
    }

    // ���� �������� �����ֱ�� �Լ�
    // ī�޶� ������ ��Ʈ��
    IEnumerator ShowStage(Transform target)
    {
        GameManager.Instance.PauseOn();
        yield return WfsManager.Instance.GetWaitForSeconds(beginningInterval);

        float count = 0;
        // stageShowInterval���� ��ǥ�������� �̵�
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
