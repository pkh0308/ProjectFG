using UnityEngine;

public class JumpingPlatform : MonoBehaviour
{
    [SerializeField] float jumpPower;
    Vector3 powVec;

    void Start()
    {
        // (0,1,0)�� ���� ���� ������ŭ ȸ�� & jumpPower �ݿ�
        powVec = transform.rotation * Vector3.up * jumpPower;
    }

    void OnCollisionEnter(Collision coll)
    {
        // �÷��̾� �� �н�
        if (!coll.gameObject.CompareTag(Tags.Player))
            return;

        // ���� velocity �ʱ�ȭ �� AddForce
        Rigidbody rigid = coll.gameObject.GetComponent<Rigidbody>();
        rigid.velocity = Vector3.zero;
        rigid.AddForce(powVec, ForceMode.Impulse);
    }
}