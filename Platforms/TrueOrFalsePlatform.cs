using System.Collections;
using UnityEngine;

public class TrueOrFalsePlatform : MonoBehaviour
{
    MeshRenderer myRenderer;
    Animator animator;

    [Header("����")]
    [SerializeField] float fallDelay;
    bool isTrue = false;
    public bool IsTrue { get { return isTrue; } }

    [Header("�Ӹ�Ƽ�� ����")]
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
        // �÷��̾� �� �浹�� �н�
        if (!coll.gameObject.CompareTag(Tags.Player))
            return;

        // �÷��̾ ������ ������ ��찡 �ƴ϶�� �н�
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