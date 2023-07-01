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
        // ���� ���� �����Ѱ� �ƴ϶�� �н�
        if (coll.transform.position.y < transform.position.y)
            return;

        coll.gameObject.GetComponent<Rigidbody>().AddForce(powVec, ForceMode.Impulse);
    }
}
