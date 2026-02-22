using UnityEngine;

/// <summary>
/// 엄폐 상태(Cover)입니다.
/// 하위에 Idle(대기/회복)과 Reload(재장전) 상태를 가집니다.
/// </summary>
public class NikkeManualCoverState : IState<CombatNikke>
{
    private StateMachine<CombatNikke> _coverStateMachine;
    private NikkeManualCoverIdleState _idleState;
    private NikkeManualReloadState _reloadState;

    public void Enter(CombatNikke owner)
    {
        // Debug.Log($"[{owner.name}] Enter Cover State");
        owner.View.UpdateVisualState(eNikkeState.Cover); // Cover 스프라이트 (공통)

        if (_coverStateMachine == null)
        {
            InitializeSubStateMachine(owner);
        }

        // 초기 진입: 탄약이 꽉 차있으면 Idle, 아니면 Reload?
        // 기획 결정: "엄폐 시 자동 장전" (유저 피드백 반영 B안)
        // -> 탄약이 부족하면 Reload로, 아니면 Idle로.

        if (owner.Weapon.CurrentAmmo.Value < owner.Weapon.MaxAmmo)
        {
            _coverStateMachine.ChangeState(_reloadState);
        }
        else
        {
            _coverStateMachine.ChangeState(_idleState);
        }
    }

    public void Execute(CombatNikke owner)
    {
        _coverStateMachine.Update();

        // 상태 전환 로직 (재장전 완료 -> Idle)
        // ReloadState가 종료되면 Idle로 전환해야 함.
        // ReloadState가 스스로 전환할 수 없으므로(구조상), 여기서 체크하거나
        // ReloadState에 Callback을 주입?

        // 간단히: 현재 상태가 Reload이고, 장전이 다 됐으면 Idle로.
        if (_coverStateMachine.CurrentState == _reloadState)
        {
            // ReloadState 로직 내에서 owner.RefillAmmo() 호출됨.
            // 완료 여부를 owner.CurrentAmmo == MaxAmmo로 판단 가능.
            if (owner.Weapon.CurrentAmmo.Value >= owner.Weapon.MaxAmmo)
            {
                _coverStateMachine.ChangeState(_idleState);
            }
        }
    }

    public void Exit(CombatNikke owner)
    {
        _coverStateMachine.CurrentState?.Exit(owner);
    }

    private void InitializeSubStateMachine(CombatNikke owner)
    {
        _coverStateMachine = new StateMachine<CombatNikke>(owner);
        _idleState = new NikkeManualCoverIdleState();
        _reloadState = new NikkeManualReloadState();
    }
}

// 내부 클래스 or 별도 파일? 별도 파일 권장되지만 편의상 여기에 (나중에 분리 가능)
// 일단 분리하지 않고 구현.

public class NikkeManualCoverIdleState : IState<CombatNikke>
{
    public void Enter(CombatNikke owner)
    {
        // Debug.Log($"[{owner.name}] Cover-Idle");
        // 엄폐 중 회복 로직 시작 등
    }

    public void Execute(CombatNikke owner)
    {
        // 대기 (체력 회복 등)
    }

    public void Exit(CombatNikke owner)
    {
    }
}
