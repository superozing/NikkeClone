using UnityEngine;

public class NikkeAutoState : IState<CombatNikke>
{
    private StateMachine<CombatNikke> _subStateMachine;
    private NikkeAutoCoverState _coverState;
    private NikkeAutoAttackState _attackState;

    public void Enter(CombatNikke owner)
    {
        if (_subStateMachine == null)
        {
            _subStateMachine = new StateMachine<CombatNikke>(owner);
            _coverState = new NikkeAutoCoverState();
            _attackState = new NikkeAutoAttackState(); // [추가]
        }

        // 초기 상태: Cover (장전 상태 확인 후 Attack 전환은 Execute에서)
        _subStateMachine.ChangeState(_coverState);

        owner.View.SetCameraActive(false);
    }

    // [전면 재작성]
    public void Execute(CombatNikke owner)
    {
        _subStateMachine.Update();

        var preferredZone = owner.Weapon.PreferredZone;

        // 매 프레임(주기)마다 타겟 존재 여부 확인
        var target = owner.TargetingSystem?.GetTarget(preferredZone);
        bool hasTarget = target != null;
        bool needsReload = owner.Weapon.CurrentAmmo.Value <= 0;

        if (_subStateMachine.CurrentState == _attackState)
        {
            if (!hasTarget)
            {
                // 타겟이 없다 -> 자동 엄폐 상태
                _subStateMachine.ChangeState(_coverState);
            }
            else if (needsReload)
            {
                // 타겟이 있지만 재장전해야 한다 -> 자동 엄폐 상태
                _subStateMachine.ChangeState(_coverState);
            }
        }
        else if (_subStateMachine.CurrentState == _coverState)
        {
            // 타겟이 있고 쏠 총알이 1발이라도 있다면 공격 상태
            if (hasTarget && !needsReload)
            {
                _subStateMachine.ChangeState(_attackState);
            }
        }
    }

    public void Exit(CombatNikke owner)
    {
        _subStateMachine.CurrentState?.Exit(owner);
    }
}
