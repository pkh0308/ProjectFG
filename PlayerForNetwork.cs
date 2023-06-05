using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerForNetwork : MonoBehaviour
{
    [Header("Player Move Ref")]
    [SerializeField] float moveSpeed;
    [SerializeField] float jumpPower;
    [SerializeField] float slidePower;
    Vector3 moveVec;
    Vector3 inputVec;
    Vector3 lastPos;

    // �׷� ����
    bool isGrapping;

    // �˹� ����
    [Header("Kunck Back Ref")]
    [SerializeField] float knuckbackInterval;
    bool inKnuckBack;

    // ������Ʈ
    Rigidbody rigid;
    Animator animator;

    // �Ͻ����� ����
    bool isPaused;
    Vector3 beforeRigidVel;

    // ���� ����
    MovingPlatform movingPlat;
    RotatingPlatform rotatingPlat;

    // �ִϸ����� ���� ������
    enum AnimatorVar
    {
        isMoving,
        isJumping,
        doSlide,
        isGrapping,
        onKnuckBack
    }

    // �÷��̾� ����
    enum CurState
    {
        OnPlatform,
        IsJumping,
        IsSliding
    }
    CurState curState;

    // ��Ʈ��ũ ����
    PhotonView PV;
    bool isMine;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        PV = GetComponent<PhotonView>();
        isMine = PV.IsMine;
    }

    void Start()
    {
        lastPos = Vector3.zero;
        curState = CurState.OnPlatform;

        isGrapping = false;
    }

    // GameManager�� �Ͻ����� ���� üũ
    void Update()
    {
        // �Ͻ����� ���·� �����
        if (GameManager.Instance.IsPaused && !isPaused)
        {
            isPaused = true;
            PlayerStop();
            return;
        }
        // �Ͻ����� ���¿��� ������
        if (!GameManager.Instance.IsPaused && isPaused)
        {
            isPaused = false;
            PlayerStopOff();
            return;
        }
    }

    void FixedUpdate()
    {
        PlayerMove();
    }

    void PlayerMove()
    {
        // �� ĳ���Ͱ� �ƴ϶�� �н�
        if (!isMine)
            return;

        // �Ͻ����� ���³� �˹����̶�� �н�
        if (GameManager.Instance.IsPaused || inKnuckBack)
            return;

        // �ȱ� �ִϸ��̼� ����
        animator.SetBool(AnimatorVar.isMoving.ToString(), inputVec.magnitude > 0);
        // �̵� �������� �ٶ󺸱�
        transform.LookAt(transform.position + inputVec);


        moveVec = inputVec * moveSpeed * Time.fixedDeltaTime;
        // �����̴� ���� ���� ���
        if (movingPlat != null)
        {
            rigid.MovePosition(rigid.position + moveVec + movingPlat.GetMoveVec());
            return;
        }
        // ȸ�� ���� ���� ���
        if (rotatingPlat != null)
        {
            rigid.MovePosition(rigid.position + moveVec + rotatingPlat.GetRotateVec(transform.position));
            return;
        }

        // �Ϲ� ����
        rigid.MovePosition(rigid.position + moveVec);
    }

    #region �÷��̾� �Է�
    // wasd �Ǵ� �����
    // ĳ���� �̵� ����
    void OnMove(InputValue value)
    {
        inputVec.x = value.Get<Vector2>().x;
        inputVec.z = value.Get<Vector2>().y;
    }

    // �����̽��� �Է�
    // ���� ����� ����, ���� ���̶�� �����̵�
    // �����̵� ���̶�� ��ŵ
    void OnJump()
    {
        // �� ĳ���Ͱ� �ƴ϶�� �н�
        if (!isMine)
            return;
        // �Ͻ����� ���̶�� �н�
        if (isPaused)
            return;

        // ȸ�� �ʱ�ȭ
        rigid.angularVelocity = Vector3.zero;
        // �����̵� ���̶�� ��ŵ
        if (curState == CurState.IsSliding)
            return;

        // ���� ���̶�� �����̵�
        if (curState == CurState.IsJumping)
        {
            Slide();
            return;
        }

        // ����
        animator.SetBool(AnimatorVar.isJumping.ToString(), true);
        rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        curState = CurState.IsJumping;
        StageSoundController.PlaySfx((int)StageSoundController.StageSfx.jump);
    }

    void Slide()
    {
        PV.RPC(nameof(RPC_SetTrigger), RpcTarget.All, AnimatorVar.doSlide.ToString());
        // To Do - ���� ���� ���� �����Ұ�
        rigid.AddForce(moveVec.normalized * slidePower + Vector3.up, ForceMode.Impulse);
        curState = CurState.IsSliding;
        StageSoundController.PlaySfx((int)StageSoundController.StageSfx.jump);
    }

    // ���콺 ��Ŭ��
    // ��ó�� �ٸ� �÷��̾ �ִٸ� ������
    void OnFire()
    {
        // �� ĳ���Ͱ� �ƴ϶�� �н�
        if (!isMine)
            return;
        // �Ͻ����� ���̶�� �н�
        if (isPaused)
            return;

        // ���� �Ǵ� �����̵� ���̶�� �н�
        if (curState == CurState.IsJumping || curState == CurState.IsSliding)
            return;

        Grap();
    }

    void Grap()
    {
        // ToDo - ������ �÷��̾� �̵��ӵ� ���� ó��
        // ToDo - ������ �÷��̾� Ż�� ���� ���� ��
        isGrapping = isGrapping != true;
        animator.SetBool(AnimatorVar.isGrapping.ToString(), isGrapping);

    }
    #endregion

    #region �浹 ó��
    void OnTriggerEnter(Collider other)
    {
        // �߶� �� ������ ������ġ�� �̵�
        if (other.CompareTag(Tags.Fall))
        {
            transform.position = lastPos;
            rigid.angularVelocity = Vector3.zero;
            StageSoundController.PlaySfx((int)StageSoundController.StageSfx.reset);
            return;
        }
        // ������ġ ����
        if (other.gameObject.CompareTag(Tags.SavePoint))
        {
            Vector3 savePos = other.GetComponent<SavePoint>().GetPos();
            if (lastPos == savePos) // �̹� ����� ��ġ��� �н�
                return;

            lastPos = savePos;
            StageSoundController.PlaySfx((int)StageSoundController.StageSfx.savePoint);
            return;
        }

        // ���� ���� ����
        if (other.gameObject.CompareTag(Tags.Goal))
        {
            GameManager.Instance.Goal();
            return;
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        // �ٴ� �浹 �� ���� ����
        if (coll.gameObject.CompareTag(Tags.Platform))
        {
            OnPlatform();
            return;
        }

        // �����̴� ���� ���� �ִ� ���
        if (coll.gameObject.CompareTag(Tags.MovingPlatform))
        {
            OnPlatform();
            movingPlat = coll.gameObject.GetComponent<MovingPlatform>();
            return;
        }

        // ȸ�� ���� ���� �ִ� ���
        if (coll.gameObject.CompareTag(Tags.RotatingPlatform))
        {
            OnPlatform();
            rotatingPlat = coll.gameObject.GetComponent<RotatingPlatform>();
            return;
        }
    }

    void OnPlatform()
    {
        animator.SetBool(AnimatorVar.isJumping.ToString(), false);
        curState = CurState.OnPlatform;
    }

    void OnCollisionExit(Collision coll)
    {
        // �����̴� ���ǿ��� ���� ���
        if (coll.gameObject.CompareTag(Tags.MovingPlatform))
        {
            movingPlat = null;
            return;
        }

        // ȸ�� ���ǿ��� ���� ���
        if (coll.gameObject.CompareTag(Tags.RotatingPlatform))
        {
            rotatingPlat = null;
            return;
        }
    }
    #endregion

    #region �Ͻ����� ó��

    // �÷��̾� �Ͻ����� ����� �Լ�
    // timeScale ���� ���� �����ϴ°� �ٽ�
    void PlayerStop()
    {
        // ���� velocity ����
        beforeRigidVel = rigid.velocity;
        rigid.velocity = Vector3.zero;
        rigid.useGravity = false;
        animator.speed = 0;
    }
    // �÷��̾� �Ͻ����� ����
    void PlayerStopOff()
    {
        rigid.velocity = beforeRigidVel;
        rigid.useGravity = true;
        animator.speed = 1;
    }
    #endregion

    #region �˹� ó��
    public void OnKnuckBack(Vector3 forceVec)
    {
        // �̹� �˹����̶�� �н�
        if (inKnuckBack) return;

        StartCoroutine(KnuckBack(forceVec));
    }

    IEnumerator KnuckBack(Vector3 forceVec)
    {
        inKnuckBack = true;
        PV.RPC(nameof(RPC_SetTrigger), RpcTarget.All, AnimatorVar.onKnuckBack.ToString());
        rigid.AddRelativeForce(forceVec, ForceMode.Impulse);
        yield return WfsManager.Instance.GetWaitForSeconds(knuckbackInterval);

        inKnuckBack = false;
    }
    #endregion

    #region RPC �Լ�
    [PunRPC]
    void RPC_SetTrigger(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }

    #endregion
}