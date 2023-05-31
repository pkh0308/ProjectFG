using System.Collections.Generic;
using UnityEngine;

// WaitForSeconds ������ Ŭ����
// �ش� Ŭ������ ���� �̹� ������ WaitForSeconds ��ü ��ȯ
// ���� �������� �ʴ� �ð��� WaitForSeconds�� ��ü ���� ����
public class WfsManager
{
    Dictionary<float, WaitForSeconds> secondsDic;

    #region �̱��� ����
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
        // �ش� �ð��� WaitForSeconds�� ���� ���
        if (secondsDic.TryGetValue(time, out value) == false)
            secondsDic.Add(time, value = new WaitForSeconds(time));

        return value;
    }
}