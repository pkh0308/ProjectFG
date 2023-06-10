using System.Collections;
using UnityEngine;

public class TrueOrFalsePlatform : MonoBehaviour
{
    MeshRenderer myRenderer;
    Animator animator;

    [Header("����")]
    [SerializeField] float fallDelay;
    bool isTrue = false;

    [Header("�Ӹ�Ƽ�� ����")]
    [SerializeField] Material trueMaterial;

    void Awake()
    {
        myRenderer = GetComponent<MeshRenderer>();
        animator = GetComponent<Animator>();
    }

    public void IsTrue()
    {
        isTrue = true;
    }

    void OnCollisionEnter(Collision coll)
    {
        if (!coll.gameObject.CompareTag(Tags.Player))
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