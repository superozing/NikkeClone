using System;
using UnityEngine;
using NikkeClone.Utils;

/// <summary>
/// 버스트 시스템의 상태와 흐름을 관리하는 매니저 클래스입니다.
/// 게이지 충전, 단계 전이, 풀버스트 상태를 통제합니다.
/// </summary>
public class CombatBurstSystem
{
    // ==================== Fields & Properties ====================

    private readonly CombatNikke[] _nikkes;
    private BurstSlotData[] _slotData;

    private class BurstSlotData
    {
        public int BurstLevel;
        public SkillData SkillData;
        public ReactiveProperty<float> CooldownRemaining = new(0f);
        public float CooldownTotal;
    }

    private float _gauge;
    private eBurstStage _currentStage = eBurstStage.None;
    private bool _isAutoMode;

    // 단계 전이 쿨다운: HandleAutoBurst()의 매 프레임 체이닝 방지
    private float _stageTransitionCooldown;
    private const float STAGE_TRANSITION_COOLDOWN = 0.5f;

    private float _fullBurstDuration = 10.0f;
    private float _fullBurstTimer = 0.0f;

    public ReactiveProperty<float> Gauge { get; } = new(0f);
    public ReactiveProperty<eBurstStage> CurrentStage { get; } = new(eBurstStage.None);
    public ReactiveProperty<bool> IsFullBurst { get; } = new(false);

    // ==================== Events ====================

    /// <summary>
    /// 버스트 스킬이 실제로 발동되었을 때 발생하는 이벤트 (NikkeSlotIndex, Stage)
    /// </summary>
    public event Action<int, eBurstStage> OnBurstTriggered;

    /// <summary>
    /// 버스트 단계가 변경되었을 때 발생하는 이벤트
    /// </summary>
    public event Action<eBurstStage> OnStageChanged;

    /// <summary>
    /// 풀버스트 모드가 시작되었을 때 발생하는 이벤트
    /// </summary>
    public event Action OnFullBurstStarted;

    /// <summary>
    /// 풀버스트 모드가 종료되었을 때 발생하는 이벤트
    /// </summary>
    public event Action OnFullBurstEnded;

    // ==================== Public Methods ====================

    public CombatBurstSystem(CombatNikke[] nikkes)
    {
        _nikkes = nikkes;
        _slotData = new BurstSlotData[nikkes.Length];
    }

    /// <summary>
    /// 슬롯별 버스트 데이터를 초기화합니다.
    /// </summary>
    public void Initialize(NikkeGameData[] gameDatas)
    {
        for (int i = 0; i < _slotData.Length; i++)
        {
            _slotData[i] = new BurstSlotData();
        }

        for (int i = 0; i < _nikkes.Length && i < gameDatas.Length; i++)
        {
            if (gameDatas[i] == null) continue;

            _slotData[i].BurstLevel = gameDatas[i].burstLevel;
            _slotData[i].SkillData = gameDatas[i].skills.Find(s => s.burstStage == gameDatas[i].burstLevel);
            _slotData[i].CooldownTotal = _slotData[i].SkillData?.cooldown ?? 20f;
            _slotData[i].CooldownRemaining.Value = 0f;
        }
    }

    /// <summary>
    /// 게이지를 충전합니다. 
    /// 풀버스트 중이거나 현재 대기 중인 단계가 있으면 충전되지 않습니다.
    /// </summary>
    public void AddGauge(float amount)
    {
        if (IsFullBurst.Value || CurrentStage.Value != eBurstStage.None)
            return;

        _gauge = Mathf.Clamp01(_gauge + amount);
        Gauge.Value = _gauge;

        if (_gauge >= 1.0f)
        {
            SetStage(eBurstStage.Stage1);
        }
    }

    /// <summary>
    /// 특정 슬롯의 니케에게 버스트 사용을 요청합니다.
    /// </summary>
    public void RequestBurst(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _nikkes.Length) return;
        var nikke = _nikkes[slotIndex];
        if (nikke == null) return;

        if (!CanUseBurst(slotIndex)) return;

        eBurstStage triggeredStage = CurrentStage.Value;

        // 쿨타임 설정 및 스킬 실행 통보
        _slotData[slotIndex].CooldownRemaining.Value = _slotData[slotIndex].CooldownTotal;
        OnBurstTriggered?.Invoke(slotIndex, triggeredStage);

        // Phase 9: 로그 출력 (Phase 14에서 연출 트리거로 확장 가능)
        if (_slotData[slotIndex].SkillData != null)
        {
            Debug.Log($"<color=orange>[Burst]</color> <b>{nikke.NikkeName}</b> executed Burst Skill: <b>{_slotData[slotIndex].SkillData.name}</b> at {triggeredStage}");
        }

        // 단계 전이
        TransitionToNextStage();
    }

    public void Tick(float deltaTime)
    {
        // 1. 풀버스트 타이머 관리
        if (IsFullBurst.Value)
        {
            _fullBurstTimer -= deltaTime;
            if (_fullBurstTimer <= 0)
            {
                EndFullBurst();
            }
        }

        // 2. 니케 쿨타임 Tick
        for (int i = 0; i < _slotData.Length; i++)
        {
            if (_slotData[i].CooldownRemaining.Value > 0)
            {
                _slotData[i].CooldownRemaining.Value -= deltaTime;
            }
        }

        // 단계 전이 쿨다운 Tick
        if (_stageTransitionCooldown > 0f)
            _stageTransitionCooldown -= deltaTime;

        // 3. 자동 전투 대응
        if (_isAutoMode)
        {
            HandleAutoBurst();
        }
    }

    public void SetAutoMode(bool isAuto)
    {
        _isAutoMode = isAuto;
    }

    public void Cleanup()
    {
        OnBurstTriggered = null;
        OnStageChanged = null;
    }

    /// <summary>
    /// 전투 종료 시 풀버스트가 진행 중이라면 즉시 종료합니다.
    /// Caller: CombatSystem.EndCombat()
    /// </summary>
    public void ForceEndFullBurst()
    {
        if (IsFullBurst.Value)
        {
            EndFullBurst();
        }
    }

    /// <summary>
    /// 해당 슬롯의 니케가 현재 단계에서 버스트 스킬을 사용할 수 있는지 확인합니다.
    /// </summary>
    public bool CanUseBurst(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _nikkes.Length) return false;
        var nikke = _nikkes[slotIndex];
        if (nikke == null || nikke.IsDead) return false;

        // 1. 현재 버스트 단계와 니케의 버스트 레벨이 일치해야 함
        int currentStageInt = (int)CurrentStage.Value;
        if (currentStageInt < 1 || currentStageInt > 3) return false;

        if (_slotData[slotIndex].BurstLevel != currentStageInt) return false;

        // 2. 단계 전이 쿨다운 체크
        if (_stageTransitionCooldown > 0f) return false;

        // 3. 쿨타임 체크
        if (_slotData[slotIndex].CooldownRemaining.Value > 0) return false;

        return true;
    }

    public int GetBurstLevel(int slotIndex) => (slotIndex >= 0 && slotIndex < _slotData.Length) ? _slotData[slotIndex].BurstLevel : 0;
    public SkillData GetSkillData(int slotIndex) => (slotIndex >= 0 && slotIndex < _slotData.Length) ? _slotData[slotIndex].SkillData : null;
    public float GetCooldownRemaining(int slotIndex) => (slotIndex >= 0 && slotIndex < _slotData.Length) ? _slotData[slotIndex].CooldownRemaining.Value : 0f;
    public ReactiveProperty<float> GetCooldownRemainingProperty(int slotIndex) => (slotIndex >= 0 && slotIndex < _slotData.Length) ? _slotData[slotIndex].CooldownRemaining : null;
    public float GetCooldownTotal(int slotIndex) => (slotIndex >= 0 && slotIndex < _slotData.Length) ? _slotData[slotIndex].CooldownTotal : 0f;
    public string GetNikkeName(int slotIndex) => (slotIndex >= 0 && slotIndex < _nikkes.Length) ? _nikkes[slotIndex]?.NikkeName : string.Empty;
    public bool IsNikkeDead(int slotIndex) => (slotIndex >= 0 && slotIndex < _nikkes.Length) ? (_nikkes[slotIndex]?.IsDead ?? true) : true;

    // ==================== Private Methods ====================

    private void SetStage(eBurstStage stage)
    {
        if (_currentStage == stage) return;

        _currentStage = stage;
        CurrentStage.Value = _currentStage;
        OnStageChanged?.Invoke(_currentStage);
        _stageTransitionCooldown = STAGE_TRANSITION_COOLDOWN;

        Debug.Log($"[BurstManager] Stage Changed: {_currentStage}");
    }

    private void TransitionToNextStage()
    {
        switch (CurrentStage.Value)
        {
            case eBurstStage.Stage1:
                SetStage(eBurstStage.Stage2);
                break;
            case eBurstStage.Stage2:
                SetStage(eBurstStage.Stage3);
                break;
            case eBurstStage.Stage3:
                StartFullBurst();
                break;
        }
    }

    private void StartFullBurst()
    {
        SetStage(eBurstStage.FullBurst);
        IsFullBurst.Value = true;
        _fullBurstTimer = _fullBurstDuration;

        OnFullBurstStarted?.Invoke();
        Debug.Log("[BurstManager] Full Burst Started!");
    }

    private void EndFullBurst()
    {
        IsFullBurst.Value = false;
        _gauge = 0f;
        Gauge.Value = 0f;
        SetStage(eBurstStage.None);

        OnFullBurstEnded?.Invoke();
        Debug.Log("[BurstManager] Full Burst Ended.");
    }

    private void HandleAutoBurst()
    {
        if (CurrentStage.Value == eBurstStage.None || IsFullBurst.Value) return;

        // 현재 단계에서 사용 가능한 니케 중 쿨타임이 끝난 첫 번째 니케를 선택
        for (int i = 0; i < _nikkes.Length; i++)
        {
            if (CanUseBurst(i))
            {
                RequestBurst(i);
                break;
            }
        }
    }
}
