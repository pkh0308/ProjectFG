using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMove : MonoBehaviour
{
    Transform target;

    // 카메라 회전 관련
    [Header("카메라 오프셋")]
    [SerializeField] Transform cameraOffset;
    [SerializeField] Transform cameraRef;

    [Header("카메라 회전 속도")]
    [SerializeField] float hRotateSpeed;
    [SerializeField] float vRotateSpeed;

    [Header("최대 각도 제한")]
    [Range(10, 60)]
    [SerializeField] float rotateLimitMax;
    [Range(10, 60)]
    [SerializeField] float rotateLimitMin;

    Vector2 curLookVec;
    Vector2 befLookVec;
    float xOffset;
    float yOffset;

    bool rDown;

    // 외부 호출용 함수
    // 추적 대상(타겟) 설정
    public void SetTarget(Transform t)
    {
        target = t;

        cameraRef.transform.position = transform.position;
        cameraRef.rotation = transform.rotation;
    }

    void OnRightDown()
    {
        rDown = true;
    }

    void OnRightUp()
    {
        rDown = false;
        befLookVec = Vector2.zero;
    }

    void OnLook(InputValue value)
    {
        // 타겟이 없거나 마우스 오른쪽 클릭중이 아니라면 패스
        if (target == null || rDown == false) return;

        curLookVec = value.Get<Vector2>();
        // 저장된 이전 값이 없다면 저장하고 패스
        if (befLookVec == Vector2.zero) 
        {
            befLookVec = curLookVec;
            return;
        }

        xOffset = (curLookVec.x - befLookVec.x) * vRotateSpeed * Time.deltaTime;
        yOffset = (curLookVec.y - befLookVec.y) * hRotateSpeed * Time.deltaTime;

        cameraRef.position = transform.position;
        // x축 기준 회전
        // 범위 체크를 위해 사전 테스트용 트랜스폼을 먼저 회전
        cameraRef.RotateAround(target.position, Vector3.right, -yOffset);
        // 범위 내일 경우에만 회전 적용
        if (cameraRef.eulerAngles.x > rotateLimitMin && cameraRef.eulerAngles.x < rotateLimitMax)
        {
            transform.RotateAround(target.position, Vector3.right, -yOffset);
            cameraOffset.RotateAround(Vector3.zero, Vector3.right, -yOffset);
        }
        cameraRef.rotation = transform.rotation;

        // y축 기준 회전
        transform.RotateAround(target.position, Vector3.up, xOffset);
        cameraOffset.RotateAround(Vector3.zero, Vector3.up, xOffset);

        // rigidbody를 이용한 roateAround
        //Quaternion q = Quaternion.AngleAxis(orbitSpeed, transform.forward);
        //rb.MovePosition(q * (rb.transform.position - target.position) + target.position);
        //rb.MoveRotation(rb.transform.rotation * q);

        // 현재 값을 이전 값으로 저장
        befLookVec = curLookVec;
    }

    void FixedUpdate()
    {
        if (target == null) return;

        transform.position = target.position + cameraOffset.position;

        if (transform.eulerAngles.z != 0)
        {
            transform.rotation =
                Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 0);
        }
    }
}