using UnityEngine;

/// <summary>
/// 랩쳐의 공격 상태입니다.
/// </summary>
public class RaptureAttackState : IState<CombatRapture>
{
    public void Enter(CombatRapture owner)
    {
        Debug.Log($"[{owner.RaptureName}] Enter Attack State");
        // TODO Phase 4: 공격 애니메이션
    }

    public void Execute(CombatRapture owner)
    {
        // TODO Phase 4: 공격 로직 (데미지 전달)
    }

    public void Exit(CombatRapture owner)
    {
        // TODO Phase 4: 공격 후 딜레이
    }
}
