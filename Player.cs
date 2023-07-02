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

    // 그랩 여부
    bool isGrapping;

    // 넉백 관련
    [Header("Kunck Back Ref")]
    [SerializeField] float knockbackInterval;
    bool inKnockBack;

    // 컴포넌트
    Rigidbody rigid;
    Animator animator;

    // 일시정지 관련
    bool isPaused;
    Vector3 beforeRigidVel;

    // 발판 관련
    MovingPlatform movingPlat;
    RotatingPlatform rotatingPlat;
    Vector3 befVelocity;

    // 애니메이터 변수 관리용
    enum AnimatorVar
    {
        isMoving,
        isJumping,
        doSlide,
        isGrapping,
        onKnockBack
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
    bool isSingle;

    #region 초기화
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // 멀티 게임이라면 PhotonView 초기화
        if (GameManager.Instance.CurMode == GameManager.GameMode.MultiGame)
        {
            PV = GetComponent<PhotonView>();
            isMine = PV.IsMine;
            isSingle = false;
        }
        // 싱글 게임이라면 PhotonView 초기화 패스
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

    // GameManager의 일시정지 여부 체크
    void Update()
    {
        // 일시정지 상태로 변경됨
        if(GameManager.Instance.IsPaused && !isPaused)
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
        if (GameManager.Instance.IsPaused || inKnockBack)
            return;

        // 걷기 애니메이션 설정
        animator.SetBool(AnimatorVar.isMoving.ToString(), inputVec.magnitude > 0);

        // 카메라 회전에 따른 이동방향 회전
        moveVec = Quaternion.Euler(0, GameManager.Instance.CamAngle, 0) 
                  * (moveSpeed * inputVec * Time.fixedDeltaTime);

        // 이동 방향으로 바라보기
        transform.LookAt(transform.position + moveVec);

        // 일반 이동
        rigid.MovePosition(rigid.position + moveVec);
    }
    #endregion

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
        SetTrigger(AnimatorVar.doSlide.ToString());
        rigid.AddForce(moveVec.normalized * slidePower, ForceMode.Impulse);
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
        if (!isMine) return;

        // 추락 시 마지막 저장위치로 이동
        if (other.CompareTag(Tags.Fall))
        {
            transform.position = lastPos;
            rigid.velocity = Vector3.zero;
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
        // 아웃
        if (other.gameObject.CompareTag(Tags.OutArea))
        {
            GameManager.Instance.PlayerOut();
            return;
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        if (!isMine) return;

        // 바닥 충돌 시 상태 변경
        if (coll.gameObject.CompareTag(Tags.Platform))
        {
            OnPlatform();
            return;
        }

        // 움직이는 발판에 착지한 경우
        if (coll.gameObject.CompareTag(Tags.MovingPlatform))
        {
            OnPlatform();
            movingPlat = coll.gameObject.GetComponent<MovingPlatform>();
            return;
        }

        // 회전 발판에 착지한 경우
        if (coll.gameObject.CompareTag(Tags.RotatingPlatform))
        {
            OnPlatform();
            rotatingPlat = coll.gameObject.GetComponent<RotatingPlatform>();
            return;
        }
        // TOF 발판 위에 착지한 경우
        // 진짜 발판일 경우에만 착지 판정 실행
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
    public void OnKnockBack(Vector3 forceVec)
    {
        // 이미 넉백중이라면 패스
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

    #region 애니메이션(트리거)
    // 싱글 게임이라면 일반적인 SetTrigger 사용
    // 멀티 게임이라면 RPC로 실행
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