using System;
using System.Collections;
using UnityEngine;

// ������������ ����
// �÷��̾� ���� �� �ʱ� �������� �並 ���
public class StageController : MonoBehaviour
{
    [Header("�ð� ����")]
    [SerializeField] int timeLimit;

    [Header("�÷��̾� ����")]
    [SerializeField] GameObject playerPrefab;
    [SerializeField] Vector3 initialPlayerPos;
    [SerializeField] Transform[] startPos;

    [Header("ī�޶� ����")]
    [SerializeField] CameraMove mainCamera;
    [SerializeField] float stageShowInterval;
    [SerializeField] float beginningInterval;
    [SerializeField] float endInterval;
    Vector3 initialPos;
    [SerializeField] Vector3 goalPos;

    // �̱�/��Ƽ ����
    bool isSingleGame;

    void Awake()
    { 
        isSingleGame = GameManager.Instance.IsSingleGame;
        Initialize();
    }

    // ��ӹ޴� Ŭ�������� ����
    protected virtual void Initialize()
    {

    }

    void Start()
    {
        // ���� ���������� ��Ƽ�� ������ ���� �� ĳ���� ����
        SceneController.SetActiveSceneToCurStage();
        // null�� ��ȯ�޾��� ��� ���� �߻���Ŵ
        GameObject p = InstantiatePlayer() ?? throw new Exception("ĳ���� ���� ����");

        // �������� �����ֱ� �ڷ�ƾ ����
        initialPos = mainCamera.gameObject.transform.position;
        StartCoroutine(ShowStage(p.transform));

        // Ÿ�̸� ����
        UiController.Instance.SetTimeLimit(timeLimit);
    }

    // ĳ���� ����
    // ��Ƽ�÷����� ��� NetworkManager�� ���� ����
    GameObject InstantiatePlayer()
    {
        GameObject p;

        if (isSingleGame)
            p = Instantiate(playerPrefab, initialPlayerPos, Quaternion.identity);
        else
            p = NetworkManager.Instance.InstantiateCharacter(startPos);

        return p;
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