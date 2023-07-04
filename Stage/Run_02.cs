using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

// Run_02 ��������
// TrueOrFalse ���� ����
// ��Ƽ �÷����� ��� TrueOrFalse ������ RPC�� �����Ͽ� ���� ����ȭ
public class Run_02 : StageController
{
    [Header("���� ���� ����")]
    [SerializeField] TrueOrFalsePlatform[] tofPlatforms;
    [SerializeField] int column;

    protected override void Initialize_Single()
    {
        // ���� ��Ʈ ����
        // 5���� ���� �� �ϳ��� true �������� ����
        // ���� ���� 1ĭ �� ���ǰ� ���� ������ ���� ���̴� true �������� ����
        Queue<int> que = new Queue<int>();
        for (int i = 0; i < tofPlatforms.Length; i += column)
        {
            int idx = Random.Range(0, 5);

            // ù���� �ƴ� ���
            if (que.Count > 0)
            {
                int high = que.Peek() > idx ? que.Peek() : idx;
                int low = que.Peek() < idx ? que.Peek() : idx;
                for (int j = low; j <= high; j++)
                    tofPlatforms[i + j].SetTrue();
                // ť ����
                que.Dequeue();
            }
            // ù ���� ���
            else
                tofPlatforms[i + idx].SetTrue();

            que.Enqueue(idx);
        }
    }

    protected override void Initialize_Multi()
    {
        // ������ Ŭ���̾�Ʈ������ ����
        // ���� ���� �κ��� RPC�� �����Ͽ� �ٸ� Ŭ���̾�Ʈ��� ����ȭ
        if (NetworkManager.Instance.IsMaster == false)
            return;

        // ���� ��Ʈ ����
        // 5���� ���� �� �ϳ��� true �������� ����
        // ���� ���� 1ĭ �� ���ǰ� ���� ������ ���� ���̴� true �������� ����
        Queue<int> que = new Queue<int>();
        for(int i = 0; i < tofPlatforms.Length; i += column) 
        {
            int idx = Random.Range(0, 5);

            // ù���� �ƴ� ���
            if(que.Count > 0)
            {
                int high = que.Peek() > idx ? que.Peek() : idx;
                int low = que.Peek() < idx ? que.Peek() : idx;
                PV.RPC(nameof(SetTrue), RpcTarget.All, i, low, high);
                // ť ����
                que.Dequeue();
            }
            // ù ���� ���
            else
                PV.RPC(nameof(SetTrue), RpcTarget.All, i, idx, idx);

            que.Enqueue(idx);
        }
    }

    // ��¥ �������� ����
    // ��� Ŭ���̾�Ʈ���� ����ȭ�ǵ��� RPC�� ����
    [PunRPC]
    void SetTrue(int idx, int low, int high)
    {
        for (int i = low; i <= high; i++)
            tofPlatforms[idx + i].SetTrue();
    }
}