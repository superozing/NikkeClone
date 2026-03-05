using System;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 전투 씬의 핵심 로직을 담당하는 오케스트레이터입니다.
/// `CombatScene`으로부터 위임받아 니케, 웨이브, UI, 게임 진행을 총괄합니다.
/// </summary>
public class CombatSystem : MonoBehaviour
{
    // ==================== Dependencies ====================
    [Header("Dependencies")]
    [SerializeField] private CombatNikke[] _nikkes; // 5 slot
    [SerializeField] private CombatWaveSystem _waveSystem;
    [SerializeField] private CombatCrosshairSystem _crosshairSystem;
    private UI_CombatHUD _combatHUD;
    private UI_HealthBarBoard _healthBarBoard;

    // ==================== Trigger & Skill (Phase 10) ====================
    private CombatTriggerSystem _triggerSystem;
    private CombatSkillSystem _skillSystem;
    private CombatStatRecordSystem _statRecordSystem;
    public CombatStatRecordSystem StatRecordSystem => _statRecordSystem;

    // ==================== State ====================
    private CombatBurstSystem _burstSystem;
    public CombatBurstSystem BurstSystem => _burstSystem;
    public int AliveNikkeCount { get; private set; }

    private IWeapon[] _weapons;
    private System.Action<CombatNikke, long>[] _onHitCallbacks;

    private bool _isCombatEnded;
    private float _timeLimitSec;

    private float _remainingTime;
    private int _lastRemainingSec = -1;

    // ==================== Phase 8 ====================
    private CombatTargetingSystem _targetingSystem;
    public CombatTargetingSystem CombatTargetingSystem => _targetingSystem;

    // 적정 사거리 피드백용


    private System.Action<UnityEngine.InputSystem.InputAction.CallbackContext>[] _onSelectNikkeWrappers;

    private bool _isAutoCombatMode = false;
    private bool _isAutoBurstMode = false;

    public ReactiveProperty<bool> IsAllCover { get; } = new(false);
    private int _currentSelectedSlot = -1;

    private CombatHUDViewModel _hudViewModel;
    private StageGameData _stageGameData;

    // ==================== Events ====================

    public event System.Action<eCombatResult> OnCombatEnded;

    // ==================== Public Methods ====================

    /// <summary>
    /// 전투 시스템을 초기화하고 전투를 시작합니다.
    /// Caller: CombatScene.Init()
    /// </summary>
    public async Task InitializeAsync(int stageId, int squadId)
    {
        Debug.Log("[CombatSystem] InitializeAsync Start");

        _isCombatEnded = false;

        // 0. 트리거 및 스킬 시스템 초기화 (Phase 10)
        _triggerSystem = new CombatTriggerSystem();
        _skillSystem = new CombatSkillSystem();
        _statRecordSystem = new CombatStatRecordSystem();
        _statRecordSystem.Initialize(_triggerSystem, _nikkes);

        // 1. 데이터 로드
        var stageData = Managers.Data.Get<StageGameData>(stageId);
        if (stageData == null)
        {
            Debug.LogError($"[CombatSystem] StageGameData missing: {stageId}");
            return;
        }
        _stageGameData = stageData;

        var battleData = Managers.Data.Get<StageBattleGameData>(stageData.stageBattleDataId);
        if (battleData == null)
        {
            Debug.LogError($"[CombatSystem] StageBattleGameData missing: {stageData.stageBattleDataId}");
            return;
        }

        _timeLimitSec = battleData.timeLimitSec > 0 ? battleData.timeLimitSec : 180;
        _remainingTime = _timeLimitSec;

        // 2. WaveSystem 초기화
        if (_waveSystem == null)
            _waveSystem = FindFirstObjectByType<CombatWaveSystem>();

        if (_waveSystem != null)
        {
            // RaptureField 처리 (WaveSystem 내부에서 필요시 찾음, 여기선 생략 가능하지만 명시적 초기화가 좋음)
            _waveSystem.OnAllPhasesComplete += OnAllPhasesComplete;
            _waveSystem.OnRaptureSpawned += OnWaveRaptureSpawned;
            _waveSystem.OnRaptureDied += OnWaveRaptureDied;
        }

        // CombatTargetingSystem 초기화
        var raptureField = FindFirstObjectByType<RaptureField>();
        if (raptureField != null)
        {
            _targetingSystem = new CombatTargetingSystem();
            _targetingSystem.Initialize(raptureField);
        }

        // ToggleAuto(Combat) 입력 바인딩
        Managers.Input.BindAction("ToggleAuto", OnToggleAutoCombatWrapper);
        Managers.Input.BindAction("ToggleAutoBurst", OnToggleAutoBurstWrapper);
        Managers.Input.BindAction("ToggleAllCover", OnToggleAllCoverWrapper);
        Managers.Input.BindAction("Pause", OnPauseWrapper);

        // NIKKE 선택 입력 바인딩
        _onSelectNikkeWrappers = new System.Action<UnityEngine.InputSystem.InputAction.CallbackContext>[5];
        for (int i = 0; i < 5; i++)
        {
            int index = i; // Closure capture
            _onSelectNikkeWrappers[i] = _ => SelectNikke(index);
            Managers.Input.BindAction($"SelectNikke{index + 1}", _onSelectNikkeWrappers[i]);
        }

        // 3. 버스트 매니저 초기화 (니케 초기화 및 HUD 초기화 이전에 수행)
        _burstSystem = new CombatBurstSystem(_nikkes);
        _burstSystem.SetAutoMode(_isAutoBurstMode);

        // 3.1 UI, Nikke 초기화
        await InitializeNikkesAsync(squadId);

        await InitializeHUDAsync();

        // 3.5 조준선 시스템 초기화 (Phase 7.1 Refactor v2)
        if (_crosshairSystem != null)
        {
            HashSet<eNikkeWeapon> squadWeaponTypes = new HashSet<eNikkeWeapon>();
            foreach (var nikke in _nikkes)
            {
                if (nikke != null && nikke.Weapon != null)
                {
                    squadWeaponTypes.Add(nikke.Weapon.WeaponType);
                }
            }
            // 외부 트리거 시스템과 델리게이트 주입
            await _crosshairSystem.InitializeAsync(squadWeaponTypes, _triggerSystem, () => _currentSelectedSlot);

        }

        // 3.6 초기 조작 니케 설정 (HUD와 Crosshair 객체가 모두 생성된 후에 호출되어야 함)
        ActivateDefaultNikke();

        // 4. 전투 시작 (웨이브)
        if (_waveSystem != null)
        {
            await _waveSystem.StartBattleAsync(battleData);
        }

        Debug.Log("[CombatSystem] Initialization Complete. Battle Started.");
        _isInitialized = true;
    }

    private void ActivateDefaultNikke()
    {
        int defaultIndex = 2; // 중심에 있는 3번 캐릭터
        if (defaultIndex < _nikkes.Length && _nikkes[defaultIndex] != null)
        {
            SelectNikke(defaultIndex);
        }
        else if (_nikkes.Length > 0 && _nikkes[0] != null)
        {
            SelectNikke(0);
        }
    }

    /// <summary>
    /// 지정된 슬롯의 니케를 플레이어 화면/조작 니케로 선택합니다.
    /// </summary>
    public void SelectNikke(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _nikkes.Length || _nikkes[slotIndex] == null || _nikkes[slotIndex].IsDead)
            return;

        // 동일한 슬롯 선택 시 무시
        if (_currentSelectedSlot == slotIndex)
            return;

        // 1. 현재 조작 중이던 니케 카메라 및 조전선 연결 비활성화
        if (_currentSelectedSlot >= 0 && _currentSelectedSlot < _nikkes.Length && _nikkes[_currentSelectedSlot] != null)
        {
            _nikkes[_currentSelectedSlot].OnDeselected();
        }

        // 2. 새 니케 선택
        _currentSelectedSlot = slotIndex;
        var newNikke = _nikkes[_currentSelectedSlot];

        // 카메라 및 조준선 활성화
        newNikke.OnSelected();
        newNikke.AutoToggle = _isAutoCombatMode; // 현재 전투의 Auto 상태를 새 니케에 적용
    }



    /// <summary>
    /// 니케 사망 시 호출
    /// Caller: CombatNikke.Die()
    /// </summary>
    public void OnNikkeDied(CombatNikke nikke)
    {
        AliveNikkeCount--;
        _healthBarBoard?.UnregisterEntity(nikke);
        Debug.Log($"[CombatSystem] Nikke Died: {nikke.NikkeName}. Alive: {AliveNikkeCount}");

        if (AliveNikkeCount <= 0)
        {
            EndCombat(eCombatResult.Defeat);
            return;
        }

        // 현재 조작 중인 니케가 죽었다면? -> CombatSystem이 판단해서 Switch 요청?
        // CombatNikke가 스스로 판단하기 어려움 (다른 생존 니케를 모르므로)
        // 따라서 여기서 다음 니케를 찾아 NotifySwitched(또는 특정 니케 Activate) 호출

        // 사망 이벤트 시점에 다음 조작 니케를 찾아 활성화
        // 방금 죽은 니케가 수동 조작 상태였다면 해당 상태가 해제되었으므로 새로운 니케를 수동 상태로 전환해야 함

        if (nikke.SlotIndex == _currentSelectedSlot)
        {
            for (int i = 0; i < _nikkes.Length; i++)
            {
                if (_nikkes[i] != null && !_nikkes[i].IsDead)
                {
                    SelectNikke(i);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// 수동 조작 전투 니케가 활성화될 때 UI와 바인딩할 무기 객체를 갱신합니다.
    /// Phase 7.1 Crosshair UI
    /// </summary>
    public void SetCrosshairWeapon(IWeapon weapon)
    {
        // Phase 7.1 Refactor v2: CombatCrosshairSystem에 위임
        _crosshairSystem?.SwitchCrosshair(weapon);
    }

    /// <summary>
    /// 전체 엄폐 토글. 활성화 시 모든 니케 Cover 강제, 해제 시 정상 전환 재개.
    /// </summary>
    /// Caller: Input Action "ToggleAllCover"
    private void ToggleAllCover()
    {
        IsAllCover.Value = !IsAllCover.Value;
        bool isCover = IsAllCover.Value;

        foreach (var nikke in _nikkes)
        {
            if (nikke != null && !nikke.IsDead)
            {
                nikke.SetForcedCover(isCover);
            }
        }

        Debug.Log($"[CombatSystem] ToggleAllCover: {isCover}");
    }

    /// <summary>
    /// 전투 종료 및 정리
    /// Caller: CombatScene.Clear()
    /// </summary>
    public void Cleanup()
    {
        if (_waveSystem != null)
        {
            _waveSystem.OnAllPhasesComplete -= OnAllPhasesComplete;
            _waveSystem.OnRaptureSpawned -= OnWaveRaptureSpawned;
            _waveSystem.OnRaptureDied -= OnWaveRaptureDied;
        }

        Managers.Input.UnbindAction("ToggleAuto", OnToggleAutoCombatWrapper);
        Managers.Input.UnbindAction("ToggleAutoBurst", OnToggleAutoBurstWrapper);
        Managers.Input.UnbindAction("ToggleAllCover", OnToggleAllCoverWrapper);
        Managers.Input.UnbindAction("Pause", OnPauseWrapper);

        if (_onSelectNikkeWrappers != null)
        {
            for (int i = 0; i < 5; i++)
            {
                if (_onSelectNikkeWrappers[i] != null)
                {
                    Managers.Input.UnbindAction($"SelectNikke{i + 1}", _onSelectNikkeWrappers[i]);
                }
            }
            _onSelectNikkeWrappers = null;
        }

        // Phase 7.1 Refactor v2: 조준선 시스템 정리
        _crosshairSystem?.Cleanup();

        // Phase 9: 버스트 매니저 정리
        _burstSystem?.Cleanup();

        // Phase 9-2: 무기 이벤트 해제
        if (_weapons != null && _onHitCallbacks != null)
        {
            for (int i = 0; i < _weapons.Length; i++)
            {
                if (_weapons[i] is WeaponBase weaponBase && _onHitCallbacks[i] != null)
                {
                    weaponBase.OnHit -= _onHitCallbacks[i];
                }
            }
            _onHitCallbacks = null;
            _weapons = null;
        }

        // 이벤트 해제 등
    }

    // ==================== Private Methods ====================

    private bool _isInitialized = false;

    private void Update()
    {
        if (!_isInitialized || _isCombatEnded) return;

        _targetingSystem?.Tick(Time.deltaTime);
        _burstSystem?.Tick(Time.deltaTime);
        _skillSystem?.Tick(Time.deltaTime); // Phase 10: 스킬 쿨타임 업데이트

        // 타이머 업데이트 (Phase 6.1 Optimization: 초 단위 변경 시에만 갱신)
        _remainingTime -= Time.deltaTime;
        int currentSec = Mathf.FloorToInt(Mathf.Max(0, _remainingTime));

        if (currentSec != _lastRemainingSec)
        {
            _lastRemainingSec = currentSec;

            int minutes = currentSec / 60;
            int seconds = currentSec % 60;

            if (_hudViewModel != null)
            {
                _hudViewModel.TimeText.Value = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
        }

        // Phase 6.1 Fix: HUD Progress Update
        if (_combatHUD != null && _waveSystem != null)
        {
            _combatHUD.UpdateProgress(_waveSystem.Progress);
        }

        if (_remainingTime <= 0)
        {
            EndCombat(eCombatResult.Timeout);
        }
    }

    private async Task InitializeNikkesAsync(int squadId)
    {
        AliveNikkeCount = 0;
        var squadData = Managers.Data.UserData.Squads[squadId];
        if (squadData == null || squadData.slot == null)
        {
            Debug.LogError($"[CombatSystem] Invalid SquadData for ID: {squadId}");
            return;
        }

        if (_nikkes == null || _nikkes.Length == 0)
        {
            Debug.LogError("[CombatSystem] No CombatNikke found in scene.");
            return;
        }

        _weapons = new IWeapon[_nikkes.Length];
        _onHitCallbacks = new System.Action<CombatNikke, long>[_nikkes.Length];
        var gameDatas = new NikkeGameData[_nikkes.Length];

        for (int i = 0; i < _nikkes.Length && i < squadData.slot.Count; i++)
        {
            if (_nikkes[i] == null) continue;

            int nikkeId = squadData.slot[i];
            var gameData = Managers.Data.Get<NikkeGameData>(nikkeId);
            gameDatas[i] = gameData;
            var userData = Managers.Data.UserData.Nikkes[nikkeId];

            // 1. 무기 생성 (CombatSystem이 직접 책임)
            var weapon = WeaponFactory.CreateWeapon(gameData?.weapon, gameData?.WeaponType ?? eNikkeWeapon.AR);
            _weapons[i] = weapon;

            // 2. OnHit -> BurstSystem 및 TriggerSystem 바인딩
            if (weapon is WeaponBase weaponBase)
            {
                int slotIdx = i; // Closure capture
                weaponBase.OnHit += (owner, damage) =>
                {
                    _burstSystem?.AddGauge(weaponBase.GaugeChargePerHit);
                };
                weaponBase.OnHit += _onHitCallbacks[i];
            }

            // 3. 니케 초기화 (무기 주입)
            await _nikkes[i].InitializeAsync(gameData, userData, i, this, weapon);

            AliveNikkeCount++;
        }

        // 모든 니케가 초기화된 후 버스트 시스템 데이터 세팅
        _burstSystem?.Initialize(gameDatas);

        // Phase 10: 트리거 시스템 초기화 (관찰 시작)
        _triggerSystem?.Initialize(_waveSystem, _weapons, _burstSystem, _nikkes);

        // Phase 10: 스킬 로딩 (TriggerSystem 주입)
        _skillSystem?.LoadNikkeSkills(this, _triggerSystem, gameDatas);
    }

    /// <summary>
    /// Caller: Input Action "ToggleAuto" (LShift)
    /// Intent: 전체 니케 AutoCombat 토글 (사격 및 엄폐 지능형 동작)
    /// </summary>
    private void OnToggleAutoCombat()
    {
        _isAutoCombatMode = !_isAutoCombatMode;

        // Selected 니케의 AutoToggle만 변경
        if (_currentSelectedSlot >= 0 && _currentSelectedSlot < _nikkes.Length)
        {
            var selectedNikke = _nikkes[_currentSelectedSlot];
            if (selectedNikke != null && !selectedNikke.IsDead)
            {
                selectedNikke.AutoToggle = _isAutoCombatMode;
            }
        }

        Debug.Log($"[CombatSystem] ToggleAutoCombat: {_isAutoCombatMode}");
    }

    /// <summary>
    /// Caller: Input Action "ToggleAutoBurst" (Tab)
    /// Intent: 버스트 스킬 자동 발동 토글
    /// </summary>
    private void OnToggleAutoBurst()
    {
        _isAutoBurstMode = !_isAutoBurstMode;
        _burstSystem?.SetAutoMode(_isAutoBurstMode);

        Debug.Log($"[CombatSystem] ToggleAutoBurst: {_isAutoBurstMode}");
    }

    private async Task InitializeHUDAsync()
    {
        if (_combatHUD == null)
        {
            // ViewModel 생성 및 데이터 주입
            _hudViewModel = new CombatHUDViewModel(_nikkes);

            // Phase 9: 버스트 시스템 연결
            if (_burstSystem != null)
            {
                var burstGaugeViewModel = new BurstGaugeViewModel(_burstSystem);
                await burstGaugeViewModel.InitializeAsync();
                _hudViewModel.BurstGauge.Value = burstGaugeViewModel;
            }

            // UI 매니저를 통해 HUD 생성
            _combatHUD = await Managers.UI.ShowAsync<UI_CombatHUD>(_hudViewModel);

            // [추가] 통합 체력바 보드 생성 및 니케 등록
            _healthBarBoard = await Managers.UI.ShowAsync<UI_HealthBarBoard>();
            if (_healthBarBoard != null)
            {
                foreach (var nikke in _nikkes)
                {
                    if (nikke != null) _healthBarBoard.RegisterEntity(nikke);
                }
            }

            if (_combatHUD == null)
            {
                Debug.LogError("[CombatSystem] Failed to load UI_CombatHUD");
                return;
            }
        }
    }

    private void OnWaveRaptureSpawned(CombatRapture rapture)
    {
        _healthBarBoard?.RegisterEntity(rapture);
    }

    private void OnWaveRaptureDied(CombatRapture rapture)
    {
        _healthBarBoard?.UnregisterEntity(rapture);
    }

    private void OnAllPhasesComplete()
    {
        EndCombat(eCombatResult.Victory);
    }


    private void EndCombat(eCombatResult result)
    {
        if (_isCombatEnded) return;
        _isCombatEnded = true;

        Debug.Log($"[CombatSystem] EndCombat: {result}");
        OnCombatEnded?.Invoke(result); // CombatScene에게 알림 (팝업 등 처리는 CombatScene이 할 수도, System이 할 수도 있음. 설계상 System이 주도)

        // 팝업 표시
        if (result == eCombatResult.Victory)
        {
            var viewModel = new CombatResultVictoryPopupViewModel(_stageGameData?.rewards ?? new List<RewardData>());
            _ = Managers.UI.ShowAsync<UI_CombatResultVictoryPopup>(viewModel);
        }
        else
        {
            var viewModel = new CombatResultDefeatPopupViewModel();
            _ = Managers.UI.ShowAsync<UI_CombatResultDefeatPopup>(viewModel);
        }
    }

    /// <summary>
    /// 디버그용 강제 전투 종료
    /// </summary>
    public void ForceEndCombat(eCombatResult result)
    {
        Debug.Log($"[CombatSystem] ForceEndCombat: {result}");
        EndCombat(result);
    }

    public CombatEntity GetEntityById(int entityId)
    {
        if (entityId >= 0 && entityId < _nikkes.Length)
            return _nikkes[entityId];

        return null;
    }

    // ==================== Input Wrappers ====================

    private void OnToggleAutoCombatWrapper(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => OnToggleAutoCombat();
    private void OnToggleAutoBurstWrapper(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => OnToggleAutoBurst();
    private void OnToggleAllCoverWrapper(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => ToggleAllCover();
    private void OnPauseWrapper(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => OnPause();

    private void OnPause()
    {
        if (_isCombatEnded) return;

        Managers.Time.PauseGame();

        // 팝업 표시 
        var viewModel = new CombatPausePopupViewModel(_hudViewModel.TimeText.Value, _statRecordSystem, _nikkes);
        _ = Managers.UI.ShowAsync<UI_CombatPausePopup>(viewModel);
    }
}
