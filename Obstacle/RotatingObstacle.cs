using UnityEngine;

public class RotatingObstacle : MonoBehaviour
{
    Rigidbody rigid;

    [SerializeField] float rotateDegree;
    [SerializeField] bool isRotating;
    Quaternion deltaRotation;
    Vector3 rotateVelocity;

    // 넉백 관련
    [SerializeField] float knockbackPower;
    Quaternion rotationDir;

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

    void OnCollisionEnter(Collision coll)
    {
        if (!coll.gameObject.CompareTag(Tags.Player))
            return;

        Vector3 forceVec = KnockBack(coll.gameObject.transform.position);
        coll.gameObject.GetComponent<Player>().OnKnockBack(forceVec);
    }

    Vector3 KnockBack(Vector3 targetPos)
    {
        Vector3 dirVec = rotationDir * (transform.position - targetPos); 
        dirVec = dirVec.normalized * knockbackPower;
        return dirVec + (Vector3.up * 2);
    }
}