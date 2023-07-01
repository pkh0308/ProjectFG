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
        // �ִϸ������� �Ķ���͵� �̸� �����صα�
        AnimatorControllerParameter[] parameters = animator.parameters;
        animTriggers = new string[parameters.Length];

        for (int i = 0; i < parameters.Length; ++i)
        {
            animTriggers[i] = parameters[i].name;
        }
    }

    public void RandomAnim()
    {
        // ���� �ִϸ��̼��� Idle�϶��� ����
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(idle))
        {
            int idx = Random.Range(0, animTriggers.Length);
            animator.SetTrigger(animTriggers[idx]);
        } 
    }
}