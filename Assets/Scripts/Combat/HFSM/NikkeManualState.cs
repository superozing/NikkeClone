using UnityEngine;
using NikkeClone.Utils;

/// <summary>
/// 수동 조작 모드 상태입니다.
/// 하위 상태 머신(Attack/Cover)을 포함합니다.
/// Cover 상태 내부에 Idle/Reload가 존재합니다.
/// </summary>
public class NikkeManualState : IState<CombatNikke>
{
    private StateMachine<CombatNikke> _subStateMachine;
    private NikkeManualAttackState _attackState;
    private NikkeManualCoverState _coverState;

    // _reloadState 제거 (Cover 내부로 이동)

    private System.Action<UnityEngine.InputSystem.InputAction.CallbackContext> _onFirePerformed;
    private System.Action<UnityEngine.InputSystem.InputAction.CallbackContext> _onFireCanceled;

    private System.Action<UnityEngine.InputSystem.InputAction.CallbackContext>[] _onSelectOtherSlots;

    public void Enter(CombatNikke owner)
    {
        // Debug.Log($"[{owner.name}] Enter Manual Mode");

        if (_subStateMachine == null)
        {
            InitializeSubStateMachine(owner);
        }

        // 초기 상태: Cover (내부에서 탄약 체크 후 Reload/Idle 분기함)
        _subStateMachine.ChangeState(_coverState);

        // Input 바인딩 (Fire)
        _onFirePerformed = _ => OnFireInput(owner, true);
        _onFireCanceled = _ => OnFireInput(owner, false);

        Managers.Input.BindAction("Fire", _onFirePerformed, UnityEngine.InputSystem.InputActionPhase.Performed);
        Managers.Input.BindAction("Fire", _onFireCanceled, UnityEngine.InputSystem.InputActionPhase.Canceled);

        // Input 바인딩 (다른 슬롯 선택 시 Auto 전환)
        BindOtherSlotInputs(owner);

        owner.View.SetCameraActive(true);

        // Phase 7.1 Crosshair 연동: 수동 조작 활성화 시 무기를 알림
        owner.NotifyManualActivated();
    }

    private void BindOtherSlotInputs(CombatNikke owner)
    {
        // 최대 5슬롯 가정
        _onSelectOtherSlots = new System.Action<UnityEngine.InputSystem.InputAction.CallbackContext>[5];

        for (int i = 0; i < 5; i++)
        {
            if (i == owner.SlotIndex) continue;

            int targetIndex = i; // Closure capture
            _onSelectOtherSlots[i] = _ => OnSelectOtherInput(owner);
            Managers.Input.BindAction($"SelectNikke{targetIndex + 1}", _onSelectOtherSlots[i]);
        }
    }

    private void UnbindOtherSlotInputs(CombatNikke owner)
    {
        if (_onSelectOtherSlots == null) return;

        for (int i = 0; i < 5; i++)
        {
            if (_onSelectOtherSlots[i] != null)
            {
                Managers.Input.UnbindAction($"SelectNikke{i + 1}", _onSelectOtherSlots[i]);
            }
        }
        _onSelectOtherSlots = null;
    }

    private void OnSelectOtherInput(CombatNikke owner)
    {
        owner.SetCombatMode(eNikkeCombatMode.Auto);
    }

    public void Execute(CombatNikke owner)
    {
        _subStateMachine.Update();

        // [Phase 6.1 Refactor]
        // AttackState에서 탄약 소진 시 Cover로 전환
        // AttackState는 IWeapon.Update()만 호출하고, 상태 전환 판단은 상위(Manual)에서 수행
        if (_subStateMachine.CurrentState == _attackState)
        {
            if (owner.Weapon.CurrentAmmo.Value <= 0)
            {
                // 탄약 다 떨어짐 -> 엄폐(Cover)로 전환 -> Cover 내부에서 Reload 시작
                _subStateMachine.ChangeState(_coverState);
            }
        }
    }

    public void Exit(CombatNikke owner)
    {
        // Debug.Log($"[{owner.name}] Exit Manual Mode");
        if (_subStateMachine != null && _subStateMachine.CurrentState == _attackState)
        {
            // 수동 조작 해제(다른 니케 선택 등) 시 무기 강제 취소 (차지 보존)
            owner.Weapon?.Exit(owner, isCancel: true);
        }

        if (_onFirePerformed != null) Managers.Input.UnbindAction("Fire", _onFirePerformed, UnityEngine.InputSystem.InputActionPhase.Performed);
        if (_onFireCanceled != null) Managers.Input.UnbindAction("Fire", _onFireCanceled, UnityEngine.InputSystem.InputActionPhase.Canceled);
        UnbindOtherSlotInputs(owner);

        _onFirePerformed = null;
        _onFireCanceled = null;
        owner.View.SetCameraActive(false);
    }

    private void InitializeSubStateMachine(CombatNikke owner)
    {
        _subStateMachine = new StateMachine<CombatNikke>(owner);
        _attackState = new NikkeManualAttackState();
        _coverState = new NikkeManualCoverState();
    }

    // Input Handler
    private void OnFireInput(CombatNikke owner, bool isPressed)
    {
        // 구조 변경: Attack <-> Cover
        // Reload는 Cover 내부의 일임.
        // 공격 시도 시 탄약이 없으면? -> AttackState 진입했다가 바로 강제 Cover로 복귀?
        // or 진입 자체를 막음?

        if (isPressed)
        {
            // 탄약 없으면 못 쏨 (재장전 필요 -> Cover로 가야 함)
            if (owner.Weapon.CurrentAmmo.Value <= 0)
            {
                // 이미 Cover라면 거기서 재장전 중일 것임.
                // Attack 중 0이 되면? -> AttackState에서 체크해서 끝내야 함.
                // 여기선 진입 차단.
                if (_subStateMachine.CurrentState != _coverState)
                    _subStateMachine.ChangeState(_coverState);
                return;
            }

            if (_subStateMachine.CurrentState != _attackState)
                _subStateMachine.ChangeState(_attackState);
        }
        else
        {
            if (_subStateMachine.CurrentState != _coverState)
                _subStateMachine.ChangeState(_coverState);
        }
    }
}
