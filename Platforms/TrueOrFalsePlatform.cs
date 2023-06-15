using System.Collections;
using UnityEngine;

public class TrueOrFalsePlatform : MonoBehaviour
{
    MeshRenderer myRenderer;
    Animator animator;

    [Header("낙하")]
    [SerializeField] float fallDelay;
    bool isTrue = false;
    public bool IsTrue { get { return isTrue; } }

    [Header("머리티얼 변경")]
    [SerializeField] Material trueMaterial;

    void Awake()
    {
        myRenderer = GetComponent<MeshRenderer>();
        animator = GetComponent<Animator>();
    }

    public void SetTrue()
    {
        isTrue = true;
    }

    void OnCollisionEnter(Collision coll)
    {
        // 플레이어 외 충돌은 패스
        if (!coll.gameObject.CompareTag(Tags.Player))
            return;

        // 플레이어가 위에서 착지한 경우가 아니라면 패스
        if (coll.transform.position.y <= transform.position.y)
            return;

        if(isTrue)
            myRenderer.material = trueMaterial;
        else
            StartCoroutine(Fall());
    }

    IEnumerator Fall()
    {
        animator.SetTrigger("False");
        yield return WfsManager.Instance.GetWaitForSeconds(fallDelay);
        gameObject.SetActive(false);
    }
}