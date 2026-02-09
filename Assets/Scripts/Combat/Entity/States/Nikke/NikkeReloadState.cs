using UnityEngine;

/// <summary>
/// 니케의 재장전 상태입니다.
/// </summary>
public class NikkeReloadState : IState<CombatNikke>
{
    private float _reloadTimer;

    public void Enter(CombatNikke owner)
    {
        _reloadTimer = 0f;
        Debug.Log($"[{owner.NikkeName}] Reloading... ({owner.ReloadTime}s)");
        owner.SetStateSprite(eNikkeState.Reload);
    }

    public void Execute(CombatNikke owner)
    {
        _reloadTimer += Time.deltaTime;

        if (_reloadTimer >= owner.ReloadTime)
        {
            // 재장전 완료 처리
            owner.RefillAmmo();

            // 원래 상태로 복귀 (Phase 3: 단순화하여 Cover로 복귀)
            // *디자인 문서*: "여전히 공격 중이면 Attack으로, 아니면 Cover로"
            // 하지만 현재는 Input 기반이므로 기본적으로 Cover로 돌아가고,
            // 유저가 계속 클릭하면 다음 프레임에 Attack으로 전환될 것임 (또는 HandleClick에서 처리)
            // 우선 Cover로 전환.
            owner.ChangeState(eNikkeState.Cover);
        }
    }

    public void Exit(CombatNikke owner)
    {
        // Enter/Execute에서 처리했으므로 Exit에서는 특별한 처리 없음
    }
}
