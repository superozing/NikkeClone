using UnityEngine;

public class SceneManagerEx : IManagerBase
{
    const eManagerType type = eManagerType.Scene;
    public eManagerType GetManagerType() => type;

    public void Init()
    {
        Debug.Log($"{type} Manager Init 합니다.");
    }

    public void Update()
    {
    }

    public void Clear()
    {
        Debug.Log($"{type} Manager Clear 합니다.");
    }
}
