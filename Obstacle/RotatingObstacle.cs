using UnityEngine;

public class RotatingObstacle : MonoBehaviour
{
    Rigidbody rigid;

    [SerializeField] float rotateDegree;
    Quaternion deltaRotation;
    Vector3 rotateVelocity;

    //// 플레이어 넉백용 변수
    //[SerializeField] float knuckbackForce;
    //Quaternion rotationDir;

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
                //rotationDir = Quaternion.Euler(0, -90, 0);
                break;
            case RotateDirection.Counter_Clockwise:
                rotateVelocity = new Vector3(0, -rotateDegree, 0);
                //rotationDir = Quaternion.Euler(0, 90, 0);
                break;
        }
    }

    void FixedUpdate()
    {
        deltaRotation = Quaternion.Euler(rotateVelocity * Time.fixedDeltaTime);
        rigid.MoveRotation(rigid.rotation * deltaRotation);
    }

    //// 플레이어 넉백용 forceVector 반환
    //// (플레이어 -> 회전 장애물) 방향의 벡터를 90/-90도 회전시킨 값을 반환
    //public Vector3 GetForceVec(Vector3 playerPos)
    //{
    //    Vector3 forceVec = rotationDir * (striker.position - playerPos); 
    //    forceVec.y = 0.05f; 
    //    forceVec *= knuckbackForce;
    //    return forceVec;
    //}

    //void OnTriggerEnter(Collider coll)
    //{
    //    if (coll.CompareTag(Tags.Player) == false)
    //        return;

    //    Vector3 forceVec = GetForceVec(coll.transform.position);
    //    coll.GetComponent<Player>().OnKnuckBack(forceVec);
    //}
}