using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

public class Survive_02 : StageController
{
    [Header("막대 회전")]
    [SerializeField] RotatingObstacle lowerStick;
    [SerializeField] RotatingObstacle upperStick;
    [SerializeField] float acclerateInterval;
    [SerializeField] float acclerateDegree;

    [Header("발판 추락")]
    [SerializeField] DelayFallPlatform[] delayFallPlatforms;
    [SerializeField] float fallDownInterval;
    int count;

    protected override void Initialize_Multi()
    {
        count = delayFallPlatforms.Length;
        StartCoroutine(FallDown());
    }

    // 스틱 회전 시작 및 가속 코루틴 호출
    protected override void OnGameStart()
    {
        lowerStick.RotationStart(); Debug.Log("OnGameStart");
        upperStick.RotationStart();
        StartCoroutine(AccelerateLowerStick());
    }
    // 스틱 회전 정지
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

        // fallDownInterval 만큼 대기 후 떨어트릴 플랫폼 지정
        // 최소 2개는 남겨두도록 설정
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