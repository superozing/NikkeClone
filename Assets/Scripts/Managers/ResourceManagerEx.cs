using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ResourceManagerEx : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.Resource;

    private readonly Dictionary<string, AsyncOperationHandle<GameObject>> _prefabHandles = new();

    public void Init()
    {
        Debug.Log($"{ManagerType} Manager Init 합니다.");
    }

    public void Update() { }

    public void Clear()
    {
        // 씬 전환 시 로드했던 모든 프리팹을 메모리에서 해제
        foreach (var handle in _prefabHandles.Values)
            Addressables.Release(handle);

        _prefabHandles.Clear();

        Debug.Log($"{ManagerType} Manager Clear 합니다.");
    }

    /// <summary>
    /// 프리팹을 비동기적으로 인스턴스화합니다. 내부적으로 PoolManager를 사용합니다.
    /// </summary>
    /// <param name="key">인스턴스화할 프리팹의 Addressable 주소</param>
    /// <param name="position">생성될 위치</param>
    /// <param name="rotation">생성될 회전값</param>
    /// <param name="parent">부모 Transform</param>
    /// <param name="onCompleted">생성이 완료되었을 때 생성된 GameObject를 전달받는 콜백</param>
    /// <param name="defaultCapacity">생성할 풀의 기본 용량</param>
    /// <param name="maxSize">생성할 풀의 최대 용량</param>
    public void InstantiateAsync(string key, Vector3? position = null, Quaternion? rotation = null, Transform parent = null, System.Action<GameObject> onCompleted = null, int defaultCapacity = 10, int maxSize = 50)
    {
        // 프리팹을 비동기적으로 로드하거나 캐시에서 가져옵니다.
        LoadPrefabAsync(key, (prefab) =>
        {
            // 프리팹 로드 실패 시 null 반환됨
            if (prefab == null)
            {
                Debug.LogError($"[ResourceManager] 객체화에 실패했습니다. addressable key: {key}");
                onCompleted?.Invoke(null);
                return;
            }

            // 로드된 프리팹을 사용하여 PoolManager를 통해 객체를 생성
            GameObject go = Managers.Pool.Spawn(prefab, position, rotation, parent, defaultCapacity, maxSize);

            // 비동기이기 때문에 완료 시점에 호출해주기 위해서 onCompleted를 사용
            onCompleted?.Invoke(go);
        });
    }

    /// <summary>
    /// 사용이 끝난 GameObject를 풀에 반납합니다.
    /// 풀에 반납할 수 없을 경우 Destroy합니다.
    /// </summary>
    /// <param name="go">풀에 반납할 GameObject</param>
    public void Destroy(GameObject go)
    {
        Managers.Pool.Despawn(go);
    }

    /// <summary>
    /// Addressable 주소를 이용해 프리팹을 비동기 로드합니다.
    /// </summary>
    /// <param name="key">Addressable 주소</param>
    /// <param name="onCompleted">로드 완료 시 콜백 동작</param>
    private void LoadPrefabAsync(string key, System.Action<GameObject> onCompleted)
    {
        // 이미 로드된 핸들이 있으면 사용
        if (_prefabHandles.TryGetValue(key, out var handle))
        {
            if (handle.IsDone)
            {
                onCompleted?.Invoke(handle.Result);
                return;
            }

            // 핸들이 아직 로딩 중이면 완료 콜백 연결
            handle.Completed += (op) => onCompleted?.Invoke(op.Result);
            return;
        }

        // 로드된 핸들이 없다면 새로 로드
        // 중복 방지를 위해서 딕셔너리에 바로 추가해요.
        var newHandle = Addressables.LoadAssetAsync<GameObject>(key);
        _prefabHandles.Add(key, newHandle);

        newHandle.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                onCompleted?.Invoke(op.Result);
            }
            else
            {
                Debug.LogError($"[ResourceManager] 프리팹 로드를 실패했습니다. addressables key: {key} - {op.OperationException}");
                _prefabHandles.Remove(key); // 실패 시 딕셔너리에서 제거
                onCompleted?.Invoke(null);
            }
        };
    }
}