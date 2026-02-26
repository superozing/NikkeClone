using UnityEngine;

/// <summary>
/// 엄폐 상태. 내부에 Idle/Reload 하위 상태 머신을 소유합니다.
/// Manual/Auto 구분 없이 동일 로직입니다.
/// </summary>
public class NikkeCoverState : IState<CombatNikke>
{
    private readonly StateMachine<CombatNikke> _subStateMachine;
    private readonly IState<CombatNikke> _idleState;
    private readonly IState<CombatNikke> _reloadState;

    /// <summary>
    /// Caller: NikkeHFSM 생성자
    /// </summary>
    public NikkeCoverState(CombatNikke owner)
    {
        _subStateMachine = new StateMachine<CombatNikke>(owner);
        _idleState = new NikkeCoverIdleState();
        _reloadState = new NikkeReloadState();
    }

    public void Enter(CombatNikke owner)
    {
        owner.UpdateState(eNikkeState.Cover);

        if (owner.Weapon.CurrentAmmo.Value < owner.Weapon.MaxAmmo)
            _subStateMachine.ChangeState(_reloadState);
        else
            _subStateMachine.ChangeState(_idleState);
    }

    public void Execute(CombatNikke owner)
    {
        _subStateMachine.Update();

        // 재장전 완료 시 Idle로 전환
        if (_subStateMachine.CurrentState == _reloadState)
        {
            if (owner.Weapon.CurrentAmmo.Value >= owner.Weapon.MaxAmmo)
            {
                _subStateMachine.ChangeState(_idleState);
            }
        }
    }

    public void Exit(CombatNikke owner)
    {
        _subStateMachine.CurrentState?.Exit(owner);
    }
}
