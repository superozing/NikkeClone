using System;
using UI;
using UnityEngine;

public class NikkeLevelUpPopupViewModel : ViewModelBase
{
    public event Action OnCloseRequested;

    private NikkeGameData _gameData;
    private UserNikkeData _userData;
    private UserItemData _creditData;

    // --- Constants (Stat Increase & Cost) ---
    private const int INC_HP = 675;
    private const int INC_ATK = 30;
    private const int INC_DEF = 3;
    private const int COST_PER_LEVEL_MULTIPLIER = 1000; // 레벨 * 1000
    private const int MAX_LEVEL_LIMIT = 200;

    // --- View Binding Properties ---

    // 1. Level Texts
    public ReactiveProperty<string> CurrentLevelStr { get; private set; } = new("");
    public ReactiveProperty<string> NextLevelStr { get; private set; } = new("");
    public ReactiveProperty<string> TargetLevelStr { get; private set; } = new("");

    // 2. Stat Texts (Value & Increase)
    // Value: 현재 전체 수치 (_gameData.hp 등 베이스 포함)
    // Inc: 목표 레벨 달성 시 증가하는 수치
    public ReactiveProperty<string> StatHpValue { get; private set; } = new("");
    public ReactiveProperty<string> StatHpInc { get; private set; } = new("");

    public ReactiveProperty<string> StatAtkValue { get; private set; } = new("");
    public ReactiveProperty<string> StatAtkInc { get; private set; } = new("");

    public ReactiveProperty<string> StatDefValue { get; private set; } = new("");
    public ReactiveProperty<string> StatDefInc { get; private set; } = new("");

    // 3. Control & State
    public ReactiveProperty<bool> IsMinusActive { get; private set; } = new(false);
    public ReactiveProperty<bool> IsPlusActive { get; private set; } = new(true);
    public ReactiveProperty<bool> IsLevelUpInteractable { get; private set; } = new(true);

    // 4. Material Info (View에서 사용)
    public int RequiredCredit { get; private set; }
    public bool HasEnoughCredit { get; private set; }

    // 내부 상태
    private int _currentLevel;
    private int _targetLevel;

    public NikkeLevelUpPopupViewModel()
    {
    }

    public void SetNikke(int nikkeId)
    {
        _gameData = Managers.Data.Get<NikkeGameData>(nikkeId);
        if (Managers.Data.UserData.Nikkes.TryGetValue(nikkeId, out var userNikke))
        {
            _userData = userNikke;
        }

        // 크레딧 데이터 참조
        if (!Managers.Data.UserData.Items.TryGetValue((int)eItemType.Credit, out _creditData))
        {
            _creditData = new UserItemData((int)eItemType.Credit, 0);
        }

        if (_gameData == null || _userData == null)
        {
            Debug.LogError($"[NikkeLevelUpPopupViewModel] 데이터 로드 실패. ID: {nikkeId}");
            return;
        }

        _currentLevel = _userData.level.Value;
        SetTargetLevel(_currentLevel + 1); // 기본 1업 상태로 진입
    }

    private void SetTargetLevel(int newTarget)
    {
        newTarget = Mathf.Clamp(newTarget, _currentLevel + 1, MAX_LEVEL_LIMIT);
        _targetLevel = newTarget;
        UpdateUI();
    }

    private void UpdateUI()
    {
        // 1. Level Texts
        CurrentLevelStr.Value = _currentLevel.ToString();
        NextLevelStr.Value = _targetLevel.ToString();
        TargetLevelStr.Value = $"Lv.{_targetLevel}";

        // 2. Buttons Active State
        IsMinusActive.Value = _targetLevel > _currentLevel + 1;
        IsPlusActive.Value = _targetLevel < MAX_LEVEL_LIMIT;

        // 3. Stats Calculation 
        // 현재 전체 HP = Base(_gameData.status.hp) + (CurrentLevel - 1) * INC
        int curHp = (int)_gameData.status.hp + (_currentLevel - 1) * INC_HP;
        int curAtk = (int)_gameData.status.attack + (_currentLevel - 1) * INC_ATK;
        int curDef = (int)_gameData.status.defense + (_currentLevel - 1) * INC_DEF;

        // 목표 전체 HP
        int nextHpTotal = (int)_gameData.status.hp + (_targetLevel - 1) * INC_HP;
        int nextAtkTotal = (int)_gameData.status.attack + (_targetLevel - 1) * INC_ATK;
        int nextDefTotal = (int)_gameData.status.defense + (_targetLevel - 1) * INC_DEF;

        // UI 반영 (Value는 현재 수치, Inc는 증가량)
        StatHpValue.Value = curHp.ToString();
        StatHpInc.Value = $"+{nextHpTotal - curHp}";

        StatAtkValue.Value = curAtk.ToString();
        StatAtkInc.Value = $"+{nextAtkTotal - curAtk}";

        StatDefValue.Value = curDef.ToString();
        StatDefInc.Value = $"+{nextDefTotal - curDef}";

        // 4. Material Cost Calculation (Cumulative)
        // 각 레벨 당 필요한 재화 = 해당 레벨 * 1000
        // 예: Lv 1 -> Lv 3 : (Lv 1->2 비용) + (Lv 2->3 비용)
        // 비용 공식: 도달하려는 레벨이 L일 때 비용이 (L-1)*1000 인지, L*1000 인지?
        // 요청 사항 "레벨 * 1000"을 "현재 레벨 * 1000" 비용으로 해석하여 (Lv 1->2 갈 때 1000 소모) 계산합니다.

        long count = _targetLevel - _currentLevel;
        long levelSum = count * (_currentLevel + (_targetLevel - 1)) / 2;
        long totalCost = levelSum * COST_PER_LEVEL_MULTIPLIER;

        RequiredCredit = (int)totalCost;
        HasEnoughCredit = _creditData.count.Value >= RequiredCredit;

        // 재화 부족 시 레벨업 버튼 비활성화
        IsLevelUpInteractable.Value = HasEnoughCredit;
    }

    // --- Interaction ---

    public void OnClickMinus() => SetTargetLevel(_targetLevel - 1);
    public void OnClickPlus() => SetTargetLevel(_targetLevel + 1);
    public void OnClickMin() => SetTargetLevel(_currentLevel + 1);
    public void OnClickMax() => SetTargetLevel(MAX_LEVEL_LIMIT);

    public void OnClickInventory()
    {
        // 인벤토리 버튼 클릭 시 로그만 띄움
        Debug.Log("[NikkeLevelUpPopupViewModel] 인벤토리 버튼 클릭됨 (구현 예정)");
        /*
        // 추후 구현:
        Managers.UI.ShowAsync<UI_InventoryPopup>();
        */
    }

    public void OnClickLevelUp()
    {
        if (_userData == null) return;

        if (!HasEnoughCredit)
        {
            // 비활성화 되어 있어 눌리지 않겠지만 예외 처리
            Debug.LogWarning($"[NikkeLevelUpPopupViewModel] 크레딧 부족. 보유: {_creditData.count.Value}, 필요: {RequiredCredit}");
            return;
        }

        // 1. 재화 차감
        _creditData.count.Value -= RequiredCredit;

        int prevLevel = _userData.level.Value;

        // 2. 레벨 적용
        _userData.level.Value = _targetLevel;

        // 3. 전투력 계산 및 캐싱
        // 최종 스탯
        int finalHp = (int)_gameData.status.hp + (_targetLevel - 1) * INC_HP;
        int finalAtk = (int)_gameData.status.attack + (_targetLevel - 1) * INC_ATK;
        int finalDef = (int)_gameData.status.defense + (_targetLevel - 1) * INC_DEF;

        // 임의 공식: (HP*0.5 + Atk*2.5 + Def*1.2) * Level * 0.001
        float score = (finalHp * 0.5f) + (finalAtk * 2.5f) + (finalDef * 1.2f);
        int newCp = Mathf.FloorToInt(score * _targetLevel * 0.001f);
        if (newCp < 100) newCp = 100;

        _userData.combatPower.Value = newCp;

        Debug.Log($"[NikkeLevelUpPopupViewModel] 레벨업 완료! Lv.{prevLevel} -> Lv.{_targetLevel}. 전투력: {newCp}");

        // 4. 미션 시스템 통지
        Managers.GameSystem.MissionSystem.ReportStageClear();

        // 5. 닫기
        OnCloseRequested?.Invoke();
    }

    public void OnClickClose() => OnCloseRequested?.Invoke();
}