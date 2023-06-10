using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    Rigidbody rigid;
    Collider myCol;
    Vector3 initialPos; // 초기 위치 저장용

    [Header("낙하")]
    [SerializeField] float fallDelay;
    bool fallDown = true;

    [Header("재생 관련")]
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
            // 이미 추락중이라면 패스
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
        // 대기 후 추락
        rigid.isKinematic = false;
    }

    // 낙하 지점에 닿으면 비활성화
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(Tags.Fall))
            return;
        
        // 재생용이라면 재생 코루틴 실행
        // 아닐 경우 오브젝트 비활성화
        if(isRestorable)
            StartCoroutine(Restore());
        else
            gameObject.SetActive(false);
    }

    IEnumerator Restore()
    {
        // 투명화 처리
        gameObject.layer = (int)Tags.Layers.IgnoreLaycast;
        
        myCol.enabled = false;
        rigid.isKinematic = true;
        transform.position = initialPos;
        yield return WfsManager.Instance.GetWaitForSeconds(restoreDelay);

        // 발판 설정 원복
        myCol.enabled = true;
        gameObject.layer = (int)Tags.Layers.Default;
    }
}