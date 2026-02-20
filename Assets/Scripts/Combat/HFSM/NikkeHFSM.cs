using System.Collections.Generic;
using UnityEngine;
using NikkeClone.Utils; // for eNikkeCombatMode

/// <summary>
/// 니케의 최상위 계층형 상태 머신(HFSM)을 관리하는 클래스입니다.
/// Manual, Auto, Stun, Dead와 같은 상위 상태(Mode) 전환을 담당합니다.
/// </summary>
public class NikkeHFSM
{
    private CombatNikke _owner;
    private StateMachine<CombatNikke> _masterStateMachine;
    private Dictionary<eNikkeCombatMode, IState<CombatNikke>> _modes;

    public eNikkeCombatMode CurrentMode { get; private set; }

    public NikkeHFSM(CombatNikke owner)
    {
        _owner = owner;
        _masterStateMachine = new StateMachine<CombatNikke>(owner);
        _modes = new Dictionary<eNikkeCombatMode, IState<CombatNikke>>();
    }

    /// <summary>
    /// 상위 상태(Mode)를 등록합니다.
    /// </summary>
    public void RegisterMode(eNikkeCombatMode mode, IState<CombatNikke> state)
    {
        if (!_modes.ContainsKey(mode))
        {
            _modes.Add(mode, state);
        }
    }

    /// <summary>
    /// 모드를 변경합니다.
    /// </summary>
    public void ChangeMode(eNikkeCombatMode newMode)
    {
        if (_modes.ContainsKey(newMode))
        {
            // 같은 모드여도 초기화가 필요하다면 전환 허용 (보통은 체크함)
            if (CurrentMode == newMode && newMode != eNikkeCombatMode.Stun) return;

            Debug.Log($"[{_owner.name}] Change Mode: {CurrentMode} -> {newMode}");
            CurrentMode = newMode;
            _masterStateMachine.ChangeState(_modes[newMode]);
        }
        else
        {
            Debug.LogError($"[NikkeHFSM] Mode not registered: {newMode}");
        }
    }

    public void Update()
    {
        _masterStateMachine.Update();
    }
}
