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
        // 발판 위로 착지한게 아니라면 패스
        if (coll.transform.position.y < transform.position.y)
            return;

        coll.gameObject.GetComponent<Rigidbody>().AddForce(powVec, ForceMode.Impulse);
    }
}
