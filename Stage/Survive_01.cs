using UnityEngine;
using System.Collections.Generic;

public class Survive_01 : StageController
{
    [Header("���� ���� ����")]
    [SerializeField] TrueOrFalsePlatform[] tofPlatforms;
    [SerializeField] int column;

    protected override void Initialize()
    {
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
                for (int j = low; j <= high; j++)
                    tofPlatforms[i + j].IsTrue();
                // ť ����
                que.Dequeue();
            }
            // ù ���� ���
            else
                tofPlatforms[i + idx].IsTrue();

            que.Enqueue(idx);
        }
    }
}