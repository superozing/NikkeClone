using UnityEngine;

/// <summary>
/// 사망 상태입니다.
/// 아무 행동도 하지 않습니다.
/// </summary>
public class NikkeDeadState : IState<CombatNikke>
{
    public void Enter(CombatNikke owner)
    {
        Debug.Log($"[{owner.name}] Dead.");
        // 사망 애니메이션은 CombatNikke.Die()에서 처리했음 (View.PlayDeathEffect)
        // 여기선 상태 유지
    }

    public void Execute(CombatNikke owner)
    {
    }

    public void Exit(CombatNikke owner)
    {
    }
}
