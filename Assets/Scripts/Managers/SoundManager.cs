using UnityEngine;

public class SoundManager : IManagerBase
{
    const eManagerType type = eManagerType.Sound;
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
