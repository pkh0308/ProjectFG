using System.Collections;
using UnityEngine;

public class DelayFallPlatform : MonoBehaviour
{
    Rigidbody rigid;
    Collider myCol;

    [Header("�߶�")]
    [SerializeField] float fallDelay;
    bool fallReady;

    [Header("��鸲")]
    [SerializeField] float speed;
    [SerializeField] float amount;
    Vector3 posVec;
    float initialY;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        myCol = GetComponent<Collider>();
    }

    void Start()
    {
        initialY = transform.position.y;
    }

    void Update()
    {
        if (!fallReady) return;

        posVec = transform.position;
        posVec.y = initialY + Mathf.Sin(Time.time * speed) * amount;
        transform.position = posVec;
    }

    public void StartFall()
    {
        StartCoroutine(Fall());
    }

    IEnumerator Fall()
    {
        fallReady = true;
        yield return WfsManager.Instance.GetWaitForSeconds(fallDelay);

        // ��� �� �߶�
        fallReady = false;
        rigid.isKinematic = false;
        myCol.enabled = false;
    }

    // �ƿ� ������ ������ ��Ȱ��ȭ
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(Tags.OutArea))
            return;

        gameObject.SetActive(false);
    }
}