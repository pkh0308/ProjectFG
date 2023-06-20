using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMove : MonoBehaviour
{
    Transform target;

    // 카메라 회전 관련
    [Header("카메라 추적 대상")]
    [SerializeField] Transform cameraOffset;

    [Header("카메라 회전 속도")]
    [SerializeField] float hRotateSpeed;
    [SerializeField] float vRotateSpeed;

    [Header("최대 각도 제한")]
    [Range(10, 60)]
    [SerializeField] float rotLimitMin;
    [Range(10, 60)]
    [SerializeField] float rotLimitMax;

    Vector2 curLookVec;
    Vector2 befLookVec;
    float xOffset;
    float yOffset;
    float sinRes;
    float cosRes;

    bool rDown;

    // 외부 호출용 함수
    // 추적 대상(타겟) 설정
    public void SetTarget(Transform t)
    {
        target = t;
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

        sinRes = Mathf.Sin(transform.eulerAngles.y * Mathf.Deg2Rad);
        cosRes = Mathf.Cos(transform.eulerAngles.y * Mathf.Deg2Rad);

        xOffset = (curLookVec.x - befLookVec.x) * hRotateSpeed * Time.deltaTime;
        yOffset = (curLookVec.y - befLookVec.y) * hRotateSpeed * Time.deltaTime;

        transform.RotateAround(target.position, Vector3.up, xOffset);
        transform.RotateAround(target.position, Vector3.right, -yOffset * cosRes);
        transform.RotateAround(target.position, Vector3.forward, yOffset * sinRes);
        // 최대/최소 각도 제한
        if (transform.eulerAngles.x > rotLimitMax)
        {
            transform.rotation = Quaternion.Euler(rotLimitMax - 0.1f, transform.eulerAngles.y, 0);
            return;
        }
        if (transform.eulerAngles.x < rotLimitMin)
        {
            transform.rotation = Quaternion.Euler(rotLimitMin + 0.1f, transform.eulerAngles.y, 0);
            return;
        }

        // 오프셋도 동일하게 회전
        cameraOffset.RotateAround(Vector3.zero, Vector3.up, xOffset);
        cameraOffset.RotateAround(Vector3.zero, Vector3.right, -yOffset * cosRes);
        cameraOffset.RotateAround(Vector3.zero, Vector3.forward, yOffset * sinRes);

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