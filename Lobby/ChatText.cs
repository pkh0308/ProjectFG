using System.Collections;
using UnityEngine;

public class ChatText : MonoBehaviour
{
    [SerializeField] float timeLimit;

    void OnEnable()
    {
        StartCoroutine(Disappear());
    }

    // timeLimit��ŭ ��� �� ��Ȱ��ȭ
    IEnumerator Disappear()
    {
        yield return WfsManager.Instance.GetWaitForSeconds(timeLimit);
        gameObject.SetActive(false);
    }
}