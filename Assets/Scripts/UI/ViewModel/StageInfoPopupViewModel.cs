using System;
using System.Threading.Tasks;
using UI;
using UnityEngine;

/// <summary>
/// 스테이지 정보 팝업 ViewModel입니다.
/// StageGameData와 UserSquadData를 조합하여 View에 바인딩 가능한 형태로 제공합니다.
/// </summary>
public class StageInfoPopupViewModel : ViewModelBase
{
    // --- 스테이지 정보 ---
    public int StageId { get; private set; }
    public ReactiveProperty<string> StageName { get; private set; } = new("");
    public ReactiveProperty<string> StageTypeName { get; private set; } = new("");
    public ReactiveProperty<int> ReferenceCombatPower { get; private set; } = new(0);
    public ReactiveProperty<int> CurrentSquadIndex { get; private set; } = new(0);
    public ReactiveProperty<int> CurrentCombatPower { get; private set; } = new(0);

    // --- 스쿼드 니케 아이콘 ViewModel (5개 스쿼드 × 5개 슬롯 = 25개) ---
    /// <summary>
    /// 5개 스쿼드 × 5개 슬롯 = 25개의 NikkeIconViewModel을 미리 생성하여 보관합니다.
    /// Index: [squadIndex][slotIndex] (squadIndex: 0~4, slotIndex: 0~4)
    /// </summary>
    public NikkeIconViewModel[][] AllSquadNikkeIcons { get; private set; }

    /// <summary>
    /// 현재 선택된 스쿼드의 NikkeIconViewModel 배열을 가리키는 프로퍼티입니다.
    /// UI_StageInfoPopup이 이 배열을 바인딩합니다.
    /// </summary>
    public NikkeIconViewModel[] CurrentSquadNikkeIcons => AllSquadNikkeIcons[CurrentSquadIndex.Value];

    // --- 이벤트 ---
    /// <summary>
    /// 스쿼드 편집 요청 이벤트입니다. View에서 스쿼드 버튼 클릭 시 호출됩니다.
    /// </summary>
    public event Action OnSquadEditRequested;

    /// <summary>
    /// 전투 시작 요청 이벤트입니다. stageId를 인자로 전달합니다.
    /// </summary>
    public event Action<int> OnBattleRequested;

    /// <summary>
    /// 팝업 닫기 요청 이벤트입니다.
    /// </summary>
    public event Action OnCloseRequested;

    /// <summary>
    /// 니케 상세 정보 팝업 요청 이벤트입니다. NikkeId를 인자로 전달합니다.
    /// </summary>
    public event Action<int> OnNikkeDetailRequested;

    private int _squadId;
    private bool _isSquadsLoaded = false; // 스쿼드 데이터 로드 완료 플래그

    // --- Sub-UI ViewModels ---
    public StageWeakCodeInfoViewModel WeakCodeInfo { get; private set; }
    public StageRangeInfoViewModel RangeInfo { get; private set; }
    public StageRewardInfoViewModel RewardInfo { get; private set; }

    /// <summary>
    /// StageInfoPopupViewModel 생성자입니다.
    /// NikkeIconViewModel 25개와 Sub-UI ViewModel들을 미리 생성하고 참조를 유지합니다.
    /// </summary>
    public StageInfoPopupViewModel()
    {
        // 5개 스쿼드 × 5개 슬롯 = 25개 ViewModel 생성
        AllSquadNikkeIcons = new NikkeIconViewModel[5][];
        for (int squadIdx = 0; squadIdx < 5; ++squadIdx)
        {
            AllSquadNikkeIcons[squadIdx] = new NikkeIconViewModel[5];
            for (int slotIdx = 0; slotIdx < 5; ++slotIdx)
            {
                var iconVM = new DisplayNikkeIconViewModel();
                AllSquadNikkeIcons[squadIdx][slotIdx] = iconVM;
                iconVM.AddRef(); // 부모 ViewModel이 자식을 소유

                // Event Subscription
                // 람다 내부에서 캡쳐하는 변수는 루프 변수 복사본 사용
                int capturedSlotIdx = slotIdx;
                iconVM.OnEditRequest += RequestSquadEdit;
                iconVM.OnDetailRequest += () => RequestNikkeDetail(capturedSlotIdx);
            }
        }

        // Sub-UI ViewModel 초기화
        WeakCodeInfo = new StageWeakCodeInfoViewModel();
        WeakCodeInfo.AddRef();
        RangeInfo = new StageRangeInfoViewModel();
        RangeInfo.AddRef();
        RewardInfo = new StageRewardInfoViewModel();
        RewardInfo.AddRef();
    }

    /// <summary>
    /// 스테이지 및 스쿼드 데이터를 초기화합니다.
    /// </summary>
    /// <param name="stageId">표시할 스테이지 ID</param>
    /// <param name="squadId">현재 스쿼드 ID (UserSquadData 조회용)</param>
    public async Task Initialize(int stageId, int squadId)
    {
        StageId = stageId;
        _squadId = squadId;

        // 1. StageGameData 조회
        var stageData = Managers.Data.Get<StageGameData>(stageId);
        if (stageData == null)
        {
            Debug.LogWarning($"[StageInfoPopupViewModel] StageGameData not found for id: {stageId}");
            return;
        }

        StageName.Value = stageData.name;
        StageTypeName.Value = GetStageTypeName(stageData.stageType);
        ReferenceCombatPower.Value = stageData.referenceCombatPower;

        // 2. 스쿼드 데이터 로드 (최초 1회만)
        if (!_isSquadsLoaded)
        {
            await LoadAllSquadsAsync();
            _isSquadsLoaded = true;
        }

        // 3. 초기 스쿼드 선택 (index = squadId - 1)
        // SelectSquad는 더이상 async가 아님 (데이터가 이미 로드되어 있으므로)
        SelectSquad(squadId - 1);

        // 4. Sub-UI 데이터 설정 -> SelectSquad 내부에서 갱신하므로 초기화 시에는 자동 갱신됨 (RewardInfo 제외)
        RewardInfo.SetData(stageData.rewards);
    }

    /// <summary>
    /// 5개 스쿼드의 모든 Nikke 데이터를 미리 로드합니다.
    /// </summary>
    private async Task LoadAllSquadsAsync()
    {
        for (int squadIdx = 0; squadIdx < 5; ++squadIdx)
        {
            int targetSquadId = squadIdx + 1;

            if (Managers.Data.UserData.Squads.TryGetValue(targetSquadId, out var squadData))
            {
                // 각 슬롯 병렬 로드 (더 빠름)
                var loadTasks = new Task[5];
                for (int slotIdx = 0; slotIdx < 5; ++slotIdx)
                {
                    int nikkeId = (squadData.slot != null && slotIdx < squadData.slot.Count)
                        ? squadData.slot[slotIdx]
                        : -1;

                    loadTasks[slotIdx] = AllSquadNikkeIcons[squadIdx][slotIdx].SetNikke(nikkeId);
                }

                await Task.WhenAll(loadTasks);
            }
            else
            {
                Debug.LogWarning($"[StageInfoPopupViewModel] UserSquadData not found for id: {targetSquadId}");

                // 빈 슬롯으로 초기화
                for (int slotIdx = 0; slotIdx < 5; ++slotIdx)
                {
                    await AllSquadNikkeIcons[squadIdx][slotIdx].SetNikke(-1);
                }
            }
        }
    }

    /// <summary>
    /// 스쿼드 데이터를 강제로 다시 로드합니다.
    /// 유저가 스쿼드를 편집한 후 호출합니다.
    /// </summary>
    public async Task RefreshSquads()
    {
        await LoadAllSquadsAsync();
        // 현재 선택된 스쿼드 UI 갱신
        SelectSquad(CurrentSquadIndex.Value);
    }

    /// <summary>
    /// 스쿼드를 선택하고 UI를 갱신합니다.
    /// 데이터가 미리 로드되어 있으므로 동기적으로 처리됩니다.
    /// </summary>
    /// <param name="index">선택할 스쿼드 인덱스 (0~4)</param>
    public void SelectSquad(int index)
    {
        index = Mathf.Clamp(index, 0, 4);
        CurrentSquadIndex.Value = index;

        // 전투력 계산
        int targetSquadId = index + 1;
        long totalCp = 0;

        if (Managers.Data.UserData.Squads.TryGetValue(targetSquadId, out var squadData))
        {
            for (int i = 0; i < 5; ++i)
            {
                int nikkeId = (squadData.slot != null && i < squadData.slot.Count) ? squadData.slot[i] : -1;

                // 전투력 합산
                if (nikkeId != -1 && Managers.Data.UserData.Nikkes.TryGetValue(nikkeId, out var userNikke))
                {
                    totalCp += userNikke.combatPower.Value;
                }
            }
        }

        CurrentCombatPower.Value = (int)totalCp;

        // Sub-UI 갱신 (현재 선택된 스쿼드 기준)
        WeakCodeInfo?.SetData(Managers.Data.Get<StageGameData>(StageId).weaknessCode, CurrentSquadNikkeIcons);
        RangeInfo?.SetData(CurrentSquadNikkeIcons);
        // Reward는 변경 없음
    }

    /// <summary>
    /// eStageType을 한글명으로 변환합니다.
    /// </summary>
    private string GetStageTypeName(eStageType stageType)
    {
        return stageType switch
        {
            eStageType.Annihilation => "섬멸전",
            eStageType.Interception => "저지전",
            eStageType.Defense => "거점방어전",
            _ => "알 수 없음"
        };
    }

    /// <summary>
    /// 스쿼드 편집 버튼 클릭 처리입니다.
    /// </summary>
    public void RequestSquadEdit()
    {
        OnSquadEditRequested?.Invoke();
    }

    /// <summary>
    /// 전투 시작 버튼 클릭 처리입니다.
    /// </summary>
    public async void RequestBattle()
    {
        // 1. 유저 데이터에 전투 정보 저장
        // SquadId는 1부터 시작하므로 Index + 1
        var combatData = new UserCombatData(
            StageId,
            CurrentSquadIndex.Value + 1
        );
        Managers.Data.UserData.Combat = combatData;

        // 2. 전투 씬으로 전환
        await Managers.Scene.LoadSceneAsync(eSceneType.CombatScene);

        // 기존 이벤트 호출 유지 (필요한 경우)
        OnBattleRequested?.Invoke(StageId);
    }

    /// <summary>
    /// 닫기 버튼 클릭 처리입니다.
    /// </summary>
    public void RequestClose()
    {
        OnCloseRequested?.Invoke();
    }

    /// <summary>
    /// 니케 상세 정보 요청 처리입니다.
    /// </summary>
    public void RequestNikkeDetail(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= 5) return;

        var iconVM = CurrentSquadNikkeIcons[slotIndex];
        if (iconVM != null && !iconVM.IsSlotEmpty && iconVM.NikkeId != -1)
        {
            OnNikkeDetailRequested?.Invoke(iconVM.NikkeId);
        }
    }

    /// <summary>
    /// ViewModel 해제 시 자식 NikkeIconViewModel들을 Release합니다.
    /// </summary>
    protected override void OnDispose()
    {
        if (AllSquadNikkeIcons != null)
        {
            for (int squadIdx = 0; squadIdx < 5; ++squadIdx)
            {
                for (int slotIdx = 0; slotIdx < 5; ++slotIdx)
                {
                    AllSquadNikkeIcons[squadIdx][slotIdx]?.Release();
                }
            }
        }

        // Sub-UI ViewModel 해제
        WeakCodeInfo?.Release();
        RangeInfo?.Release();
        RewardInfo?.Release();
    }
}
