using UnityEngine;

public class RotatingObstacle : MonoBehaviour
{
    Rigidbody rigid;

    [SerializeField] float rotateDegree;
    Quaternion deltaRotation;
    Vector3 rotateVelocity;

    [SerializeField] bool isRotating;

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
                break;
            case RotateDirection.Counter_Clockwise:
                rotateVelocity = new Vector3(0, -rotateDegree, 0);
                break;
        }
    }

    void FixedUpdate()
    {
        if (!isRotating) return;

        deltaRotation = Quaternion.Euler(rotateVelocity * Time.fixedDeltaTime);
        rigid.MoveRotation(rigid.rotation * deltaRotation);
    }

    #region 외부 호출용
    public void RotationStop()
    {
        isRotating = false;
    }

    public void RotationStart()
    {
        isRotating = true;
    }

    public void Accelerate(float value)
    {
        switch (myDir)
        {
            case RotateDirection.Clockwise:
                rotateVelocity.y += value;
                break;
            case RotateDirection.Counter_Clockwise:
                rotateVelocity.y -= value;
                break;
        }
    }
    #endregion
}