using UnityEngine;

public class ResultModel : MonoBehaviour
{
    Animator animator;
    enum AnimationVar
    {
        doVictory,
        doLoose
    }

    public void SetAnimation(bool isWinner)
    {
        animator = GetComponent<Animator>();

        if (isWinner)
            animator.SetTrigger(AnimationVar.doVictory.ToString());
        else
            animator.SetTrigger(AnimationVar.doLoose.ToString());
    }
}