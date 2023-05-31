using UnityEngine;

// 이동 발판의 이동 범위 조절
// 범퍼가 다른 플랫폼에 닿으면 OnBumped() 호출
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