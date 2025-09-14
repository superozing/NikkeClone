using UnityEngine;

public class PoolManager : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.Pool;
    public void Init()
    {
        Debug.Log($"{ManagerType} Manager Init 합니다.");
    }

    public void Update()
    {
    }

    public void Clear()
    {
        Debug.Log($"{ManagerType} Manager Clear 합니다.");
    }
}
