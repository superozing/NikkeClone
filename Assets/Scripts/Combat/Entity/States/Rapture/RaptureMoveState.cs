using UnityEngine;

/// <summary>
/// 랩쳐의 이동 상태입니다.
/// </summary>
public class RaptureMoveState : IState<CombatRapture>
{
    public void Enter(CombatRapture owner)
    {
        Debug.Log($"[{owner.RaptureName}] Enter Move State");
        // TODO Phase 4: 이동 애니메이션
    }

    public void Execute(CombatRapture owner)
    {
        // TODO Phase 4: 구역 이동 처리
    }

    public void Exit(CombatRapture owner)
    {
        // TODO Phase 4: 이동 완료 처리
    }
}
