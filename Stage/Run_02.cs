using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

// Run_02 스테이지
// TrueOrFalse 발판 설정
// 멀티 플레이일 경우 TrueOrFalse 설정은 RPC로 실행하여 정보 동기화
public class Run_02 : StageController
{
    [Header("랜덤 발판 설정")]
    [SerializeField] TrueOrFalsePlatform[] tofPlatforms;
    [SerializeField] int column;

    protected override void Initialize_Single()
    {
        // 랜덤 루트 설정
        // 5개의 발판 중 하나는 true 발판으로 설정
        // 이전 발판 1칸 앞 발판과 새로 지정한 발판 사이는 true 발판으로 설정
        Queue<int> que = new Queue<int>();
        for (int i = 0; i < tofPlatforms.Length; i += column)
        {
            int idx = Random.Range(0, 5);

            // 첫행이 아닐 경우
            if (que.Count > 0)
            {
                int high = que.Peek() > idx ? que.Peek() : idx;
                int low = que.Peek() < idx ? que.Peek() : idx;
                for (int j = low; j <= high; j++)
                    tofPlatforms[i + j].SetTrue();
                // 큐 비우기
                que.Dequeue();
            }
            // 첫 행일 경우
            else
                tofPlatforms[i + idx].SetTrue();

            que.Enqueue(idx);
        }
    }

    protected override void Initialize_Multi()
    {
        // 마스터 클라이언트에서만 실행
        // 발판 설정 부분은 RPC로 실행하여 다른 클라이언트들과 동기화
        if (NetworkManager.Instance.IsMaster == false)
            return;

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
                PV.RPC(nameof(SetTrue), RpcTarget.All, i, low, high);
                // 큐 비우기
                que.Dequeue();
            }
            // 첫 행일 경우
            else
                PV.RPC(nameof(SetTrue), RpcTarget.All, i, idx, idx);

            que.Enqueue(idx);
        }
    }

    // 진짜 발판으로 설정
    // 모든 클라이언트에서 동기화되도록 RPC로 실행
    [PunRPC]
    void SetTrue(int idx, int low, int high)
    {
        for (int i = low; i <= high; i++)
            tofPlatforms[idx + i].SetTrue();
    }
}