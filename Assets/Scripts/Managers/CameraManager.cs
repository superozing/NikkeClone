using UnityEngine;

public class CameraManager : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.Camera;
    public void Init()
    {
        Debug.Log($"{ManagerType} Manager Init 합니다.");
    }

    public void Start()
    {
        Debug.Log($"{ManagerType} Manager Start 합니다.");
    }

    public void Update()
    {
    }

    public void Clear()
    {
        Debug.Log($"{ManagerType} Manager Clear 합니다.");
    }
}
