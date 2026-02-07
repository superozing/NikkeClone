using UnityEngine;

/// <summary>
/// 니케의 사망 상태입니다.
/// </summary>
public class NikkeDeadState : IState<CombatNikke>
{
    public void Enter(CombatNikke owner)
    {
        Debug.Log($"[{owner.NikkeName}] Enter Dead State");
        // TODO Phase 3: 사망 애니메이션, UI 비활성화, 타겟 제외
    }

    public void Execute(CombatNikke owner)
    {
        // 사망 상태에서는 아무것도 하지 않음
    }

    public void Exit(CombatNikke owner)
    {
        // 일반적으로 사망 상태에서 빠져나오지 않음 (부활 로직이 없다면)
    }
}
