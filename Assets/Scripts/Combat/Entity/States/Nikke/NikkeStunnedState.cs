using UnityEngine;

/// <summary>
/// 니케의 기절 상태 (스킬 피격 등)입니다.
/// </summary>
public class NikkeStunnedState : IState<CombatNikke>
{
    public void Enter(CombatNikke owner)
    {
        Debug.Log($"[{owner.NikkeName}] Enter Stunned State");
        // TODO Phase 3: 기절 애니메이션, 조작 불능 처리
    }

    public void Execute(CombatNikke owner)
    {
        // TODO Phase 3: 기절 지속시간 체크 후 복귀 -> Cover
    }

    public void Exit(CombatNikke owner)
    {
        // TODO Phase 3: 조작 가능 상태로 복구
    }
}
