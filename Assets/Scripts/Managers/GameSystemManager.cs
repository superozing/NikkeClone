using UnityEngine;

public class GameSystemManager : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.GameSystem;
    public MissionSystem MissionSystem { get; private set; }

    public void Init()
    {
        MissionSystem = new MissionSystem();
        MissionSystem.Init();

        Debug.Log($"{ManagerType} Manager Init 합니다.");
    }

    public void Update() { }

    public void Clear()
    {
        MissionSystem?.Dispose();

        Debug.Log($"{ManagerType} Manager Clear 합니다.");
    }
}
