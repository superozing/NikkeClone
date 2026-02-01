using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UI;
using UnityEngine;

/// <summary>
/// StageWaveInfoPopup ViewModel입니다.
/// 스테이지 정보, 스쿼드 니케 정보, Sub-UI ViewModel을 관리합니다.
/// </summary>
public class StageWaveInfoPopupViewModel : ViewModelBase
{
    // --- 스테이지 정보 ---
    public ReactiveProperty<string> StageTypeName { get; private set; } = new("");
    public ReactiveProperty<string> StageName { get; private set; } = new("");

    // --- 스쿼드 니케 아이콘 ViewModel (5개 고정) ---
    public NikkeIconViewModel[] NikkeIcons { get; private set; }

    // --- Sub-UI ViewModels ---
    public StageWeakCodeInfoViewModel WeakCodeInfo { get; private set; }
    public StageRangeInfoViewModel RangeInfo { get; private set; }

    // --- 랩쳐 아이콘 리스트 ---
    public List<RaptureIconViewModel> SoldierIconViewModels { get; private set; } = new();
    public List<RaptureIconViewModel> SubTargetIconViewModels { get; private set; } = new();
    public List<RaptureIconViewModel> TargetIconViewModels { get; private set; } = new();

    // --- 이벤트 ---
    public event Action OnCloseRequested;

    // 스쿼드 ID (갱신 시 필요)
    private int _squadId = 1;

    /// <summary>
    /// 생성자. NikkeIconViewModel 배열 및 Sub-UI ViewModel을 초기화합니다.
    /// </summary>
    public StageWaveInfoPopupViewModel()
    {
        // NikkeIconViewModel 5개 생성
        NikkeIcons = new NikkeIconViewModel[5];
        for (int i = 0; i < 5; ++i)
        {
            NikkeIcons[i] = new NikkeIconViewModel();
            NikkeIcons[i].AddRef();
        }

        // Sub-UI ViewModel 초기화
        WeakCodeInfo = new StageWeakCodeInfoViewModel();
        WeakCodeInfo.AddRef();
        RangeInfo = new StageRangeInfoViewModel();
        RangeInfo.AddRef();
    }

    /// <summary>
    /// 데이터를 초기화합니다.
    /// </summary>
    /// <param name="stageId">스테이지 ID</param>
    /// <param name="squadId">스쿼드 ID (기본값: 1)</param>
    public async Task Initialize(int stageId, int squadId = 1)
    {
        _squadId = squadId;

        // 1. StageGameData 조회
        var stageData = Managers.Data.Get<StageGameData>(stageId);
        if (stageData == null)
        {
            Debug.LogError($"[StageWaveInfoPopupViewModel] StageGameData not found: {stageId}");
            return;
        }

        StageTypeName.Value = GetStageTypeName(stageData.stageType);
        StageName.Value = stageData.name;

        // 2. 스쿼드 데이터 조회 및 NikkeIcon 초기화
        await InitializeNikkeIcons(squadId);

        // 3. Sub-UI 데이터 설정
        WeakCodeInfo.SetData(stageData.weaknessCode, NikkeIcons);
        RangeInfo.SetData(NikkeIcons);

        // 4. StageBattleGameData 조회 및 랩쳐 데이터 설정
        var battleData = Managers.Data.Get<StageBattleGameData>(stageData.stageBattleDataId);

        // 기존 VM 리소스 해제
        foreach (var vm in SoldierIconViewModels) vm.Release();
        foreach (var vm in SubTargetIconViewModels) vm.Release();
        foreach (var vm in TargetIconViewModels) vm.Release();

        SoldierIconViewModels.Clear();
        SubTargetIconViewModels.Clear();
        TargetIconViewModels.Clear();

        if (battleData != null && battleData.appearingRaptureIds != null)
        {
            foreach (var raptureId in battleData.appearingRaptureIds)
            {
                var raptureData = Managers.Data.Get<RaptureGameData>(raptureId);
                if (raptureData == null) continue;

                var iconVM = new RaptureIconViewModel(raptureId);
                iconVM.AddRef(); // 자식 ViewModel 생명주기 관리

                switch (raptureData.grade)
                {
                    case 1: SoldierIconViewModels.Add(iconVM); break;   // Soldier
                    case 2: SubTargetIconViewModels.Add(iconVM); break; // SubTarget
                    case 3: TargetIconViewModels.Add(iconVM); break;    // Target
                    default:
                        // 예외 케이스: 일단 Soldier로 취급하거나 버림
                        SoldierIconViewModels.Add(iconVM);
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 스쿼드 ID를 기반으로 NikkeIconViewModel 배열을 초기화합니다.
    /// </summary>
    private async Task InitializeNikkeIcons(int squadId)
    {
        if (Managers.Data.UserData.Squads.TryGetValue(squadId, out var squadData))
        {
            for (int i = 0; i < 5; ++i)
            {
                int nikkeId = (squadData.slot != null && i < squadData.slot.Count) ? squadData.slot[i] : -1;
                await NikkeIcons[i].SetNikke(nikkeId);
            }
        }
        else
        {
            for (int i = 0; i < 5; ++i)
            {
                await NikkeIcons[i].SetNikke(-1);
            }
        }
    }

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

    public void RequestClose()
    {
        OnCloseRequested?.Invoke();
    }

    protected override void OnDispose()
    {
        // NikkeIconViewModel 해제
        if (NikkeIcons != null)
        {
            foreach (var vm in NikkeIcons)
            {
                vm?.Release();
            }
        }

        // Sub-UI ViewModel 해제
        WeakCodeInfo?.Release();
        RangeInfo?.Release();

        // 기존 VM 리소스 해제
        foreach (var vm in SoldierIconViewModels) vm.Release();
        foreach (var vm in SubTargetIconViewModels) vm.Release();
        foreach (var vm in TargetIconViewModels) vm.Release();

        SoldierIconViewModels.Clear();
        SubTargetIconViewModels.Clear();
        TargetIconViewModels.Clear();


        base.OnDispose();
    }
}
