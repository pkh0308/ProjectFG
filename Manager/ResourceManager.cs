using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using Object = UnityEngine.Object;

public class ResourceManager
{
    Dictionary<string, Object> resources = new Dictionary<string, Object>();
    Dictionary<string, AsyncOperationHandle> handles = new Dictionary<string, AsyncOperationHandle>();
    public int OnGoingHandles { get; private set; }

    #region 싱글톤 구현
    private static ResourceManager instance;
    public static ResourceManager Instance
    {
        get
        {
            if (instance == null)
                instance = new ResourceManager();

            return instance;
        }
    }
    #endregion

    #region 리소스 로드
    // 리소스 로드 함수
    // key 리소스가 존재한다면 바로 리턴, 로드중이라면 Completed에 추가
    // 처음 요청됐다면 로드 시작
    public void GetResource<T>(string key, Action<T> callback) where T : Object
    {
        if(resources.TryGetValue(key, out Object resource))
        {
            callback?.Invoke(resource as T);
            return;
        }

        if(handles.TryGetValue(key, out AsyncOperationHandle handle))
        {
            handle.Completed += (op) => { callback?.Invoke(op.Result as T); };
            return;
        }

        handles.Add(key, Addressables.LoadAssetAsync<T>(key));
        OnGoingHandles++;
        handles[key].Completed += (op) =>
        {
            resources.Add(key, op.Result as T);
            callback?.Invoke(op.Result as T);
            OnGoingHandles--;
        };
    }


    #endregion
}
