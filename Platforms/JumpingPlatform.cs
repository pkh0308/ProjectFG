using UnityEngine;

public class JumpingPlatform : MonoBehaviour
{
    [SerializeField] float jumpPower;
    Vector3 powVec;

    void Start()
    {
        // (0,1,0)을 현재 발판 각도만큼 회전 & jumpPower 반영
        powVec = transform.rotation * Vector3.up * jumpPower;
    }

    void OnCollisionEnter(Collision coll)
    {
        // 플레이어 외 패스
        if (!coll.gameObject.CompareTag(Tags.Player))
            return;

        // 기존 velocity 초기화 후 AddForce
        Rigidbody rigid = coll.gameObject.GetComponent<Rigidbody>();
        rigid.velocity = Vector3.zero;
        rigid.AddForce(powVec, ForceMode.Impulse);
    }
}