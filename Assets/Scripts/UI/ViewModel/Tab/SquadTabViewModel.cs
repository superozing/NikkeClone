using System;
using System.Collections.Generic;
using UI;
using UnityEngine;

public class SquadTabViewModel : ViewModelBase
{
    // --- Reactive Properties ---
    /// <summary>
    /// 현재 선택된 스쿼드 인덱스 (0 ~ 4)
    /// </summary>
    public ReactiveProperty<int> CurrentSquadIndex { get; private set; } = new(0);

    /// <summary>
    /// 현재 스쿼드의 총 전투력 텍스트
    /// </summary>
    public ReactiveProperty<string> TotalCombatPower { get; private set; } = new("0");

    // --- Data ---
    /// <summary>
    /// 현재 선택된 스쿼드의 5개 슬롯에 해당하는 카드 뷰모델 배열.
    /// 빈 슬롯은 null입니다. View는 이 배열을 참조하여 UI를 갱신합니다.
    /// </summary>
    public NikkeCardViewModel[] SlotViewModels { get; private set; }

    // --- Caching ---
    /// <summary>
    /// 스쿼드별(5개) 슬롯별(5개) ViewModel을 캐싱하는 2차원 배열입니다.
    /// Lazy Caching 방식을 사용하므로 초기에는 null이며, 접근 시 생성됩니다.
    /// </summary>
    private NikkeCardViewModel[][] _cachedSquadViewModels = new NikkeCardViewModel[5][];

    private readonly Dictionary<int, UserSquadData> _userSquads;

    public SquadTabViewModel()
    {
        // 유저 스쿼드 데이터 참조
        _userSquads = Managers.Data.UserData.Squads;

        // 기본적으로 0번(첫 번째) 스쿼드를 선택합니다.
        SelectSquad(0);
    }

    /// <summary>
    /// View에서 스쿼드 선택 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void OnClickSquadButton(int index)
    {
        if (CurrentSquadIndex.Value == index)
            return;

        SelectSquad(index);
    }

    private void SelectSquad(int index)
    {
        // 1. 캐시 확인 및 생성 (Lazy Loading)
        if (_cachedSquadViewModels[index] == null)
        {
            CreateSquadViewModelCache(index);
        }

        // 2. 현재 슬롯 뷰모델 교체 (캐시된 배열 참조)
        SlotViewModels = _cachedSquadViewModels[index];

        // 3. 전투력 계산
        long totalCp = 0;
        foreach (var vm in SlotViewModels)
        {
            if (vm != null)
                totalCp += vm.CombatPower;
        }
        TotalCombatPower.Value = Utils.FormatNumber((int)totalCp);

        // 4. 인덱스 변경 알림 (View 갱신 트리거)
        // View는 OnSquadIndexChanged에서 SlotViewModels를 읽어가므로 순서가 중요합니다.
        CurrentSquadIndex.Value = index;
    }

    /// <summary>
    /// 해당 인덱스의 스쿼드 데이터를 로드하여 ViewModel 캐시를 생성합니다.
    /// </summary>
    private void CreateSquadViewModelCache(int index)
    {
        _cachedSquadViewModels[index] = new NikkeCardViewModel[5];

        int squadId = index + 1; // ID는 1부터 시작

        if (_userSquads.TryGetValue(squadId, out UserSquadData squadData))
        {
            for (int i = 0; i < 5; i++)
            {
                int nikkeId = squadData.slot[i];

                if (nikkeId != -1) // -1은 빈 슬롯
                {
                    var nikkeGameData = Managers.Data.Get<NikkeGameData>(nikkeId);
                    var userNikkeData = Managers.Data.UserData.Nikkes.ContainsKey(nikkeId) ? Managers.Data.UserData.Nikkes[nikkeId] : null;

                    if (nikkeGameData != null && userNikkeData != null)
                    {
                        var vm = new NikkeCardViewModel(userNikkeData, nikkeGameData);
                        vm.AddRef(); // 캐시에서 보유하므로 참조 카운트 증가
                        vm.OnClick += OnCardViewModelClicked;

                        _cachedSquadViewModels[index][i] = vm;
                    }
                }
                else
                {
                    _cachedSquadViewModels[index][i] = null;
                }
            }
        }
    }

    // --- Interaction Methods ---

    public void OnClickSkill(int slotIndex)
    {
        int nikkeId = GetNikkeIdFromCurrentSquad(slotIndex);
        if (nikkeId == -1) return;

        ShowSkillPopup(nikkeId);
    }

    public void OnClickDetail(int slotIndex)
    {
        int nikkeId = GetNikkeIdFromCurrentSquad(slotIndex);
        if (nikkeId == -1) return;

        ShowDetailPopup(nikkeId);
    }

    public void OnClickAutoFormation()
    {
        Debug.Log("[SquadTabViewModel] 자동 편성 버튼 클릭됨 (구현 예정)");
    }

    /// <summary>
    /// 니케 카드가 클릭되었을 때 호출됩니다.
    /// </summary>
    private void OnCardViewModelClicked(int nikkeId)
    {
        Debug.Log($"[SquadTabViewModel] 카드 클릭됨: NikkeID({nikkeId}). UI_SquadDetailPopup 생성 요청 (로그)");
    }

    // --- Helpers ---

    private int GetNikkeIdFromCurrentSquad(int slotIndex)
    {
        // 현재 선택된 스쿼드의 데이터에서 ID 조회
        if (SlotViewModels != null && slotIndex >= 0 && slotIndex < SlotViewModels.Length)
        {
            var vm = SlotViewModels[slotIndex];
            if (vm != null)
                return vm.NikkeId;
        }
        return -1;
    }

    private async void ShowSkillPopup(int nikkeId)
    {
        SkillInfoPopupViewModel vm = new SkillInfoPopupViewModel();
        vm.SetData(nikkeId);
        await Managers.UI.ShowAsync<UI_SkillInfoPopup>(vm);
    }

    private async void ShowDetailPopup(int nikkeId)
    {
        NikkeDetailPopupViewModel vm = new NikkeDetailPopupViewModel();
        await vm.SetNikkeID(nikkeId);
        await Managers.UI.ShowAsync<UI_NikkeDetailPopup>(vm);
    }

    protected override void OnDispose()
    {
        // 캐싱된 모든 ViewModel 해제
        if (_cachedSquadViewModels != null)
        {
            for (int i = 0; i < _cachedSquadViewModels.Length; i++)
            {
                var squadVMs = _cachedSquadViewModels[i];
                if (squadVMs != null)
                {
                    foreach (var vm in squadVMs)
                    {
                        if (vm != null)
                        {
                            vm.OnClick -= OnCardViewModelClicked;
                            vm.Release(); // AddRef에 대한 Release
                        }
                    }
                }
                _cachedSquadViewModels[i] = null;
            }
            _cachedSquadViewModels = null;
        }

        SlotViewModels = null;
    }
}