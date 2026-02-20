using UnityEngine;

/// <summary>
/// 자동 전투(AI) 모드 상태입니다.
/// Phase 6.1에서는 구조만 잡고, 실제 AI 로직은 추후 구현합니다.
/// 현재는 단순히 엄폐 상태만 유지하거나, 간단한 로직만 수행합니다.
/// </summary>
public class NikkeAutoState : IState<CombatNikke>
{
    private StateMachine<CombatNikke> _subStateMachine;
    private NikkeAutoCoverState _coverState;
    // 추후 AutoAttack, AutoReload 등 추가

    private System.Action<UnityEngine.InputSystem.InputAction.CallbackContext> _onSelectSelf;

    public void Enter(CombatNikke owner)
    {
        // Debug.Log($"[{owner.name}] Enter Auto Mode");

        if (_subStateMachine == null)
        {
            _subStateMachine = new StateMachine<CombatNikke>(owner);
            _coverState = new NikkeAutoCoverState();
        }

        // 일단 무조건 Cover 상태로 시작 (AI 미구현)
        _subStateMachine.ChangeState(_coverState);

        // Auto 모드에서는 카메라 비활성화 (선택된 니케만 활성화)
        owner.View.SetCameraActive(false);

        // Input 바인딩 (자신 선택 시 Manual 전환)
        _onSelectSelf = _ => OnSelectSelfInput(owner);
        Managers.Input.BindAction($"SelectNikke{owner.SlotIndex + 1}", _onSelectSelf);
    }

    public void Execute(CombatNikke owner)
    {
        _subStateMachine.Update();

        // Phase 8: 여기서 AI 판단 로직 (엄폐할지, 쏠지, 재장전할지) 수행
    }

    public void Exit(CombatNikke owner)
    {
        _subStateMachine.CurrentState?.Exit(owner);

        if (_onSelectSelf != null)
        {
            Managers.Input.UnbindAction($"SelectNikke{owner.SlotIndex + 1}", _onSelectSelf);
            _onSelectSelf = null;
        }
    }

    private void OnSelectSelfInput(CombatNikke owner)
    {
        owner.SetCombatMode(NikkeClone.Utils.eNikkeCombatMode.Manual);
    }
}
