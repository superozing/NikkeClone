using UnityEngine;

public class GameSystemManager : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.GameSystem;
    public MissionSystem MissionSystem { get; private set; }
    public TimeSystem TimeSystem { get; private set; }

    public void Init()
    {
        Debug.Log($"{ManagerType} Manager Init 합니다.");
    }

    public void Start()
    {
        MissionSystem = new MissionSystem();
        MissionSystem.Init();

        TimeSystem = new TimeSystem();
        TimeSystem.Init();

        Debug.Log($"{ManagerType} Manager Start 합니다.");
    }

    public void Update() { }

    public void Clear()
    {
        MissionSystem?.Dispose();
        TimeSystem?.Dispose();

        Debug.Log($"{ManagerType} Manager Clear 합니다.");
    }


}
