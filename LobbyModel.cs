using UnityEngine;

using Random = UnityEngine.Random;

public class LobbyModel : MonoBehaviour
{
    Animator animator;

    [SerializeField] float animInterval;
    string[] animTriggers;
    string idle = "Idle";

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        // 애니메이터의 파라미터들 이름 저장해두기
        AnimatorControllerParameter[] parameters = animator.parameters;
        animTriggers = new string[parameters.Length];

        for (int i = 0; i < parameters.Length; ++i)
        {
            animTriggers[i] = parameters[i].name;
        }
    }

    public void RandomAnim()
    {
        // 현재 애니메이션이 Idle일때만 실행
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(idle))
        {
            int idx = Random.Range(0, animTriggers.Length);
            animator.SetTrigger(animTriggers[idx]);
        } 
    }
}