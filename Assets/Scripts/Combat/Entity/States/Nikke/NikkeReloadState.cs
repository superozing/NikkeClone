using UnityEngine;

/// <summary>
/// 니케의 재장전 상태입니다.
/// </summary>
public class NikkeReloadState : IState<CombatNikke>
{
    public void Enter(CombatNikke owner)
    {
        Debug.Log($"[{owner.NikkeName}] Enter Reload State");
        // TODO Phase 3: 재장전 애니메이션 및 타이머 시작
    }

    public void Execute(CombatNikke owner)
    {
        // TODO Phase 3: 재장전 완료 체크 -> Cover로 전환
    }

    public void Exit(CombatNikke owner)
    {
        // TODO Phase 3: 탄환 보충 (RefillAmmo)
    }
}
