using UnityEngine;

public class CameraMove : MonoBehaviour
{
    Transform target;
    [SerializeField] Vector3 offset;

    void FixedUpdate()
    {
        if (target == null) return;

        transform.position = target.position + offset;
    }

    public void SetTarget(Transform t)
    {
        target = t;
    }
}