using UnityEngine;

/// <summary>
/// Auto 모드 공격 하위 상태.
/// TargetingSystem에서 타겟을 획득하여 IWeapon.Update에 위치를 전달합니다.
/// </summary>
public class NikkeAutoAttackState : IState<CombatNikke>
{
    private CombatRapture _currentTarget;
    private eRangeZone _preferredZone;

    /// <summary>Caller: NikkeAutoState의 SubStateMachine</summary>
    public void Enter(CombatNikke owner)
    {
        _preferredZone = owner.Weapon.PreferredZone;

        // 타겟 획득
        _currentTarget = owner.TargetingSystem?.GetTarget(_preferredZone);

        owner.View.UpdateVisualState(eNikkeState.Attack);
        owner.Weapon?.Enter(owner);

        if (_currentTarget != null)
        {
            Debug.Log($"[NikkeAutoAttackState] {owner.name} Target: {_currentTarget.RaptureName} at {_currentTarget.CurrentZone}");
        }
    }

    /// <summary>
    /// Caller: SubStateMachine.Update()
    /// Intent: 타겟 유효성 체크 → IWeapon.Update(owner, targetPos) 호출
    /// </summary>
    public void Execute(CombatNikke owner)
    {
        // 탄약 체크는 상위(NikkeAutoState)에서 수행

        // 1. 타겟 유효성 확인
        if (_currentTarget == null || _currentTarget.IsDead)
        {
            _currentTarget = owner.TargetingSystem?.GetTarget(_preferredZone);

            if (_currentTarget == null)
            {
                // 타겟 없음 → 상위에서 Cover로 전환 판단
                return;
            }
        }

        // 2. ui 십자선 방향으로 쏠 목표(Raycast) 좌표 산출 
        Vector3 targetWorldPos;
        if (owner.Weapon != null && Camera.main != null)
        {
            Vector2 aimScreenPos = owner.Weapon.CurrentAimScreenPosition.Value;

            // 만약 십자선이 화면을 벗어났거나 초기 상태인 경우, 실제 적의 위치로 대체
            if (aimScreenPos == Vector2.zero)
            {
                targetWorldPos = _currentTarget.transform.position;
            }
            else
            {
                Ray ray = Camera.main.ScreenPointToRay(aimScreenPos);
                int layerMask = LayerMask.GetMask("CombatRapture", "CombatObstacle");
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
                {
                    targetWorldPos = hit.point;
                }
                else
                {
                    targetWorldPos = ray.GetPoint(100f);
                }
            }
        }
        else
        {
            targetWorldPos = _currentTarget.transform.position;
        }

        owner.Weapon?.Update(owner, targetWorldPos);

        // [추가] 3. 적정 사거리 UI 피드백 업데이트
        if (owner.Weapon != null)
        {
            owner.Weapon.IsInPreferredZone.Value = IsTargetInPreferredZone();

            // [추가] 자동 조준 화면 좌표 업데이트 (조명, 이펙트를 위한 월드->스크린)
            if (Camera.main != null)
            {
                owner.Weapon.AutoTargetScreenPosition.Value = Camera.main.WorldToScreenPoint(_currentTarget.transform.position);
            }
        }
    }

    /// <summary>Caller: SubStateMachine (상태 전환 시)</summary>
    public void Exit(CombatNikke owner)
    {
        owner.Weapon?.Exit(owner, isCancel: true);
        if (owner.Weapon != null)
        {
            owner.Weapon.IsInPreferredZone.Value = false;
        }
        _currentTarget = null;
    }

    /// <summary>
    /// 현재 타겟이 적정 사거리에 있는지 반환합니다.
    /// </summary>
    /// Caller: NikkeAutoState.Execute() — 크로스헤어 색상 피드백용
    public bool IsTargetInPreferredZone()
    {
        if (_currentTarget == null || _currentTarget.IsDead) return false;
        return _currentTarget.CurrentZone == _preferredZone;
    }

    /// <summary>현재 타겟 참조 (상위 상태에서 어드밴티지 판정 등에 사용)</summary>
    public CombatRapture CurrentTarget => _currentTarget;
}
