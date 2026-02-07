using UnityEngine;

/// <summary>
/// 니케의 공격 상태입니다.
/// </summary>
public class NikkeAttackState : IState<CombatNikke>
{
    public void Enter(CombatNikke owner)
    {
        Debug.Log($"[{owner.NikkeName}] Enter Attack State");
        // TODO Phase 3: 조준/발사 애니메이션 재생
    }

    public void Execute(CombatNikke owner)
    {
        // TODO Phase 3: 사격 로직 (탄환 소비, 레이캐스트)
        // TODO Phase 3: 탄환 소진 시 Reload로 전환
    }

    public void Exit(CombatNikke owner)
    {
        // TODO Phase 3: 사격 중지
    }
}
