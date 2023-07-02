using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
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
    [SerializeField] float knockbackInterval;
    bool inKnockBack;

    // ������Ʈ
    Rigidbody rigid;
    Animator animator;

    // �Ͻ����� ����
    bool isPaused;
    Vector3 beforeRigidVel;

    // ���� ����
    MovingPlatform movingPlat;
    RotatingPlatform rotatingPlat;
    Vector3 befVelocity;

    // �ִϸ����� ���� ������
    enum AnimatorVar
    {
        isMoving,
        isJumping,
        doSlide,
        isGrapping,
        onKnockBack
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
    bool isSingle;

    #region �ʱ�ȭ
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // ��Ƽ �����̶�� PhotonView �ʱ�ȭ
        if (GameManager.Instance.CurMode == GameManager.GameMode.MultiGame)
        {
            PV = GetComponent<PhotonView>();
            isMine = PV.IsMine;
            isSingle = false;
        }
        // �̱� �����̶�� PhotonView �ʱ�ȭ �н�
        else
        {
            isMine = true;
            isSingle = true;
        }
    }

    void Start()
    {
        lastPos = Vector3.zero;
        curState = CurState.OnPlatform;

        isGrapping = false;
        inKnockBack = false;
    }
    #endregion

    #region Update / FixedUpdate

    // GameManager�� �Ͻ����� ���� üũ
    void Update()
    {
        // �Ͻ����� ���·� �����
        if(GameManager.Instance.IsPaused && !isPaused)
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
        if (GameManager.Instance.IsPaused || inKnockBack)
            return;

        // �ȱ� �ִϸ��̼� ����
        animator.SetBool(AnimatorVar.isMoving.ToString(), inputVec.magnitude > 0);

        // ī�޶� ȸ���� ���� �̵����� ȸ��
        moveVec = Quaternion.Euler(0, GameManager.Instance.CamAngle, 0) 
                  * (moveSpeed * inputVec * Time.fixedDeltaTime);

        // �̵� �������� �ٶ󺸱�
        transform.LookAt(transform.position + moveVec);

        // �Ϲ� �̵�
        rigid.MovePosition(rigid.position + moveVec);
    }
    #endregion

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
        SetTrigger(AnimatorVar.doSlide.ToString());
        rigid.AddForce(moveVec.normalized * slidePower, ForceMode.Impulse);
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
        if (!isMine) return;

        // �߶� �� ������ ������ġ�� �̵�
        if (other.CompareTag(Tags.Fall))
        {
            transform.position = lastPos;
            rigid.velocity = Vector3.zero;
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
        // �ƿ�
        if (other.gameObject.CompareTag(Tags.OutArea))
        {
            GameManager.Instance.PlayerOut();
            return;
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        if (!isMine) return;

        // �ٴ� �浹 �� ���� ����
        if (coll.gameObject.CompareTag(Tags.Platform))
        {
            OnPlatform();
            return;
        }

        // �����̴� ���ǿ� ������ ���
        if (coll.gameObject.CompareTag(Tags.MovingPlatform))
        {
            OnPlatform();
            movingPlat = coll.gameObject.GetComponent<MovingPlatform>();
            return;
        }

        // ȸ�� ���ǿ� ������ ���
        if (coll.gameObject.CompareTag(Tags.RotatingPlatform))
        {
            OnPlatform();
            rotatingPlat = coll.gameObject.GetComponent<RotatingPlatform>();
            return;
        }
        // TOF ���� ���� ������ ���
        // ��¥ ������ ��쿡�� ���� ���� ����
        if (coll.gameObject.CompareTag(Tags.TOFPlatform))
        {
            if (coll.gameObject.GetComponent<TrueOrFalsePlatform>().IsTrue)
                OnPlatform();
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
        if (!isMine) return;

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
    public void OnKnockBack(Vector3 forceVec)
    {
        // �̹� �˹����̶�� �н�
        if (inKnockBack) return;

        StartCoroutine(KnockBack(forceVec));
    }

    IEnumerator KnockBack(Vector3 forceVec)
    {
        inKnockBack = true;
        SetTrigger(AnimatorVar.onKnockBack.ToString());
        rigid.AddForce(forceVec, ForceMode.Impulse);
        yield return WfsManager.Instance.GetWaitForSeconds(knockbackInterval);

        inKnockBack = false;
    }
    #endregion

    #region �ִϸ��̼�(Ʈ����)
    // �̱� �����̶�� �Ϲ����� SetTrigger ���
    // ��Ƽ �����̶�� RPC�� ����
    void SetTrigger(string triggerName)
    {
        if (isSingle)
            animator.SetTrigger(triggerName);
        else
            PV.RPC(nameof(RPC_SetTrigger), RpcTarget.All, triggerName);
    }

    [PunRPC]
    void RPC_SetTrigger(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }
    #endregion
}