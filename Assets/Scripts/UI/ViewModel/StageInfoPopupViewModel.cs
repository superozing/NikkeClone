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

    // --- 스쿼드 니케 아이콘 ViewModel (5개 고정) ---
    public NikkeIconViewModel[] NikkeIcons { get; private set; }

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

    private int _squadId;

    // --- Sub-UI ViewModels ---
    public StageWeakCodeInfoViewModel WeakCodeInfo { get; private set; }
    public StageRangeInfoViewModel RangeInfo { get; private set; }
    public StageRewardInfoViewModel RewardInfo { get; private set; }

    /// <summary>
    /// StageInfoPopupViewModel 생성자입니다.
    /// NikkeIconViewModel 5개와 Sub-UI ViewModel들을 미리 생성하고 참조를 유지합니다.
    /// </summary>
    public StageInfoPopupViewModel()
    {
        NikkeIcons = new NikkeIconViewModel[5];
        for (int i = 0; i < 5; ++i)
        {
            NikkeIcons[i] = new NikkeIconViewModel();
            NikkeIcons[i].AddRef(); // 부모 ViewModel이 자식을 소유
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

        // 2. 초기 스쿼드 선택 (index = squadId - 1)
        await SelectSquad(squadId - 1);

        // Sub-UI 데이터 설정 -> SelectSquad 내부에서 갱신하므로 초기화 시에는 자동 갱신됨 (RewardInfo 제외)
        RewardInfo.SetData(stageData.rewards);
    }

    /// <summary>
    /// 스쿼드를 선택하고 UI를 갱신합니다.
    /// </summary>
    /// <param name="index">선택할 스쿼드 인덱스 (0~4)</param>
    public async Task SelectSquad(int index)
    {
        index = Mathf.Clamp(index, 0, 4);
        CurrentSquadIndex.Value = index;

        int targetSquadId = index + 1;
        long totalCp = 0;

        if (Managers.Data.UserData.Squads.TryGetValue(targetSquadId, out var squadData))
        {
            for (int i = 0; i < 5; ++i)
            {
                int nikkeId = (squadData.slot != null && i < squadData.slot.Count) ? squadData.slot[i] : -1;
                await NikkeIcons[i].SetNikke(nikkeId);

                // 전투력 합산
                if (nikkeId != -1 && Managers.Data.UserData.Nikkes.TryGetValue(nikkeId, out var userNikke))
                {
                    totalCp += userNikke.combatPower.Value;
                }
            }
        }
        else
        {
            Debug.LogWarning($"[StageInfoPopupViewModel] UserSquadData not found for id: {targetSquadId}");
            for (int i = 0; i < 5; ++i)
            {
                await NikkeIcons[i].SetNikke(-1);
            }
        }

        CurrentCombatPower.Value = (int)totalCp;

        // Sub-UI 갱신
        WeakCodeInfo?.SetData(Managers.Data.Get<StageGameData>(StageId).weaknessCode, NikkeIcons);
        RangeInfo?.SetData(NikkeIcons);
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
    public void RequestBattle()
    {
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
    /// ViewModel 해제 시 자식 NikkeIconViewModel들을 Release합니다.
    /// </summary>
    protected override void OnDispose()
    {
        if (NikkeIcons != null)
        {
            foreach (var icon in NikkeIcons)
            {
                icon?.Release();
            }
        }

        // Sub-UI ViewModel 해제
        WeakCodeInfo?.Release();
        RangeInfo?.Release();
        RewardInfo?.Release();
    }
}
