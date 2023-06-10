using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    Rigidbody rigid;
    Collider myCol;
    Vector3 initialPos; // �ʱ� ��ġ �����

    [Header("����")]
    [SerializeField] float fallDelay;
    bool fallDown = true;

    [Header("��� ����")]
    [SerializeField] bool isRestorable;
    [SerializeField] float restoreDelay;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        myCol = GetComponent<Collider>();
    }

    void Start()
    {
        initialPos = transform.position;
    }

    void OnCollisionEnter(Collision coll)
    {
        if (!fallDown) return;

        if(coll.gameObject.CompareTag(Tags.Player)) 
        {
            // �̹� �߶����̶�� �н�
            if (rigid.isKinematic == false) 
                return;

            StartCoroutine(Fall());
            return;
        }
    }

    public void NotFall()
    {
        fallDown = false;
    }

    IEnumerator Fall()
    {
        yield return WfsManager.Instance.GetWaitForSeconds(fallDelay);
        // ��� �� �߶�
        rigid.isKinematic = false;
    }

    // ���� ������ ������ ��Ȱ��ȭ
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(Tags.Fall))
            return;
        
        // ������̶�� ��� �ڷ�ƾ ����
        // �ƴ� ��� ������Ʈ ��Ȱ��ȭ
        if(isRestorable)
            StartCoroutine(Restore());
        else
            gameObject.SetActive(false);
    }

    IEnumerator Restore()
    {
        // ����ȭ ó��
        gameObject.layer = (int)Tags.Layers.IgnoreLaycast;
        
        myCol.enabled = false;
        rigid.isKinematic = true;
        transform.position = initialPos;
        yield return WfsManager.Instance.GetWaitForSeconds(restoreDelay);

        // ���� ���� ����
        myCol.enabled = true;
        gameObject.layer = (int)Tags.Layers.Default;
    }
}