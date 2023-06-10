using UnityEngine;
using System.Collections.Generic;

public class Survive_01 : StageController
{
    [Header("랜덤 발판 설정")]
    [SerializeField] TrueOrFalsePlatform[] tofPlatforms;
    [SerializeField] int column;

    protected override void Initialize()
    {
        // 랜덤 루트 설정
        // 5개의 발판 중 하나는 true 발판으로 설정
        // 이전 발판 1칸 앞 발판과 새로 지정한 발판 사이는 true 발판으로 설정
        Queue<int> que = new Queue<int>();
        for(int i = 0; i < tofPlatforms.Length; i += column) 
        {
            int idx = Random.Range(0, 5);

            // 첫행이 아닐 경우
            if(que.Count > 0)
            {
                int high = que.Peek() > idx ? que.Peek() : idx;
                int low = que.Peek() < idx ? que.Peek() : idx;
                for (int j = low; j <= high; j++)
                    tofPlatforms[i + j].IsTrue();
                // 큐 비우기
                que.Dequeue();
            }
            // 첫 행일 경우
            else
                tofPlatforms[i + idx].IsTrue();

            que.Enqueue(idx);
        }
    }
}