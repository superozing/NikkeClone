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
    /// 5개의 슬롯에 해당하는 카드 뷰모델 배열. 빈 슬롯은 null입니다.
    /// </summary>
    public NikkeCardViewModel[] SlotViewModels { get; private set; } = new NikkeCardViewModel[5];

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
        // 1. 기존 슬롯의 ViewModel 정리
        ClearSlots();

        // 2. 새 스쿼드 데이터 조회 (ID는 1부터 시작한다고 가정: index + 1)
        int squadId = index + 1;
        long totalCp = 0;

        if (_userSquads.TryGetValue(squadId, out UserSquadData squadData))
        {
            // 3. 5개 슬롯을 순회하며 ViewModel 생성
            for (int i = 0; i < 5; i++)
            {
                int nikkeId = squadData.slot[i];

                // -1은 빈 슬롯을 의미함
                if (nikkeId != -1)
                {
                    // 데이터 조회
                    var nikkeGameData = Managers.Data.Get<NikkeGameData>(nikkeId);
                    var userNikkeData = Managers.Data.UserData.Nikkes.ContainsKey(nikkeId) ? Managers.Data.UserData.Nikkes[nikkeId] : null;

                    if (nikkeGameData != null && userNikkeData != null)
                    {
                        var vm = new NikkeCardViewModel(userNikkeData, nikkeGameData);
                        vm.AddRef();

                        // 카드 클릭 이벤트 구독
                        vm.OnClick += OnCardViewModelClicked;

                        SlotViewModels[i] = vm;

                        // 전투력 합산
                        totalCp += userNikkeData.combatPower.Value;
                    }
                }
                else
                {
                    SlotViewModels[i] = null;
                }
            }
        }

        // 4. 전투력 텍스트 갱신
        TotalCombatPower.Value = Utils.FormatNumber((int)totalCp);

        // 5. 인덱스 변경 알림 (View 갱신 트리거)
        CurrentSquadIndex.Value = index;
    }

    private void ClearSlots()
    {
        for (int i = 0; i < SlotViewModels.Length; i++)
        {
            if (SlotViewModels[i] != null)
            {
                SlotViewModels[i].OnClick -= OnCardViewModelClicked; // 구독 해제
                SlotViewModels[i].Release();
                SlotViewModels[i] = null;
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
        Debug.Log($"[SquadTabViewModel] 카드 클릭됨: NikkeID({nikkeId}). UI_SquadDetailPopup 생성해야 해요.");
    }

    // --- Helpers ---

    private int GetNikkeIdFromCurrentSquad(int slotIndex)
    {
        int squadId = CurrentSquadIndex.Value + 1;
        if (_userSquads.TryGetValue(squadId, out UserSquadData data))
        {
            if (slotIndex >= 0 && slotIndex < data.slot.Count)
                return data.slot[slotIndex];
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
        ClearSlots();
    }
}