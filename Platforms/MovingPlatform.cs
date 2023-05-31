using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    Rigidbody rigid;

    [SerializeField] float moveSpeed;
    Vector3 moveVec;
    Vector3 dirVec;

    public enum MoveDirection
    {
        Vertical,
        Horizontal,
        UpperLeft_LowerRight,
        UpperRight_LowerLeft
    }
    [SerializeField] MoveDirection myDir;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // 방향 벡터 초기화
        switch(myDir) 
        {
            case MoveDirection.Vertical:
                dirVec = Vector3.forward;
                break;
            case MoveDirection.Horizontal:
                dirVec = Vector3.right;
                break;
            case MoveDirection.UpperLeft_LowerRight:
                dirVec = new Vector3(1, 0, -1).normalized;
                break;
            case MoveDirection.UpperRight_LowerLeft:
                dirVec = new Vector3(1, 0, 1).normalized;
                break;
        }
    }

    void FixedUpdate()
    {
        moveVec = dirVec * moveSpeed * Time.fixedDeltaTime;
        rigid.MovePosition(transform.position + moveVec);
    }

    public void OnBumped()
    {
        dirVec *= -1;
    }

    // 플레이어에 발판의 이동벡터 전달
    public Vector3 GetMoveVec()
    {
        return moveVec;
    }
}