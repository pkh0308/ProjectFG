using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMove : MonoBehaviour
{
    Transform target;

    // ī�޶� ȸ�� ����
    [Header("ī�޶� ������")]
    [SerializeField] Transform cameraOffset;
    [SerializeField] Transform cameraRef;

    [Header("ī�޶� ȸ�� �ӵ�")]
    [SerializeField] float hRotateSpeed;
    [SerializeField] float vRotateSpeed;

    [Header("�ִ� ���� ����")]
    [Range(10, 60)]
    [SerializeField] float rotateLimitMax;
    [Range(10, 60)]
    [SerializeField] float rotateLimitMin;

    Vector2 curLookVec;
    Vector2 befLookVec;
    float xOffset;
    float yOffset;
    bool canRotate = true;

    // �ܺ� ȣ��� �Լ�
    // ���� ���(Ÿ��) ����
    public void SetTarget(Transform t)
    {
        target = t;
    }

    #region ī�޶� ȸ��
    // �ܺ� ȣ���(StageController)
    // �ʱ� ���� �������� ī�޶� �� ������ ȸ��(y��)
    public void SetRotation(float y)
    {
        // y�� ���� ȸ��
        transform.RotateAround(target.position, Vector3.up, y); 
        cameraOffset.RotateAround(Vector3.zero, Vector3.up, y);
        cameraRef.rotation = transform.rotation;

        // z�� ȸ�� ����
        if (transform.eulerAngles.z != 0)
        {
            transform.rotation =
                Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 0);
        }
        GameManager.Instance.SetCamAngle(transform.eulerAngles.y);
    }

    // ���콺 �����ǿ� ���� ī�޶� ȸ��
    void OnLook(InputValue value)
    {
        // Ÿ���� ���ų� ȸ�� �Ұ����� ��� �н�
        if (target == null || !canRotate) return;

        curLookVec = value.Get<Vector2>();
        // ����� ���� ���� ���ٸ� �����ϰ� �н�
        if (befLookVec == Vector2.zero) 
        {
            befLookVec = curLookVec;
            return;
        }

        xOffset = (curLookVec.x - befLookVec.x) * vRotateSpeed * Time.deltaTime;
        yOffset = (curLookVec.y - befLookVec.y) * hRotateSpeed * Time.deltaTime;

        cameraRef.position = transform.position;
        // x�� ���� ȸ��
        // ���� üũ�� ���� ���� �׽�Ʈ�� Ʈ�������� ���� ȸ��
        cameraRef.RotateAround(target.position, Vector3.right, yOffset);
        // ���� ���� ��쿡�� ȸ�� ����
        if (cameraRef.eulerAngles.x > rotateLimitMin && cameraRef.eulerAngles.x < rotateLimitMax)
        {
            transform.RotateAround(target.position, Vector3.right, yOffset);
            cameraOffset.RotateAround(Vector3.zero, Vector3.right, yOffset);
        }
        cameraRef.rotation = transform.rotation;

        // y�� ���� ȸ��
        transform.RotateAround(target.position, Vector3.up, xOffset);
        cameraOffset.RotateAround(Vector3.zero, Vector3.up, xOffset);

        // rigidbody�� �̿��� roateAround
        //Quaternion q = Quaternion.AngleAxis(orbitSpeed, transform.forward);
        //rb.MovePosition(q * (rb.transform.position - target.position) + target.position);
        //rb.MoveRotation(rb.transform.rotation * q);

        // z�� ȸ�� ����
        if (transform.eulerAngles.z != 0)
        {
            transform.rotation =
                Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 0);
        }

        GameManager.Instance.SetCamAngle(transform.eulerAngles.y);

        // ���� ���� ���� ������ ���� 
        befLookVec = curLookVec;
    }

    void FixedUpdate()
    {
        if (target == null) return;

        transform.position = target.position + cameraOffset.position;
    }
    #endregion

    void OnEscape()
    {
        canRotate = UiController.Instance.SetExitPopUp();
    }
}