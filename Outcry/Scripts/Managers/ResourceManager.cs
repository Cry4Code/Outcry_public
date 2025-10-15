using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

// --------------- ResourceManager의 자체 참조 카운트가 필요한 이유 ---------------
// ResourceManager는 여러 시스템(StageManager, UIManager 등)의 요청을 받아
// 에셋 하나당 Addressables.Load를 대표로 단 한 번만 호출합니다.
// 따라서 ResourceManager가 자체적으로 자신에게 요청한 고객이 몇 명인지를 세지 않으면
// 한 고객이 Unload를 요청했을 때 다른 고객이 여전히 사용 중임에도 불구하고
// 에셋을 조기에 메모리에서 해제하는 심각한 문제가 발생할 수 있습니다.
// 이 참조 카운트는 그 문제를 해결하기 위한 고객 관리 장부입니다.

public class ResourceManager : Singleton<ResourceManager>
{
    // 로드한 에셋 관리
    private Dictionary<string, Object> assetPool = new Dictionary<string, Object>();

    // Addressable로 로드한 에셋 핸들 관리(메모리 해제용)
    private Dictionary<string, AsyncOperationHandle> addressableHandles = new Dictionary<string, AsyncOperationHandle>();
    // 참조 카운트 관리
    private Dictionary<string, int> refCounts = new Dictionary<string, int>();

    // 동기 Resources 에셋 로드
    public T LoadAsset<T>(string assetName,string path) where T : Object
    {
        T result = default;

        string assetPath = Path.Combine(path, assetName);

        if (!assetPool.ContainsKey(assetPath))
        {
            var asset =  Resources.Load<T>(assetPath);
            if (asset == null) 
            {
                Debug.LogWarning($"{assetPath} 를 불러오기에 실패했습니다.");
                return default(T);
            }

            assetPool.Add(assetPath, asset);
        }

        result = (T)assetPool[assetPath];

        return result;
    }

    // 비동기 Resources 에셋 로드
    public async Task<T> LoadAssetAsync<T>(string assetName, string path) where T : Object
    {
        T result = default;

        string assetPath = Path.Combine(path, assetName);

        if (!assetPool.ContainsKey(assetPath))
        {
            var op = Resources.LoadAsync<T>(assetPath);
            while (!op.isDone)
            {
                await Task.Yield();
            }

            var obj = op.asset;
            if (obj == null)
            {
                Debug.LogWarning($"{assetPath} 를 불러오기에 실패했습니다.");
                return default(T);
            }

            assetPool.Add(assetPath, obj);
        }

        result = (T)assetPool[assetPath];

        return result;
    }

    #region 어드레서블 로드/언로드
    // 비동기 어드레서블 에셋 로드
    // 이미 로드된 에셋은 캐시에서 즉시 반환하며 참조 카운트 1 증가
    public async Task<T> LoadAssetAddressableAsync<T>(string key) where T : Object
    {
        // 이미 로드된 에셋인지 확인 (참조 카운트 확인)
        if (refCounts.TryGetValue(key, out int count))
        {
            refCounts[key]++; // 참조 카운트 1 증가

            // 로딩은 완료되었는지 핸들을 통해 확인
            if (addressableHandles.TryGetValue(key, out var handle) && handle.IsDone)
            {
                return assetPool[key] as T;
            }
            else // 아직 로딩 중인 경우 완료될 때까지 대기
            {
                await addressableHandles[key].Task;
                return assetPool[key] as T;
            }
        }

        // 처음 로드하는 에셋인 경우
        refCounts[key] = 1; // 참조 카운트를 1로 초기화

        var loadHandle = Addressables.LoadAssetAsync<T>(key);
        addressableHandles[key] = loadHandle; // 핸들 저장

        await loadHandle.Task; // 로드가 끝날 때까지 대기

        if (loadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            assetPool[key] = loadHandle.Result; // 캐시에 저장
            return loadHandle.Result;
        }
        else
        {
            Debug.LogError($"[ResourceManager] 에셋 로드 실패: {key}");
            refCounts.Remove(key); // 실패 시 참조 카운트 정보 제거
            addressableHandles.Remove(key);
            Addressables.Release(loadHandle); // 실패한 핸들은 즉시 릴리즈
            return null;
        }
    }

    // 리소스 주소 목록을 받아 모두 로드하는 코루틴
    public IEnumerator LoadAllAssetsCoroutine(List<string> assetKeys)
    {
        // 유효하지 않거나 비어있는 키는 미리 제거하여 안정성 확보
        var validKeys = assetKeys?.Where(key => !string.IsNullOrEmpty(key)).ToList();
        if (validKeys == null || validKeys.Count == 0)
        {
            Debug.Log("[ResourceManager] 로드할 유효한 리소스가 없습니다.");
            yield break;
        }

        Debug.Log($"[ResourceManager] {validKeys.Count}개의 리소스 로드를 시작합니다...");

        // 각 키에 대해 로드 작업을 처리할 핸들 목록 생성
        var handlesToWaitFor = new List<AsyncOperationHandle>();

        foreach (var key in validKeys)
        {
            // 이미 로드되었거나 로딩 중인 에셋인 경우
            if (refCounts.TryGetValue(key, out _))
            {
                refCounts[key]++; // 참조 카운트만 1 증가
                // 진행 중인 로드가 완료될 때까지 기다려야 하므로, 핸들 목록에 추가
                if (addressableHandles.TryGetValue(key, out var existingHandle))
                {
                    handlesToWaitFor.Add(existingHandle);
                }
                continue; // 다음 키로 넘어감
            }

            // 처음 로드하는 에셋인 경우
            refCounts[key] = 1; // 참조 카운트를 1로 초기화
            var newHandle = Addressables.LoadAssetAsync<GameObject>(key); // 타입은 필요에 맞게 변경 가능
            addressableHandles[key] = newHandle; // 핸들 저장
            handlesToWaitFor.Add(newHandle); // 대기 목록에 추가
        }

        // 모든 로드 작업이 완료될 때까지 기다림
        foreach (var handle in handlesToWaitFor)
        {
            yield return handle; // 각 핸들이 완료될 때까지 대기

            // 로드 성공 시에만 assetPool에 결과 저장
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                // 핸들에서 주소(키)를 직접 얻을 수 없으므로 addressableHandles에서 역으로 검색
                string key = addressableHandles.FirstOrDefault(x => x.Value.Equals(handle)).Key;
                if (key != null)
                {
                    assetPool[key] = handle.Result as UnityEngine.Object;
                }
            }
            else
            {
                Debug.LogError($"{handle.DebugName} 리소스 로드 실패!");
            }
        }

        Debug.Log("[ResourceManager] 모든 리소스 로드가 완료되었습니다.");
    }

    // 사용이 끝난 어드레서블 에셋 참조 카운트 1 감소
    // 참조 카운트가 0이 되면 실제 메모리에서 언로드
    public void UnloadAddressableAsset(string key)
    {
        if (!refCounts.ContainsKey(key))
        {
            Debug.LogWarning($"[ResourceManager] 언로드하려는 에셋이 로드된 적 없습니다: {key}");
            return;
        }

        refCounts[key]--; // 참조 카운트 1 감소

        // 참조 카운트가 0 이하가 되면
        if (refCounts[key] <= 0)
        {
            refCounts.Remove(key); // 참조 카운트 제거
            assetPool.Remove(key); // 풀에서 제거

            if (addressableHandles.TryGetValue(key, out AsyncOperationHandle handle))
            {
                Addressables.Release(handle); // 메모리 해제
                addressableHandles.Remove(key); // 핸들 딕셔너리에서 제거
            }
        }
    }

    /// <summary>
    /// 지정된 레이블을 가진 모든 에셋을 비동기적으로 로드합니다.
    /// 내부적으로 각 에셋의 참조 카운트를 개별적으로 관리합니다.
    /// </summary>
    /// <typeparam name="T">로드할 에셋의 타입</typeparam>
    /// <param name="label">로드할 에셋들의 레이블</param>
    /// <returns>성공적으로 로드된 에셋의 리스트</returns>
    public async Task<IList<T>> LoadAssetsByLabelAsync<T>(string label) where T : Object
    {
        // 레이블에 해당하는 모든 리소스의 위치(주소) 정보를 먼저 가져옴
        var locationsHandle = Addressables.LoadResourceLocationsAsync(label, typeof(T));
        IList<IResourceLocation> locations = await locationsHandle.Task;
        if (locations == null || locations.Count == 0)
        {
            Debug.LogWarning($"[ResourceManager] '{label}' 레이블에 해당하는 에셋이 없습니다.");
            Addressables.Release(locationsHandle);
            return new List<T>();
        }

        // 각 위치 정보(주소)를 이용해 개별 에셋을 로드하는 Task 목록 생성
        var loadTasks = new List<Task<T>>();
        foreach (var location in locations)
        {
            // 기존의 개별 에셋 로드 메서드를 호출하여 참조 카운팅 시스템을 그대로 활용
            loadTasks.Add(LoadAssetAddressableAsync<T>(location.PrimaryKey));
        }

        // 위치 정보 핸들은 더 이상 필요 없으므로 즉시 해제
        Addressables.Release(locationsHandle);

        // 모든 개별 에셋 로드가 완료될 때까지 기다림
        T[] loadedAssets = await Task.WhenAll(loadTasks);

        // 로드된 에셋 리스트 반환(null이 포함될 수 있으므로 필터링)
        return loadedAssets.Where(asset => asset != null).ToList();
    }

    /// <summary>
    /// 지정된 레이블을 가진 모든 에셋의 참조 카운트를 1씩 감소시키고 0이 되면 언로드
    /// </summary>
    /// <param name="label">언로드할 에셋들의 레이블</param>
    public async Task UnloadAssetsByLabelAsync(string label)
    {
        // 레이블에 해당하는 모든 리소스의 위치(주소) 정보 가져옴
        var locationsHandle = Addressables.LoadResourceLocationsAsync(label);
        IList<IResourceLocation> locations = await locationsHandle.Task;
        if (locations == null)
        {
            Addressables.Release(locationsHandle);
            return;
        }

        // 각 위치 정보(주소)를 이용해 개별 에셋 언로드
        foreach (var location in locations)
        {
            // 기존 개별 에셋 언로드 메서드를 호출하여 참조 카운팅 시스템 활용
            UnloadAddressableAsset(location.PrimaryKey);
        }

        // 위치 정보 핸들은 더 이상 필요 없으므로 즉시 해제합니다.
        Addressables.Release(locationsHandle);
    }
    #endregion

    /// <summary>
    /// 캐시에서 이미 로드된 리소스를 가져옴
    /// </summary>
    public T GetLoadedAsset<T>(string key) where T : Object
    {
        if (string.IsNullOrEmpty(key) || !assetPool.ContainsKey(key))
        {
            return null;
        }
        return assetPool[key] as T;
    }

    public void ClearResourcePools()
    {
        foreach(var handle in addressableHandles.Values)
        {
            Addressables.Release(handle);
        }

        assetPool.Clear();
        addressableHandles.Clear();

        // Resources 폴더에서 로드된 에셋 중 더 이상 참조되지 않는 것을 언로드
        Resources.UnloadUnusedAssets();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        ClearResourcePools();
    }
}
