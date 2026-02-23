using UnityEngine;

public class NikkeAutoCoverState : IState<CombatNikke>
{
    private StateMachine<CombatNikke> _coverStateMachine;
    private NikkeAutoCoverIdleState _idleState;
    private NikkeAutoReloadState _reloadState;

    public void Enter(CombatNikke owner)
    {
        owner.View.UpdateVisualState(eNikkeState.Cover);

        if (_coverStateMachine == null)
        {
            InitializeSubStateMachine(owner);
        }

        // Auto 모드: 엄폐 시 탄약 부족하면 자동 장전
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

        if (_coverStateMachine.CurrentState == _reloadState)
        {
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
        _idleState = new NikkeAutoCoverIdleState();
        _reloadState = new NikkeAutoReloadState();
    }
}

public class NikkeAutoCoverIdleState : IState<CombatNikke>
{
    public void Enter(CombatNikke owner)
    {
        // Debug.Log($"[{owner.name}] Auto Cover Idle");
    }

    public void Execute(CombatNikke owner)
    {
        // 대기 (HP 회복 등?)
    }

    public void Exit(CombatNikke owner)
    {
    }
}
