using UnityEngine;

/// <summary>
/// 랩쳐의 대기 상태입니다.
/// </summary>
public class RaptureIdleState : IState<CombatRapture>
{
    public void Enter(CombatRapture owner)
    {
        Debug.Log($"[{owner.RaptureName}] Enter Idle State");
        // TODO Phase 4: 대기 애니메이션
    }

    public void Execute(CombatRapture owner)
    {
        // TODO Phase 4: 타겟 탐색 및 이동/공격 전환
    }

    public void Exit(CombatRapture owner)
    {
        // TODO Phase 4: 이동 준비
    }
}
