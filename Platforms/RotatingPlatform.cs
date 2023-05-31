using UnityEngine;

// ȸ�� ���ǿ� Ŭ����
// �ð���� / �ݽð����
public class RotatingPlatform : MonoBehaviour
{
    Rigidbody rigid;

    [SerializeField] float rotateDegree;
    Quaternion deltaRotation;
    Vector3 rotateVelocity;

    // �÷��̾� ȸ���� ����
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
        // ���� ���� �ʱ�ȭ
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

    // �÷��̾� ȸ���� �Լ�
    // (�÷��̾� -> ȸ�� ����) ������ ���͸� 90/-90�� ȸ����Ų ���� ��ȯ
    public Vector3 GetRotateVec(Vector3 playerPos)
    {
        dirVec = rotationDir * (transform.position - playerPos);
        dirVec = dirVec.normalized * 2 * Mathf.PI * dirVec.magnitude / (360 / rotateDegree);
        return dirVec * Time.fixedDeltaTime;
    }
}
