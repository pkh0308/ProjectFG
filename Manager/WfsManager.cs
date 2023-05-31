using System.Collections.Generic;
using UnityEngine;

// WaitForSeconds 관리용 클래스
// 해당 클래스를 통해 이미 생성한 WaitForSeconds 객체 반환
// 아직 존재하지 않는 시간의 WaitForSeconds만 객체 새로 생성
public class WfsManager
{
    Dictionary<float, WaitForSeconds> secondsDic;

    #region 싱글톤 구현
    private WfsManager()
    {
        secondsDic = new Dictionary<float, WaitForSeconds>();
    }

    private static WfsManager instance;
    public static WfsManager Instance
    {
        get
        {
            if (instance == null)
                instance = new WfsManager();

            return instance;
        }
    }
    #endregion

    public WaitForSeconds GetWaitForSeconds(float time)
    {
        WaitForSeconds value;
        // 해당 시간의 WaitForSeconds가 있을 경우
        if (secondsDic.TryGetValue(time, out value) == false)
            secondsDic.Add(time, value = new WaitForSeconds(time));

        return value;
    }
}