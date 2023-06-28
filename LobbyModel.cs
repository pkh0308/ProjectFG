using System.Collections;
using UnityEngine;

using Random = UnityEngine.Random;

public class LobbyModel : MonoBehaviour
{
    Animator animator;

    [SerializeField] float animInterval;
    [SerializeField] string[] animTriggers;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        StartCoroutine(AnimationLoop());
    }

    IEnumerator AnimationLoop()
    {
        bool isIdle = true;

        while(gameObject.activeSelf)
        {
            yield return WfsManager.Instance.GetWaitForSeconds(animInterval);

            // Idle 상태가 아니라면 랜덤 애니메이션 실행
            if (isIdle)
            {
                int idx = Random.Range(0, animTriggers.Length);
                animator.SetTrigger(animTriggers[idx]);
                isIdle = false;
            }
            else
                isIdle = true;
        }
    }
}