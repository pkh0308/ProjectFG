using UnityEngine;

// 회전 발판용 클래스
// 시계방향 / 반시계방향
public class RotatingPlatform : MonoBehaviour
{
    Rigidbody rigid;

    [SerializeField] float rotateDegree;
    Quaternion deltaRotation;
    Vector3 rotateVelocity;

    // 플레이어 회전용 변수
    Quaternion rotationDir;
    Vector3 dirVec;

    public enum RotateDirection
    {
        Clockwise,
        Counter_Clockwise
    }
    [SerializeField] RotateDirection myDir;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // 방향 벡터 초기화
        switch (myDir)
        {
            case RotateDirection.Clockwise:
                rotateVelocity = new Vector3(0, rotateDegree, 0);
                rotationDir = Quaternion.Euler(0, -90, 0);
                break;
            case RotateDirection.Counter_Clockwise:
                rotateVelocity = new Vector3(0, -rotateDegree, 0);
                rotationDir = Quaternion.Euler(0, 90, 0);
                break;
        }
    }

    void FixedUpdate()
    {
        deltaRotation = Quaternion.Euler(rotateVelocity * Time.fixedDeltaTime);
        rigid.MoveRotation(rigid.rotation * deltaRotation);
    }

    // 플레이어 회전용 함수
    // (플레이어 -> 회전 발판) 방향의 벡터를 90/-90도 회전시킨 값을 반환
    public Vector3 GetRotateVec(Vector3 playerPos)
    {
        dirVec = rotationDir * (transform.position - playerPos);
        dirVec = dirVec.normalized * 2 * Mathf.PI * dirVec.magnitude / (360 / rotateDegree);
        return dirVec * Time.fixedDeltaTime;
    }
}
