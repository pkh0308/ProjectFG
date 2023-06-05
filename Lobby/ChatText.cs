using System.Collections;
using UnityEngine;

public class ChatText : MonoBehaviour
{
    [SerializeField] float timeLimit;

    void OnEnable()
    {
        StartCoroutine(Disappear());
    }

    // timeLimit만큼 대기 후 비활성화
    IEnumerator Disappear()
    {
        yield return WfsManager.Instance.GetWaitForSeconds(timeLimit);
        gameObject.SetActive(false);
    }
}