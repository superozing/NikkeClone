using UnityEngine;
using System;

/// <summary>
/// UI 데미지 넘버 방출 요청을 중계하는 뷰모델입니다.
/// </summary>
public class DamageNumberViewModel : ViewModelBase
{
    /// <summary>
    /// 새로운 데미지 넘버 방출 시 발생하는 이벤트입니다.
    /// 파라미터: 데미지량, 월드 좌표
    /// </summary>
    public event Action<long, Vector3> OnDamageEmitted;

    /// <summary>
    /// 데미지 넘버 방출을 요청합니다.
    /// </summary>
    /// <param name="damage">데미지 수치</param>
    /// <param name="worldPos">피격 지점의 3D 월드 좌표</param>
    public void RequestDamageNumber(long damage, Vector3 worldPos)
    {
        OnDamageEmitted?.Invoke(damage, worldPos);
    }

    protected override void OnDispose()
    {
        OnDamageEmitted = null;
    }
}
