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
    private CombatTargetingSystem _targetingSystem;


    // ==================== Properties ====================

    public int NikkeId => _gameData?.id ?? -1;
    public int SlotIndex => _slotIndex;
    public IWeapon Weapon => _weapon;
    public NikkeView View => _view;
    public string NikkeName => _gameData?.name;
    public NikkeGameData GameData => _gameData;
    public CombatTargetingSystem CombatTargetingSystem => _targetingSystem;
    public CombatSystem CombatSystem => _combatSystem;


    // ==================== V2 추가 ====================

    /// <summary>현재 이 니케가 플레이어의 조작 대상인지 (반응형)</summary>
    public ReactiveProperty<bool> IsSelected { get; } = new(false);

    /// <summary>현재 이 니케의 상태 (반응형 UI 및 애니메이션 동기화용)</summary>
    public ReactiveProperty<eNikkeState> State { get; } = new(eNikkeState.Cover);

    /// <summary>자동 전투 토글 상태 (Selected 니케만 영향)</summary>
    /// Setter: CombatSystem.OnToggleAuto()
    public bool AutoToggle
    {
        get => _autoToggle;
        set
        {
            if (_autoToggle != value)
            {
                _autoToggle = value;
                SyncWeaponCombatMode();
            }
        }
    }
    private bool _autoToggle;

    /// <summary>마우스(Fire 버튼) 누르고 있는지</summary>
    /// Setter: OnFirePerformed(), OnFireCanceled()
    public bool IsMousePressed { get; private set; }

    /// <summary>캐싱된 메인 카메라 (Selected 시 캐싱, 미선택 시 Camera.main 폴백)</summary>
    public Camera CachedCamera
    {
        get
        {
            if (_cachedCamera == null) _cachedCamera = Camera.main;
            return _cachedCamera;
        }
        private set => _cachedCamera = value;
    }
    private Camera _cachedCamera;

    // ==================== Strategy (횡단 관심사) ====================

    private IAimStrategy _manualAimStrategy;
    private IAimStrategy _autoAimStrategy;
    private IAimStrategy _currentAimStrategy;
    private Vector2 _currentAimScreenPos;

    private System.Action<UnityEngine.InputSystem.InputAction.CallbackContext> _onFirePerformed;
    private System.Action<UnityEngine.InputSystem.InputAction.CallbackContext> _onFireCanceled;

    // ==================== Events ====================

    /// <summary>니케 사망 시 발생하는 이벤트 (상속된 OnDeath 외에 니케 전용 정보 포함)</summary>
    public new event System.Action<CombatNikke> OnDeath;

    // ==================== Public Methods ====================

    /// <summary>
    /// 니케 초기화 및 HFSM 가동
    /// Caller: CombatSystem.InitializeAsync()
    /// </summary>
    public async Task InitializeAsync(NikkeGameData gameData, UserNikkeData userData, int slotIndex, CombatSystem combatSystem, IWeapon weapon)
    {
        _gameData = gameData;
        _userData = userData;
        _slotIndex = slotIndex;
        _combatSystem = combatSystem;
        _targetingSystem = combatSystem.CombatTargetingSystem;

        // 스탯 계산 및 초기화
        CalculateStatus();
        InitializeStatus();
        _currentHp = MaxHp;

        // View 초기화
        if (_view == null)
        {
            Debug.LogError($"[CombatNikke] {_gameData?.name} (Slot {_slotIndex}) has no NikkeView! Check Inspector binding.");
            return;
        }

        // Weapon 주입 (CombatSystem에서 생성해서 넘겨줌)
        _weapon = weapon;

        // View 초기화
        await _view.InitializeAsync(gameData, slotIndex, _vcam);

        // Strategy 초기화
        _manualAimStrategy = new ManualAimStrategy();
        _autoAimStrategy = new AutoAimStrategy();
        _currentAimStrategy = _autoAimStrategy; // 기본: Auto

        // HFSM 초기화 (V2: 고정 상태 생성)
        _hfsm = new NikkeHFSM(this);


        Debug.Log($"[CombatNikke] Initialized: {name}");
    }

    // 사망 처리
    public override void Die()
    {
        base.Die();
        UpdateState(eNikkeState.Dead);
        _hfsm.OnDead();
        _view.PlayDeathEffect();
        OnDeath?.Invoke(this);
    }

    /// <summary>
    /// 이 니케가 플레이어의 조작 대상으로 선택됨.
    /// Camera 활성화, Input 바인딩, Crosshair 연동.
    /// </summary>
    /// Caller: CombatSystem.SelectNikke()
    public void OnSelected()
    {
        IsSelected.Value = true;
        CachedCamera = Camera.main;
        _view.SetCameraActive(true);

        // Input 바인딩
        _onFirePerformed = _ => OnFirePerformed();
        _onFireCanceled = _ => OnFireCanceled();
        Managers.Input.BindAction("Fire", _onFirePerformed, UnityEngine.InputSystem.InputActionPhase.Performed);
        Managers.Input.BindAction("Fire", _onFireCanceled, UnityEngine.InputSystem.InputActionPhase.Canceled);

        // Crosshair UI 연동
        NotifyManualActivated();

        SyncWeaponCombatMode();
    }

    /// <summary>
    /// 이 니케의 조작 대상 해제.
    /// Camera 비활성화, Input 언바인딩.
    /// </summary>
    /// Caller: CombatSystem.SelectNikke()
    public void OnDeselected()
    {
        IsSelected.Value = false;
        IsMousePressed = false;

        if (_onFirePerformed != null)
        {
            Managers.Input.UnbindAction("Fire", _onFirePerformed, UnityEngine.InputSystem.InputActionPhase.Performed);
            Managers.Input.UnbindAction("Fire", _onFireCanceled, UnityEngine.InputSystem.InputActionPhase.Canceled);
            _onFirePerformed = null;
            _onFireCanceled = null;
        }

        _view.SetCameraActive(false);
        CachedCamera = null;

        SyncWeaponCombatMode();
    }

    /// <summary>
    /// 조준 Strategy를 실제 교체합니다. Weapon 세션을 리셋하여 차지 발사/취소를 처리합니다.
    /// </summary>
    private void ApplyAimStrategy(bool toManual)
    {
        var newStrategy = toManual ? _manualAimStrategy : _autoAimStrategy;
        if (_currentAimStrategy == newStrategy) return;

        var prev = _currentAimStrategy;
        _currentAimStrategy = newStrategy;

        // Attack 상태일 때만 Weapon 세션 리셋
        if (_hfsm != null && _hfsm.CurrentState is NikkeAttackState)
        {
            if (prev == _manualAimStrategy)
            {
                // Manual → Auto: 차지 무기는 "발사"로 종료 (isCancel: false)
                _weapon?.Exit(this, isCancel: false);
                _weapon?.Enter(this);
            }
            else if (prev == _autoAimStrategy)
            {
                // Auto → Manual: Auto 세션 취소 후 Manual 세션 시작
                _weapon?.Exit(this, isCancel: true);
                _weapon?.Enter(this);
            }
        }
    }

    /// <summary>
    /// 매 프레임 조준 스크린 좌표를 갱신합니다.
    /// State와 무관하게 항상 실행됩니다.
    /// Cover 상태에서도 조준선이 마우스/타겟을 추종합니다.
    /// </summary>
    private void UpdateAimPosition()
    {
        if (_weapon == null || _currentAimStrategy == null) return;

        // [수정] 수동 모드 판정 기준 통일: 무기의 CombatMode가 Manual이면 수동 모드로 간주
        // SyncWeaponCombatMode()가 IsSelected, IsMousePressed, AutoToggle을 종합하여 결정함
        bool isManual = _weapon.CombatMode.Value == eNikkeCombatMode.Manual;

        if (_hfsm != null && _hfsm.IsCovering && isManual) return;

        // Selected가 아닌 니케도 Auto 전략으로 좌표 갱신 (비선택 니케의 Auto 전투용)
        _currentAimScreenPos = _currentAimStrategy.GetAimScreenPosition(
            this, _currentAimScreenPos, Time.deltaTime);

        _weapon.CurrentAimScreenPosition.Value = _currentAimScreenPos;
    }

    /// <summary>
    /// UI 호환성(CrosshairViewModel)을 위해 무기의 CombatMode를 동기화합니다.
    /// </summary>
    private void SyncWeaponCombatMode()
    {
        if (_weapon == null) return;

        // 1. 선택되지 않은 니케는 항상 Auto
        if (!IsSelected.Value)
        {
            _weapon.CombatMode.Value = eNikkeCombatMode.Auto;
            ApplyAimStrategy(toManual: false);
            return;
        }

        // 2. 선택된 니케는 (수동 조준 중) 또는 (자동 전투 OFF)일 때 Manual
        bool isManual = IsMousePressed || !AutoToggle;
        _weapon.CombatMode.Value = isManual ? eNikkeCombatMode.Manual : eNikkeCombatMode.Auto;
        ApplyAimStrategy(toManual: isManual);
    }

    private void OnFirePerformed()
    {
        IsMousePressed = true;
        SyncWeaponCombatMode();
        // EvaluateTransitions()가 다음 프레임에 Attack 전환 판단
    }

    private void OnFireCanceled()
    {
        IsMousePressed = false;
        SyncWeaponCombatMode();
        // EvaluateTransitions()에서 Cover 전환 또는 Auto 전투 유지 판단
    }

    // ==================== Private Methods ====================

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

    protected override void Update()
    {
        base.Update();
        UpdateAimPosition();        // (1) 조준 좌표 (State 무관, 항상 실행)
        _hfsm?.Update();            // (2) 상태 실행 + 전환 평가

        _weapon?.Tick(Time.deltaTime); // (3) 무기 Tick
    }

    /// <summary>
    /// 니케의 상태를 갱신합니다. 애니메이션(View)과 UI(ReactiveProperty)를 함께 동기화합니다.
    /// </summary>
    public void UpdateState(eNikkeState newState)
    {
        if (State.Value == newState && newState != eNikkeState.Dead) return;

        State.Value = newState;
        _view.UpdateVisualState(newState);
    }


    /// <summary>
    /// 전체 엄폐 토글. HFSM에 위임합니다.
    /// </summary>
    /// Caller: CombatSystem.ToggleAllCover()
    public void SetForcedCover(bool forced) => _hfsm.SetForcedCover(forced);

    private void OnDestroy()
    {
        if (_view != null)
            _view.DestroyView();
    }
}

