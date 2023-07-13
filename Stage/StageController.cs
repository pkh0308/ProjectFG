using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;

// ������������ ����
// �÷��̾� ���� �� �ʱ� �������� �並 ���
public class StageController : MonoBehaviour
{
    protected PhotonView PV;

    [Header("�ð� ����")]
    [Tooltip("�����̹� ����������� -1�� ����")]
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

    enum State
    {
        NotBegin,
        OnGame,
        EndGame
    }
    State curState;

    void Awake()
    {
        isSingleGame = GameManager.Instance.IsSingleGame;
        curState = State.NotBegin;
        PV = GetComponent<PhotonView>();

        // �̱۰���/��Ƽ���� �ʱ�ȭ ����
        if (NetworkManager.Instance.InRoom)
            Initialize_Multi();
        else
            Initialize_Single();
    }

    #region ���� �Լ�
    // ��ӹ޴� Ŭ�������� ����
    // �̱� �÷��̿� �ʱ�ȭ �Լ�
    protected virtual void Initialize_Single() { }
    // ��Ƽ �÷��̿� �ʱ�ȭ �Լ�
    protected virtual void Initialize_Multi() { }
    // ���� ���� ������ ȣ���ϴ� �Լ�
    protected virtual void OnGameStart() { }
    // ���� ���� ������ ȣ���ϴ� �Լ�
    protected virtual void OnGameStop() { }
    #endregion

    void Start()
    {
        // �ε� ��ũ�� �� �� ����ȭ ����
        SceneController.Instance.LoadingCompleted();
        // ���� ���������� ��Ƽ�� ������ ���� �� ĳ���� ����
        SceneController.Instance.SetActiveScene();
        // null�� ��ȯ�޾��� ��� ���� �߻���Ŵ
        GameObject p = InstantiatePlayer() ?? throw new Exception("ĳ���� ���� ����");

        // �������� �����ֱ� �ڷ�ƾ ����
        initialPos = mainCamera.gameObject.transform.position;
        StartCoroutine(ShowStage(p.transform));

        // Ÿ�̸� ����
        UiController.Instance.SetTimeLimit(timeLimit);
    }

    // ���� ���� �� ���� ���� üũ
    void Update()
    {
        // ���� ���� ����
        if(curState == State.NotBegin && !GameManager.Instance.IsPaused) 
        {
            curState = State.OnGame;
            OnGameStart();
            return;
        }
        // ���� ���� ����
        if (curState == State.OnGame && GameManager.Instance.IsPaused)
        {
            curState= State.EndGame;
            OnGameStop();
            return;
        }
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

        // ī�޶� Ÿ�� ���� �� �ʱ� ȸ��
        mainCamera.SetTarget(target);
        if(GameManager.Instance.CurMode == GameManager.GameMode.MultiGame)
            Camera.main.GetComponent<CameraMove>().SetRotation(
                startPos[NetworkManager.Instance.GetMyIdx()].eulerAngles.y);

        // ī��Ʈ�ٿ� ����
        UiController.Instance.StartCountDown();
    }
}