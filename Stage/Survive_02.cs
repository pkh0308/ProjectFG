using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

public class Survive_02 : StageController
{
    [Header("���� ȸ��")]
    [SerializeField] RotatingObstacle lowerStick;
    [SerializeField] RotatingObstacle upperStick;
    [SerializeField] float acclerateInterval;
    [SerializeField] float acclerateDegree;

    [Header("���� �߶�")]
    [SerializeField] GameObject[] platforms;
    [SerializeField] float fallDownInterval;
    int count;

    protected override void Initialize_Multi()
    {
        count = platforms.Length;
        StartCoroutine(FallDown());
    }

    protected override void OnGameStart()
    {
        lowerStick.RotationStart();
        upperStick.RotationStart();
        StartCoroutine(AccelerateLowerStick());
    }

    IEnumerator FallDown()
    {
        List<int> list = new List<int>(platforms.Length);
        for(int i = 0; i < platforms.Length; i++)
            list.Add(i);

        // fallDownInterval ��ŭ ��� �� ����Ʈ�� �÷��� ����
        // �ּ� 2���� ���ܵε��� ����
        int randIdx;
        while (count > 2)
        {
            yield return WfsManager.Instance.GetWaitForSeconds(fallDownInterval);
            randIdx = Random.Range(0, list.Count);
            // ToDo : �ִϸ��̼� ����
            yield return WfsManager.Instance.GetWaitForSeconds(1.0f);
            // ToDo : �߶� ����

            count--;
        }
    }

    IEnumerator AccelerateLowerStick()
    {
        while(!GameManager.Instance.IsPaused)
        {
            yield return WfsManager.Instance.GetWaitForSeconds(acclerateInterval);
            lowerStick.Accelerate(acclerateDegree);
        }
    }
}