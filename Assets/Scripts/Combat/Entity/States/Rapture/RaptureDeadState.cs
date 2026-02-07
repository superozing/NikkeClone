using UnityEngine;

/// <summary>
/// 랩쳐의 사망 상태입니다.
/// </summary>
public class RaptureDeadState : IState<CombatRapture>
{
    public void Enter(CombatRapture owner)
    {
        Debug.Log($"[{owner.RaptureName}] Enter Dead State");
        // TODO Phase 4: 사망 애니메이션 및 오브젝트 풀 반환
    }

    public void Execute(CombatRapture owner)
    {
        // 아무것도 하지 않음
    }

    public void Exit(CombatRapture owner)
    {
        // 재사용 시 Reset 로직 필요
    }
}
