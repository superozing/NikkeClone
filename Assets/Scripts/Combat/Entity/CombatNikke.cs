using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투용 니케 클래스입니다.
/// Phase 2: 상태 머신과 CombatEntity 상속이 적용되었습니다.
/// </summary>
public class CombatNikke : CombatEntity
{
    // ==================== Data ====================

    private NikkeGameData _gameData;
    private UserNikkeData _userData;
    private StateMachine<CombatNikke> _stateMachine;
    private int _slotIndex;
    private int _currentAmmo;
    private Dictionary<eNikkeState, IState<CombatNikke>> _states;

    // ==================== Properties ====================

    /// <summary>니케 ID</summary>
    public int NikkeId => _gameData?.id ?? -1;

    /// <summary>니케 이름</summary>
    public string NikkeName => _gameData?.name ?? "Unknown";

    /// <summary>현재 상태</summary>
    public eNikkeState CurrentState { get; private set; }

    /// <summary>배치 슬롯 인덱스</summary>
    public int SlotIndex => _slotIndex;

    /// <summary>현재 탄약</summary>
    public int CurrentAmmo => _currentAmmo;

    /// <summary>최대 탄약</summary>
    public int MaxAmmo => _gameData.weapon.maxAmmo;

    /// <summary>재장전 시간</summary>
    public float ReloadTime => _gameData.weapon.reloadTime;

    // Phase 3: CanAttack 프로퍼티 추가 예정

    // ==================== Public Methods ====================

    /// <summary>
    /// 니케 데이터를 주입하고 상태 머신을 초기화합니다.
    /// Caller: CombatScene.InitializeNikkes()
    /// </summary>
    public void Initialize(NikkeGameData gameData, UserNikkeData userData, int slotIndex)
    {
        _gameData = gameData;
        _userData = userData;
        _slotIndex = slotIndex;

        // 스탯 계산
        CalculateStatus();
        _currentHp = MaxHp;
        _currentAmmo = MaxAmmo;

        // 상태 머신 초기화
        _stateMachine = new StateMachine<CombatNikke>(this);
        _states = new Dictionary<eNikkeState, IState<CombatNikke>>
        {
            { eNikkeState.Cover, new NikkeCoverState() },
            { eNikkeState.Attack, new NikkeAttackState() },
            { eNikkeState.Reload, new NikkeReloadState() },
            { eNikkeState.Stunned, new NikkeStunnedState() },
            { eNikkeState.Dead, new NikkeDeadState() }
        };

        // 초기 상태 진입
        ChangeState(eNikkeState.Cover);

        Debug.Log($"[CombatNikke] Initialized: {NikkeName} (Lv.{_userData.level.Value}) HP:{MaxHp}");
    }

    /// <summary>
    /// 상태를 변경합니다.
    /// Caller: 각 상태 클래스의 Execute()
    /// </summary>
    public void ChangeState(eNikkeState newState)
    {
        if (_states.ContainsKey(newState))
        {
            CurrentState = newState;
            _stateMachine.ChangeState(_states[newState]);
        }
        else
        {
            Debug.LogError($"[CombatNikke] State not found: {newState}");
        }
    }

    // Phase 3: ConsumeAmmo, RefillAmmo 구현 예정

    // ==================== Private Methods ====================

    private void CalculateStatus()
    {
        if (_gameData == null || _userData == null) return;

        // 기본 스탯 복사
        _baseStatus = new StatusData
        {
            hp = _gameData.status.hp,
            attack = _gameData.status.attack,
            defense = _gameData.status.defense
        };

        // 레벨 보정: 1 + (Lv-1) * 0.05
        float levelMultiplier = 1 + (_userData.level.Value - 1) * 0.05f;

        // 스탯 적용
        _baseStatus.hp = (long)(_baseStatus.hp * levelMultiplier);
        _baseStatus.attack = (long)(_baseStatus.attack * levelMultiplier);
        _baseStatus.defense = (long)(_baseStatus.defense * levelMultiplier);
    }

    // ==================== Test Code (Phase 2 Only) ====================

    private void Update()
    {
        _stateMachine?.Update();

        // 테스트: 스페이스바로 상태 전환 루프
        // Cover -> Attack -> Reload -> Cover
        if (Input.GetKeyDown(KeyCode.Space) && _stateMachine != null)
        {
            if (CurrentState == eNikkeState.Cover)
                ChangeState(eNikkeState.Attack);
            else if (CurrentState == eNikkeState.Attack)
                ChangeState(eNikkeState.Reload);
            else if (CurrentState == eNikkeState.Reload)
                ChangeState(eNikkeState.Cover);
        }
    }
}
