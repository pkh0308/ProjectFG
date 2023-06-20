using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMove : MonoBehaviour
{
    Transform target;

    // ī�޶� ȸ�� ����
    [Header("ī�޶� ���� ���")]
    [SerializeField] Transform cameraOffset;

    [Header("ī�޶� ȸ�� �ӵ�")]
    [SerializeField] float hRotateSpeed;
    [SerializeField] float vRotateSpeed;

    [Header("�ִ� ���� ����")]
    [Range(10, 60)]
    [SerializeField] float rotLimitMin;
    [Range(10, 60)]
    [SerializeField] float rotLimitMax;

    Vector2 curLookVec;
    Vector2 befLookVec;
    float xOffset;
    float yOffset;
    float sinRes;
    float cosRes;

    bool rDown;

    // �ܺ� ȣ��� �Լ�
    // ���� ���(Ÿ��) ����
    public void SetTarget(Transform t)
    {
        target = t;
    }

    void OnRightDown()
    {
        rDown = true;
    }

    void OnRightUp()
    {
        rDown = false;
        befLookVec = Vector2.zero;
    }

    void OnLook(InputValue value)
    {
        // Ÿ���� ���ų� ���콺 ������ Ŭ������ �ƴ϶�� �н�
        if (target == null || rDown == false) return;

        curLookVec = value.Get<Vector2>();
        // ����� ���� ���� ���ٸ� �����ϰ� �н�
        if (befLookVec == Vector2.zero) 
        {
            befLookVec = curLookVec;
            return;
        }

        sinRes = Mathf.Sin(transform.eulerAngles.y * Mathf.Deg2Rad);
        cosRes = Mathf.Cos(transform.eulerAngles.y * Mathf.Deg2Rad);

        xOffset = (curLookVec.x - befLookVec.x) * hRotateSpeed * Time.deltaTime;
        yOffset = (curLookVec.y - befLookVec.y) * hRotateSpeed * Time.deltaTime;

        transform.RotateAround(target.position, Vector3.up, xOffset);
        transform.RotateAround(target.position, Vector3.right, -yOffset * cosRes);
        transform.RotateAround(target.position, Vector3.forward, yOffset * sinRes);
        // �ִ�/�ּ� ���� ����
        if (transform.eulerAngles.x > rotLimitMax)
        {
            transform.rotation = Quaternion.Euler(rotLimitMax - 0.1f, transform.eulerAngles.y, 0);
            return;
        }
        if (transform.eulerAngles.x < rotLimitMin)
        {
            transform.rotation = Quaternion.Euler(rotLimitMin + 0.1f, transform.eulerAngles.y, 0);
            return;
        }

        // �����µ� �����ϰ� ȸ��
        cameraOffset.RotateAround(Vector3.zero, Vector3.up, xOffset);
        cameraOffset.RotateAround(Vector3.zero, Vector3.right, -yOffset * cosRes);
        cameraOffset.RotateAround(Vector3.zero, Vector3.forward, yOffset * sinRes);

        // ���� ���� ���� ������ ����
        befLookVec = curLookVec;
    }

    void FixedUpdate()
    {
        if (target == null) return;

        transform.position = target.position + cameraOffset.position;

        if (transform.eulerAngles.z != 0)
        {
            transform.rotation = 
                Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 0);
        }
    }
}