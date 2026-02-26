using UnityEngine;

/// <summary>
/// 통합된 재장전(Reload) 상태 클래스입니다.
/// Phase 8.1: Manual/Auto에서 각각 정의되던 로직을 하나로 합치고, 중복 호출 버그를 수정했습니다.
/// </summary>
public class NikkeReloadState : IState<CombatNikke>
{
    private float _reloadTimer;
    private bool _isReloaded;

    public void Enter(CombatNikke owner)
    {
        owner.UpdateState(eNikkeState.Reload);
        _reloadTimer = 0f;
        _isReloaded = false;
    }

    public void Execute(CombatNikke owner)
    {
        // 이미 재장전 처리가 끝났다면 중복 실행 방지 (Auto 모드 무한 루프 수정)
        if (_isReloaded) return;

        _reloadTimer += Time.deltaTime;

        if (_reloadTimer >= owner.Weapon.ReloadTime)
        {
            owner.Weapon.Reload();
            _isReloaded = true;
            // 실제 상태 전환은 상위(NikkeCoverStateBase)에서 탄약 변화를 감지하여 수행합니다.
        }
    }

    public void Exit(CombatNikke owner)
    {
        _isReloaded = false;
    }
}
