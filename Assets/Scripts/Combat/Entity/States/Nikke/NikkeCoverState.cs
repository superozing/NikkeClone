using UnityEngine;

/// <summary>
/// 니케의 엄폐 상태 (기본 상태)입니다.
/// </summary>
public class NikkeCoverState : IState<CombatNikke>
{
    public void Enter(CombatNikke owner)
    {
        Debug.Log($"[{owner.NikkeName}] Enter Cover State");
        // TODO Phase 3: 엄폐 애니메이션 재생
        // TODO Phase 3: 피격 범위 축소 적용
    }

    public void Execute(CombatNikke owner)
    {
        // TODO Phase 3: 플레이어 입력 대기 (발사 버튼 등)
    }

    public void Exit(CombatNikke owner)
    {
        // TODO Phase 3: 엄폐 해제 애니메이션
    }
}
