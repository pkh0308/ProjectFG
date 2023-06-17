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
    [SerializeField] DelayFallPlatform[] delayFallPlatforms;
    [SerializeField] float fallDownInterval;
    int count;

    protected override void Initialize_Multi()
    {
        count = delayFallPlatforms.Length;
        StartCoroutine(FallDown());
    }

    // ��ƽ ȸ�� ���� �� ���� �ڷ�ƾ ȣ��
    protected override void OnGameStart()
    {
        lowerStick.RotationStart(); Debug.Log("OnGameStart");
        upperStick.RotationStart();
        StartCoroutine(AccelerateLowerStick());
    }
    // ��ƽ ȸ�� ����
    protected override void OnGameStop()
    {
        lowerStick.RotationStop(); Debug.Log("OnGameStop");
        upperStick.RotationStop();
    }

    IEnumerator FallDown()
    {
        List<int> list = new List<int>(delayFallPlatforms.Length);
        for(int i = 0; i < delayFallPlatforms.Length; i++)
            list.Add(i);

        // fallDownInterval ��ŭ ��� �� ����Ʈ�� �÷��� ����
        // �ּ� 2���� ���ܵε��� ����
        int randIdx;
        while (count > 2)
        {
            yield return WfsManager.Instance.GetWaitForSeconds(fallDownInterval);
            randIdx = Random.Range(0, list.Count);
            delayFallPlatforms[randIdx].StartFall();
            list.Remove(randIdx);

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