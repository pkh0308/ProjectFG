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

            // Idle ���°� �ƴ϶�� ���� �ִϸ��̼� ����
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