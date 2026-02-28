using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투용 랩쳐 클래스입니다.
/// </summary>
public class CombatRapture : CombatEntity
{
    // ==================== Data ====================

    private RaptureGameData _gameData;
    private StateMachine<CombatRapture> _stateMachine;
    private eRangeZone _currentZone;
    private Dictionary<eRaptureState, IState<CombatRapture>> _states;

    // ==================== Properties ====================

    /// <summary>랩쳐 ID</summary>
    public int RaptureId => _gameData?.id ?? -1;

    /// <summary>랩쳐 이름</summary>
    public string RaptureName => _gameData?.name ?? "Unknown";

    /// <summary>현재 상태</summary>
    public eRaptureState CurrentState { get; private set; }

    /// <summary>현재 위치 구역</summary>
    public eRangeZone CurrentZone => _currentZone;

    /// <summary>사망 시 발생</summary>
    public event System.Action<CombatRapture> OnDeath;

    // ==================== Public Methods ====================

    /// <summary>
    /// 랩쳐 데이터를 주입하고 상태 머신을 초기화합니다.
    /// Caller: WaveSystem (Phase 4)
    /// </summary>
    public void Initialize(RaptureGameData gameData, eRangeZone zone)
    {
        _gameData = gameData;
        _currentZone = zone;

        // 스탯 초기화
        if (_gameData != null)
        {
            _baseStatus = _gameData.status;
            _currentHp = MaxHp;
        }

        // 상태 머신 초기화
        _stateMachine = new StateMachine<CombatRapture>(this);
        _states = new Dictionary<eRaptureState, IState<CombatRapture>>
        {
            { eRaptureState.Idle, new RaptureIdleState() },
            { eRaptureState.Move, new RaptureMoveState() },
            { eRaptureState.Attack, new RaptureAttackState() },
            { eRaptureState.Dead, new RaptureDeadState() }
        };

        // 초기 상태 진입
        ChangeState(eRaptureState.Idle);

        Debug.Log($"[CombatRapture] Initialized: {RaptureName} HP:{MaxHp} Zone:{_currentZone}");
    }

    /// <summary>
    /// 상태를 변경합니다.
    /// Caller: 각 상태 클래스의 Execute()
    /// </summary>
    public void ChangeState(eRaptureState newState)
    {
        if (_states.ContainsKey(newState))
        {
            CurrentState = newState;
            _stateMachine.ChangeState(_states[newState]);
        }
        else
        {
            Debug.LogError($"[CombatRapture] State not found: {newState}");
        }
    }

    /// <summary>
    /// 사망 처리 (Override)
    /// </summary>
    public override void Die()
    {
        base.Die();
        ChangeState(eRaptureState.Dead);
        OnDeath?.Invoke(this);
    }

    // ==================== Test Code (Phase 2 Only) ====================

    private void Update()
    {
        _stateMachine?.Update();
    }
}
