using UnityEngine;
using NikkeClone.Utils;

/// <summary>
/// 현재 엔터티에 적용되어 실행 중인 개별 버프/디버프 인스턴스입니다.
/// </summary>
public class ActiveEffect
{
    public EffectData Data { get; private set; }
    public float RemainingTime { get; set; }

    /// <summary>지속시간 종료 여부</summary>
    public bool IsExpired => RemainingTime <= 0f;

    public ActiveEffect(EffectData data)
    {
        Data = data; // 구조체라 깊복
        RemainingTime = data.Duration;
    }
}
