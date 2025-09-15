using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.Pool;

    private readonly Dictionary<string, ObjectPool<GameObject>> _pools = new();
    private Transform _root;

    public void Init()
    {
        // 풀 매니저 루트 설정
        if (_root == null)
        {
            GameObject root = GameObject.Find("@PoolRoot") ?? new GameObject { name = "@PoolRoot" };
            Object.DontDestroyOnLoad(root);
            _root = root.transform;
        }

        Debug.Log($"{ManagerType} Manager Init 합니다.");
    }

    public void Update() { }

    public void Clear()
    {
        foreach (var pool in _pools.Values)
            pool.Clear();

        _pools.Clear();
        
        Debug.Log($"{ManagerType} Manager Clear 합니다.");
    }

    public void Register(string key, int defaultCapacity = 10, int maxSize = 50)
    {
        if (_pools.ContainsKey(key))
        {
            Debug.LogWarning($"[PoolManager] 이미 등록된 key입니다: {key}");
            return;
        }

        GameObject prefab = Resources.Load<GameObject>($"Prefabs/{key}");
        if (prefab == null)
        {
            Debug.LogError($"[PoolManager] 프리팹 로드 실패: Prefabs/{key}");
            return;
        }
        if (prefab.GetComponent<Poolable>() == null)
        {
            Debug.LogError($"[PoolManager] Poolable 컴포넌트가 없습니다: Prefabs/{key}");
            return;
        }

        var pool = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                GameObject go = Object.Instantiate(prefab, _root);
                go.GetComponent<Poolable>().PrefabKey = key;
                return go;
            },
            actionOnGet: go => go.SetActive(true),
            actionOnRelease: go => go.SetActive(false),
            actionOnDestroy: go => Object.Destroy(go),
            collectionCheck: false,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );

        _pools.Add(key, pool);

        WarmUpPool(pool, defaultCapacity);
    }

    public GameObject Spawn(string key, Vector3 position, Quaternion rotation)
    {
        if (!_pools.TryGetValue(key, out var pool))
        {
            Debug.LogError($"[PoolManager] Spawn 실패 - key 미등록: {key}");
            return null;
        }

        GameObject go = pool.Get();
        go.transform.SetPositionAndRotation(position, rotation);
        return go;
    }

    public void Despawn(GameObject go)
    {
        if (go == null)
        {
            Debug.LogWarning("[PoolManager] Despawn 실패 - null GameObject");
            return;
        }

        if (go.TryGetComponent<Poolable>(out var poolable) && !string.IsNullOrEmpty(poolable.PrefabKey))
        {
            if (_pools.TryGetValue(poolable.PrefabKey, out var pool))
            {
                pool.Release(go);
                return;
            }
        }

        Debug.LogWarning($"[PoolManager] 풀에 등록되지 않은 오브젝트({go.name})입니다. Destroy 합니다.");
        Object.Destroy(go);
    }

    private void WarmUpPool(ObjectPool<GameObject> pool, int count)
    {
        var warmUpList = new List<GameObject>();
        for (int i = 0; i < count; i++)
        {
            warmUpList.Add(pool.Get());
        }
        foreach (var item in warmUpList)
        {
            pool.Release(item);
        }
    }
}