using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Cinemachine;
using NikkeClone.Utils;


/// <summary>
/// 전투용 니케 클래스입니다.
/// Phase 6.1: HFSM과 IWeapon을 적용하여 자율적으로 행동하도록 리팩토링되었습니다.
/// 더 이상 상위(CombatScene)가 상태를 직접 제어하지 않습니다.
/// </summary>
public class CombatNikke : CombatEntity
{
    // ==================== Data ====================

    [SerializeField] private NikkeView _view;
    [SerializeField] private CinemachineCamera _vcam;

    private NikkeGameData _gameData;
    private UserNikkeData _userData;

    private NikkeHFSM _hfsm;
    private IWeapon _weapon;
    private CombatSystem _combatSystem;
    private int _slotIndex;
    private TargetingSystem _targetingSystem;

    // ==================== Properties ====================

    public int NikkeId => _gameData?.id ?? -1;
    public int SlotIndex => _slotIndex;
    public IWeapon Weapon => _weapon;
    public NikkeView View => _view;
    public string NikkeName => _gameData?.name;
    public TargetingSystem TargetingSystem => _targetingSystem;
    public CombatSystem CombatSystem => _combatSystem;

    // ==================== Events ====================

    public event System.Action<CombatNikke> OnDeath;
    public event System.Action<eNikkeCombatMode> OnModeChanged;

    // ==================== Public Methods ====================

    /// <summary>
    /// 니케 초기화 및 HFSM 가동
    /// Caller: CombatSystem.InitializeAsync()
    /// </summary>
    public async Task InitializeAsync(NikkeGameData gameData, UserNikkeData userData, int slotIndex, CombatSystem combatSystem)
    {
        _gameData = gameData;
        _userData = userData;
        _slotIndex = slotIndex;
        _combatSystem = combatSystem;
        _targetingSystem = combatSystem.TargetingSystem;

        // 스탯 계산
        CalculateStatus();
        _currentHp = MaxHp;

        // View 초기화
        if (_view == null)
        {
            Debug.LogError($"[CombatNikke] {_gameData?.name} (Slot {_slotIndex}) has no NikkeView! Check Inspector binding.");
            return;
        }

        // Weapon 초기화 (Phase 7: 다형성 기반 Factory 패턴)
        _weapon = WeaponFactory.CreateWeapon(_gameData?.weapon, _gameData?.WeaponType ?? eNikkeWeapon.AR);

        // View 초기화
        await _view.InitializeAsync(gameData, slotIndex, _vcam);

        // HFSM 초기화
        _hfsm = new NikkeHFSM(this);
        _hfsm.RegisterMode(eNikkeCombatMode.Manual, new NikkeManualState());
        _hfsm.RegisterMode(eNikkeCombatMode.Auto, new NikkeAutoState());
        _hfsm.RegisterMode(eNikkeCombatMode.Stun, new NikkeStunState());
        _hfsm.RegisterMode(eNikkeCombatMode.Dead, new NikkeDeadState()); // Empty State

        // 초기 상태: Auto
        _hfsm.ChangeMode(eNikkeCombatMode.Auto);

        Debug.Log($"[CombatNikke] Initialized: {name}");
    }

    // 사망 처리
    public override void Die()
    {
        base.Die();
        _hfsm.ChangeMode(eNikkeCombatMode.Dead);
        _view.PlayDeathEffect();
        OnDeath?.Invoke(this);
    }

    /// <summary>
    /// 외부(State 등)에서 모드 변경을 요청할 때 사용
    /// </summary>
    public void SetCombatMode(eNikkeCombatMode mode)
    {
        ChangeMode(mode);
    }

    // ==================== Private Methods ====================

    private void ChangeMode(eNikkeCombatMode mode)
    {
        if (_hfsm.CurrentMode == mode) return;

        _hfsm.ChangeMode(mode);

        if (_weapon != null)
        {
            _weapon.CombatMode.Value = mode;
        }

        OnModeChanged?.Invoke(mode);
    }



    /// <summary>
    /// 외부(CombatSystem)에서 강제로 조작 니케를 변경할 때 사용
    /// Caller: CombatSystem.InitializeAsync(), OnNikkeDied()
    /// </summary>
    public void ForceActivate()
    {
        SetCombatMode(eNikkeCombatMode.Manual);
    }

    /// <summary>
    /// 수동 조작 전투 니케가 활성화될 때 UI와 바인딩할 무기 객체를 갱신합니다.
    /// Phase 7.1 Crosshair UI
    /// </summary>
    public void NotifyManualActivated()
    {
        if (_weapon != null && _combatSystem != null)
        {
            _combatSystem.SetCrosshairWeapon(_weapon);
        }
    }

    /// <summary>
    /// 선택된 니케의 카메라를 활성화하고 크로스헤어를 바인딩합니다.
    /// Caller: CombatSystem.SelectNikke()
    /// </summary>
    public void ActivateCameraAndCrosshair()
    {
        _view.SetCameraActive(true);
        if (_weapon != null && _combatSystem != null)
        {
            _combatSystem.SetCrosshairWeapon(_weapon);
        }
    }

    /// <summary>
    /// 선택 해제된 니케의 카메라를 비활성화합니다.
    /// Caller: CombatSystem.SelectNikke()
    /// </summary>
    public void DeactivateCamera()
    {
        _view.SetCameraActive(false);
    }

    private void CalculateStatus()
    {
        if (_gameData == null || _userData == null) return;

        _baseStatus = new StatusData
        {
            hp = _gameData.status.hp,
            attack = _gameData.status.attack,
            defense = _gameData.status.defense
        };

        float levelMultiplier = 1 + (_userData.level.Value - 1) * 0.05f;
        _baseStatus.hp = (long)(_baseStatus.hp * levelMultiplier);
        _baseStatus.attack = (long)(_baseStatus.attack * levelMultiplier);
        _baseStatus.defense = (long)(_baseStatus.defense * levelMultiplier);
    }

    private void Update()
    {
        _hfsm?.Update();
        _weapon?.Tick(Time.deltaTime);
    }

    private void OnDestroy()
    {
        if (_view != null)
            _view.DestroyView();
    }
}

