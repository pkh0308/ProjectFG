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

        // fallDownInterval 만큼 대기 후 떨어트릴 플랫폼 지정
        // 최소 2개는 남겨두도록 설정
        int randIdx;
        while (count > 2)
        {
            yield return WfsManager.Instance.GetWaitForSeconds(fallDownInterval);
            randIdx = Random.Range(0, list.Count);
            // ToDo : 애니메이션 설정
            yield return WfsManager.Instance.GetWaitForSeconds(1.0f);
            // ToDo : 추락 실행

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