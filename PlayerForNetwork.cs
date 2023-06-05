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

    // 그랩 여부
    bool isGrapping;

    // 넉백 관련
    [Header("Kunck Back Ref")]
    [SerializeField] float knuckbackInterval;
    bool inKnuckBack;

    // 컴포넌트
    Rigidbody rigid;
    Animator animator;

    // 일시정지 관련
    bool isPaused;
    Vector3 beforeRigidVel;

    // 발판 관련
    MovingPlatform movingPlat;
    RotatingPlatform rotatingPlat;

    // 애니메이터 변수 관리용
    enum AnimatorVar
    {
        isMoving,
        isJumping,
        doSlide,
        isGrapping,
        onKnuckBack
    }

    // 플레이어 상태
    enum CurState
    {
        OnPlatform,
        IsJumping,
        IsSliding
    }
    CurState curState;

    // 네트워크 관련
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

    // GameManager의 일시정지 여부 체크
    void Update()
    {
        // 일시정지 상태로 변경됨
        if (GameManager.Instance.IsPaused && !isPaused)
        {
            isPaused = true;
            PlayerStop();
            return;
        }
        // 일시정지 상태에서 해제됨
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
        // 내 캐릭터가 아니라면 패스
        if (!isMine)
            return;

        // 일시정지 상태나 넉백중이라면 패스
        if (GameManager.Instance.IsPaused || inKnuckBack)
            return;

        // 걷기 애니메이션 설정
        animator.SetBool(AnimatorVar.isMoving.ToString(), inputVec.magnitude > 0);
        // 이동 방향으로 바라보기
        transform.LookAt(transform.position + inputVec);


        moveVec = inputVec * moveSpeed * Time.fixedDeltaTime;
        // 움직이는 발판 위일 경우
        if (movingPlat != null)
        {
            rigid.MovePosition(rigid.position + moveVec + movingPlat.GetMoveVec());
            return;
        }
        // 회전 발판 위일 경우
        if (rotatingPlat != null)
        {
            rigid.MovePosition(rigid.position + moveVec + rotatingPlat.GetRotateVec(transform.position));
            return;
        }

        // 일반 발판
        rigid.MovePosition(rigid.position + moveVec);
    }

    #region 플레이어 입력
    // wasd 또는 ↑←↓→
    // 캐릭터 이동 방향
    void OnMove(InputValue value)
    {
        inputVec.x = value.Get<Vector2>().x;
        inputVec.z = value.Get<Vector2>().y;
    }

    // 스페이스바 입력
    // 발판 위라면 점프, 점프 중이라면 슬라이딩
    // 슬라이딩 중이라면 스킵
    void OnJump()
    {
        // 내 캐릭터가 아니라면 패스
        if (!isMine)
            return;
        // 일시정지 중이라면 패스
        if (isPaused)
            return;

        // 회전 초기화
        rigid.angularVelocity = Vector3.zero;
        // 슬라이딩 중이라면 스킵
        if (curState == CurState.IsSliding)
            return;

        // 점프 중이라면 슬라이딩
        if (curState == CurState.IsJumping)
        {
            Slide();
            return;
        }

        // 점프
        animator.SetBool(AnimatorVar.isJumping.ToString(), true);
        rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        curState = CurState.IsJumping;
        StageSoundController.PlaySfx((int)StageSoundController.StageSfx.jump);
    }

    void Slide()
    {
        PV.RPC(nameof(RPC_SetTrigger), RpcTarget.All, AnimatorVar.doSlide.ToString());
        // To Do - 추후 방향 벡터 수정할것
        rigid.AddForce(moveVec.normalized * slidePower + Vector3.up, ForceMode.Impulse);
        curState = CurState.IsSliding;
        StageSoundController.PlaySfx((int)StageSoundController.StageSfx.jump);
    }

    // 마우스 좌클릭
    // 근처에 다른 플레이어가 있다면 붙잡음
    void OnFire()
    {
        // 내 캐릭터가 아니라면 패스
        if (!isMine)
            return;
        // 일시정지 중이라면 패스
        if (isPaused)
            return;

        // 점프 또는 슬라이딩 중이라면 패스
        if (curState == CurState.IsJumping || curState == CurState.IsSliding)
            return;

        Grap();
    }

    void Grap()
    {
        // ToDo - 붙잡은 플레이어 이동속도 감소 처리
        // ToDo - 붙잡힌 플레이어 탈출 조건 정할 것
        isGrapping = isGrapping != true;
        animator.SetBool(AnimatorVar.isGrapping.ToString(), isGrapping);

    }
    #endregion

    #region 충돌 처리
    void OnTriggerEnter(Collider other)
    {
        // 추락 시 마지막 저장위치로 이동
        if (other.CompareTag(Tags.Fall))
        {
            transform.position = lastPos;
            rigid.angularVelocity = Vector3.zero;
            StageSoundController.PlaySfx((int)StageSoundController.StageSfx.reset);
            return;
        }
        // 저장위치 갱신
        if (other.gameObject.CompareTag(Tags.SavePoint))
        {
            Vector3 savePos = other.GetComponent<SavePoint>().GetPos();
            if (lastPos == savePos) // 이미 저장된 위치라면 패스
                return;

            lastPos = savePos;
            StageSoundController.PlaySfx((int)StageSoundController.StageSfx.savePoint);
            return;
        }

        // 골인 지점 도착
        if (other.gameObject.CompareTag(Tags.Goal))
        {
            GameManager.Instance.Goal();
            return;
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        // 바닥 충돌 시 상태 변경
        if (coll.gameObject.CompareTag(Tags.Platform))
        {
            OnPlatform();
            return;
        }

        // 움직이는 발판 위에 있는 경우
        if (coll.gameObject.CompareTag(Tags.MovingPlatform))
        {
            OnPlatform();
            movingPlat = coll.gameObject.GetComponent<MovingPlatform>();
            return;
        }

        // 회전 발판 위에 있는 경우
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
        // 움직이는 발판에서 나갈 경우
        if (coll.gameObject.CompareTag(Tags.MovingPlatform))
        {
            movingPlat = null;
            return;
        }

        // 회전 발판에서 나갈 경우
        if (coll.gameObject.CompareTag(Tags.RotatingPlatform))
        {
            rotatingPlat = null;
            return;
        }
    }
    #endregion

    #region 일시정지 처리

    // 플레이어 일시정지 연출용 함수
    // timeScale 수정 없이 구현하는게 핵심
    void PlayerStop()
    {
        // 현재 velocity 저장
        beforeRigidVel = rigid.velocity;
        rigid.velocity = Vector3.zero;
        rigid.useGravity = false;
        animator.speed = 0;
    }
    // 플레이어 일시정지 해제
    void PlayerStopOff()
    {
        rigid.velocity = beforeRigidVel;
        rigid.useGravity = true;
        animator.speed = 1;
    }
    #endregion

    #region 넉백 처리
    public void OnKnuckBack(Vector3 forceVec)
    {
        // 이미 넉백중이라면 패스
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

    #region RPC 함수
    [PunRPC]
    void RPC_SetTrigger(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }

    #endregion
}