using UnityEngine;

// �̵� ������ �̵� ���� ����
// ���۰� �ٸ� �÷����� ������ OnBumped() ȣ��
public class Bumper : MonoBehaviour
{
    [SerializeField] MovingPlatform m_platform;

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag(Tags.Platform))
        {
            m_platform.OnBumped();
        }
    }
}