using UnityEngine;

/// <summary>
/// 니케의 계층형 상태 머신.
/// L0: Attack / Cover / Stun / Dead
/// Attack 내부: IAimStrategy (Manual/Auto) 교체
/// Cover 내부: L1 SubStateMachine (Idle/Reload)
/// </summary>
public class NikkeHFSM
{
    private readonly CombatNikke _owner;
    private readonly StateMachine<CombatNikke> _stateMachine;

    private readonly NikkeAttackState _attackState;
    private readonly NikkeCoverState _coverState;
    private readonly NikkeStunState _stunState;
    private readonly NikkeDeadState _deadState;

    private bool _isForcedCover;

    public IState<CombatNikke> CurrentState => _stateMachine.CurrentState;
    public bool IsCovering => _stateMachine.CurrentState == _coverState;
    public bool IsForcedCover => _isForcedCover;

    /// Caller: CombatNikke.InitializeAsync()
    public NikkeHFSM(CombatNikke owner)
    {
        _owner = owner;
        _stateMachine = new StateMachine<CombatNikke>(owner);

        _attackState = new NikkeAttackState();
        _coverState = new NikkeCoverState(owner);
        _stunState = new NikkeStunState();
        _deadState = new NikkeDeadState();

        // 초기 상태: Cover
        _stateMachine.ChangeState(_coverState);
    }

    /// Caller: CombatNikke.Update()
    public void Update()
    {
        _stateMachine.Update();
        EvaluateTransitions();
    }

    // ==================== 전환 판단 ====================

    /// <summary>
    /// 매 프레임 전환 조건을 평가합니다.
    /// StateMachine.Update() 이후에 호출됩니다.
    /// </summary>
    private void EvaluateTransitions()
    {
        var current = _stateMachine.CurrentState;

        // ===== 절대 상태 (ResolveDesiredState 대상 아님) =====
        if (current == _deadState) return;

        if (current == _stunState)
        {
            if (_stunState.IsFinished)
            {
                _stateMachine.ChangeState(_coverState);
            }
            return;
        }

        // ===== 우선순위 평가 → 전환 =====
        var desired = ResolveDesiredState();

        if (desired != current)
        {
            _stateMachine.ChangeState(desired);
        }
    }

    /// <summary>
    /// 우선순위 테이블을 순회하여 현재 조건에 맞는 목표 상태를 반환합니다.
    /// 현재 상태와 무관하게, 조건의 우선순위만으로 판단합니다.
    /// </summary>
    /// <returns>전환하고자 하는 목표 상태</returns>
    private IState<CombatNikke> ResolveDesiredState()
    {
        // P3: 전체 엄폐 강제 (가장 높은 우선순위)
        if (_isForcedCover) return _coverState;

        bool hasAmmo = _owner.Weapon.CurrentAmmo.Value > 0;

        // P4: 탄약 소진 (재장전 필요 시 엄폐 강제)
        if (!hasAmmo) return _coverState;

        bool isMousePressed = _owner.IsSelected.Value && _owner.IsMousePressed;

        // P5: 수동 사격 (Selected 니케 + 마우스 누름)
        if (isMousePressed) return _attackState;

        bool hasTarget = _owner.CombatTargetingSystem?.GetTarget(_owner.Weapon.PreferredZone) != null;
        bool isAutoEligible = !_owner.IsSelected.Value || _owner.AutoToggle;

        // P6: 자동 사격 (타겟 존재 + 자동 적격)
        if (hasTarget && isAutoEligible) return _attackState;

        // P7: 기본 상태 (대기 중 엄폐 유지)
        return _coverState;
    }

    // ==================== 외부 트리거 ====================

    /// <summary>
    /// CC(기절) 적용. 외부에서 지속시간을 전달합니다.
    /// </summary>
    /// Caller: CombatNikke.ApplyStun() (Phase N 구현 시)
    public void OnStun(float duration)
    {
        if (_stateMachine.CurrentState == _deadState) return;
        _stunState.SetStunInfo(duration);
        _stateMachine.ChangeState(_stunState);
    }

    /// <summary>
    /// 사망 처리.
    /// </summary>
    /// Caller: CombatNikke.Die()
    public void OnDead()
    {
        _stateMachine.ChangeState(_deadState);
    }

    /// <summary>
    /// 전체 엄폐 토글. 활성화 시 Attack→Cover 강제 전환 + Cover 유지.
    /// 해제 시 EvaluateTransitions가 정상 판단 재개.
    /// </summary>
    /// Caller: CombatNikke.SetForcedCover()
    public void SetForcedCover(bool forced)
    {
        _isForcedCover = forced;
    }
}
