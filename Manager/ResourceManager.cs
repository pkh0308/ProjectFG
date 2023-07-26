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

    #region �̱��� ����
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

    #region ���ҽ� �ε�
    // ���ҽ� �ε� �Լ�
    // key ���ҽ��� �����Ѵٸ� �ٷ� ����, �ε����̶�� Completed�� �߰�
    // ó�� ��û�ƴٸ� �ε� ����
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
