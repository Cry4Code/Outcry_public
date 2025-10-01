using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;


public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    //Resources 폴더 내의 경로 + 프리팹 이름을 키로 사용
    private Dictionary<string, IObjectPool<GameObject>> pools = new Dictionary<string, IObjectPool<GameObject>>();
    
    /// <summary>
    /// Pool을 등록하는 메서드
    /// 만약에 Pool에 이미 등록된 path라면 아무 동작도 하지 않는다.s
    /// </summary>
    /// <param name="fullPath">Resources 폴더 내부의 Path + prefab name까지 포함한다. </param>
    /// <param name="defaultCapacity"></param>
    /// <param name="maxSize"></param>
    public async UniTask RegisterPoolAsync(string fullPath, int defaultCapacity = 1, int maxSize = 100)
    {
        if (pools.ContainsKey(fullPath)) return;

        var prefab = await ResourceManager.Instance.LoadAssetAddressableAsync<GameObject>(fullPath);
        if (prefab == null)
        {
            Debug.LogError($"프리팹 {fullPath}을 찾을 수 없다...");
            return;
        }
        
        var pool = new ObjectPool<GameObject>(
            () => Instantiate(prefab),
            obj =>
            {
                obj.gameObject.SetActive(true);
                // var initializable = obj.GetComponent<IInitializable>(); //사용 후 초기화가 필요한 컴포넌트가 있다면 초기화 메서드 호출
                // initializable?.Initialize();
            },
            obj => obj.gameObject.SetActive(false),
            obj => Destroy(obj),
            false, defaultCapacity, maxSize
        );

        pools[fullPath] = pool;
    }

    /// <summary>
    /// pool에 등록된 path가 없다면 자동으로 등록 후 오브젝트를 반환한다. (defaultCapacity = 1, maxSize = 100)
    /// </summary>
    /// <param name="fullPath">Resources 폴더 내부의 Path + prefab name까지 포함한다. </param>
    /// <param name="parent">지정할 경우 해당 객체 하위에 생성된다. </param>
    /// <param name="position"></param>
    /// <returns></returns>
    public async UniTask<GameObject> GetObjectAsync(string fullPath, Transform parent = null, Vector3 position = default(Vector3))
    {
        if (!pools.ContainsKey(fullPath))
        {
            await RegisterPoolAsync(fullPath);
        }
        var obj = pools[fullPath].Get();
        Debug.Log($"[ObjectPool] GetObject: {fullPath}, obj: {obj}, id: {obj.GetInstanceID()}, activeSelf: {obj.activeSelf}");
        if(parent != null)
        {
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position;
        }
        else
        {
            obj.transform.position = position;
        }
        return obj;
    }

    /// <summary>
    /// 등록된 풀에서만 오브젝트를 가져올 수 있습니다.
    /// parent가 null일 경우 월드 좌표에 생성됩니다.
    /// 
    /// </summary>
    /// <param name="fullPath"></param>
    /// <param name="parent"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public GameObject GetObject(string fullPath, Transform parent = null, Vector3 position = default(Vector3))
    {
        if (!pools.ContainsKey(fullPath))
        {
            Debug.LogError($"풀에 등록되지 않은 경로입니다: {fullPath}");
            return null;
        }

        var obj = pools[fullPath].Get();
        
        if (parent != null)
        {
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position;
        }
        else
        {
            obj.transform.position = position;
        }
        
        Debug.Log($"[ObjectPool] GetObject: {fullPath}, obj: {obj}, id: {obj.GetInstanceID()}, activeSelf: {obj.activeSelf}");

        return obj;
    }
    
    public void ReleaseObject(string fullPath, GameObject obj)
    {
        Debug.Log($"[ObjectPool] ReleaseObject: {fullPath}, obj: {obj}, id: {obj.GetInstanceID()}, activeSelf: {obj.activeSelf}");
        if (pools.TryGetValue(fullPath, out var pool))
        {
            pool.Release(obj);
            Debug.Log($"[ObjectPool] Release 후 activeSelf: {obj.activeSelf}, id: {obj.GetInstanceID()}");
            if (pool is ObjectPool<GameObject> objectPool)
                Debug.Log($"[ObjectPool] 현재 풀 비활성 오브젝트 수: {objectPool.CountInactive}");
        }
    }
    
    public void ClearAllPools()
    {
        // foreach 루프 중에 Dictionary를 수정하면 오류가 발생하므로 키 목록 복사해서 사용
        List<string> keys = new List<string>(pools.Keys);
        foreach (var fullPath in keys)
        {
            ClearPoolByKey(fullPath);
        }
    }

    public void ClearPoolByKey(string fullPath)
    {
        if (pools.TryGetValue(fullPath, out var pool))
        {
            pool.Clear(); // 풀에 있는 모든 인스턴스 파괴
            pools.Remove(fullPath);

            // ResourceManager에 반납 요청
            ResourceManager.Instance.UnloadAddressableAsset(fullPath);
        }
    }
}

