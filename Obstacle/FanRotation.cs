using UnityEngine;

public class FanRotation : MonoBehaviour
{
    [SerializeField] Vector3 rotateVec;

    void FixedUpdate()
    {
        transform.Rotate(rotateVec * Time.fixedDeltaTime);
    }
}
