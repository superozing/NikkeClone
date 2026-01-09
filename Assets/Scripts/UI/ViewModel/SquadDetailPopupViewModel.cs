using System;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;

public class SquadDetailPopupViewModel : ViewModelBase
{
    public event Action OnCloseRequested;
    public event Action OnSquadDataChanged; // 슬롯 UI 갱신용

    // --- State ---
    public ReactiveProperty<int> CurrentSquadIndex { get; private set; } = new(0);
    public ReactiveProperty<string> TotalCombatPower { get; private set; } = new("0");
    public ReactiveProperty<string> SquadName { get; private set; } = new("스쿼드 1");

    // --- Child ViewModels ---
    public NikkeIconViewModel[] SlotViewModels { get; private set; }

    // 수정됨: SquadDetail 전용 구현체 사용
    public SquadDetailNikkeCardScrollViewModel ScrollViewModel { get; private set; }

    // --- Data ---
    // 편집 중인 임시 데이터 (스쿼드 ID별)
    private Dictionary<int, UserSquadData> _tempSquads = new();

    public SquadDetailPopupViewModel(int initialSquadIndex = 0)
    {
        // 1. 임시 데이터 생성 (Deep Copy)
        CloneSquadData();

        // 2. 슬롯 뷰모델 초기화
        SlotViewModels = new NikkeIconViewModel[5];
        for (int i = 0; i < 5; i++)
        {
            SlotViewModels[i] = new NikkeIconViewModel();
            SlotViewModels[i].AddRef();
        }

        // 3. 스크롤 뷰모델 초기화
        ScrollViewModel = new SquadDetailNikkeCardScrollViewModel();
        ScrollViewModel.AddRef();
        // 스크롤뷰에서 카드를 클릭하여 스쿼드가 변경되면, 슬롯 UI도 갱신해야 함
        ScrollViewModel.OnSquadChanged += OnScrollSelectionChanged;

        // 4. 초기 스쿼드 선택
        SelectSquad(initialSquadIndex);
    }

    /// <summary>
    /// DataManager의 유저 스쿼드 데이터를 복제하여 임시 저장소에 보관합니다.
    /// </summary>
    private void CloneSquadData()
    {
        _tempSquads.Clear();
        var userSquads = Managers.Data.UserData.Squads;

        // 1~5번 스쿼드 순회
        for (int i = 1; i <= 5; i++)
        {
            if (userSquads.TryGetValue(i, out var originalData))
            {
                // UserSquadData의 Clone 메서드 사용
                _tempSquads.Add(i, originalData.Clone());
            }
            else
            {
                // 없으면 빈 스쿼드 생성
                _tempSquads.Add(i, new UserSquadData(i));
            }
        }
    }

    public void SelectSquad(int index)
    {
        index = Mathf.Clamp(index, 0, 4);
        CurrentSquadIndex.Value = index;
        SquadName.Value = $"스쿼드 {index + 1}";

        // 스크롤 뷰모델에 현재 편집할 임시 데이터 주입
        int squadId = index + 1;
        if (_tempSquads.TryGetValue(squadId, out var tempSquadData))
        {
            ScrollViewModel.SetSquadData(tempSquadData);
        }

        RefreshSlots();
    }

    private void OnScrollSelectionChanged()
    {
        // 스크롤뷰 조작으로 데이터가 변했으니 슬롯 UI 갱신
        RefreshSlots();
    }

    /// <summary>
    /// 현재 선택된 스쿼드 데이터로 슬롯 아이콘들을 갱신합니다.
    /// </summary>
    private async void RefreshSlots()
    {
        int squadId = CurrentSquadIndex.Value + 1;
        if (!_tempSquads.TryGetValue(squadId, out var currentSquadData))
            return;

        var currentSlots = currentSquadData.slot;
        long totalCp = 0;

        for (int i = 0; i < 5; i++)
        {
            int nikkeId = currentSlots[i];
            await SlotViewModels[i].SetNikke(nikkeId);

            // 전투력 합산
            if (nikkeId != -1 && Managers.Data.UserData.Nikkes.TryGetValue(nikkeId, out var userNikke))
            {
                totalCp += userNikke.combatPower.Value;
            }
        }

        TotalCombatPower.Value = Utils.FormatNumber((int)totalCp);

        // 뷰 갱신 알림
        OnSquadDataChanged?.Invoke();
    }

    /// <summary>
    /// 드래그 앤 드롭으로 슬롯 간 위치를 교환하거나 이동합니다.
    /// </summary>
    public void SwapSlot(int fromIndex, int toIndex)
    {
        if (fromIndex == toIndex) return;
        if (fromIndex < 0 || fromIndex >= 5 || toIndex < 0 || toIndex >= 5) return;

        int squadId = CurrentSquadIndex.Value + 1;
        var slots = _tempSquads[squadId].slot;

        int temp = slots[fromIndex];
        slots[fromIndex] = slots[toIndex];
        slots[toIndex] = temp;

        RefreshSlots();

        // 스크롤뷰의 선택 상태도 갱신 (순서는 상관없지만 일관성 유지)
        ScrollViewModel.RefreshSelection();
    }

    public void OnClickAutoFormation()
    {
        // 자동 편성 로직 (기존과 유사하나 임시 데이터 사용)
        var userNikkes = Managers.Data.UserData.Nikkes.Values.ToList();
        var gameDataTable = Managers.Data.GetTable<NikkeGameData>();

        var sortedCandidates = userNikkes
            .Select(u => new { User = u, Game = gameDataTable[u.id] })
            .OrderByDescending(x => x.User.combatPower.Value)
            .ToList();

        HashSet<int> usedIds = new HashSet<int>();
        List<int> newSlots = new List<int> { -1, -1, -1, -1, -1 };

        int PickBest(int burstLevel)
        {
            foreach (var candidate in sortedCandidates)
            {
                if (usedIds.Contains(candidate.User.id)) continue;
                if (burstLevel == 0 || candidate.Game.burstLevel == burstLevel)
                {
                    usedIds.Add(candidate.User.id);
                    return candidate.User.id;
                }
            }
            return -1;
        }

        newSlots[0] = PickBest(1);
        newSlots[1] = PickBest(2);
        newSlots[2] = PickBest(3);
        newSlots[3] = PickBest(3);
        newSlots[4] = PickBest(0);

        for (int i = 0; i < 5; i++)
        {
            if (newSlots[i] == -1) newSlots[i] = PickBest(0);
        }

        int squadId = CurrentSquadIndex.Value + 1;
        _tempSquads[squadId].slot = newSlots;

        RefreshSlots();
        ScrollViewModel.RefreshSelection(); // 스크롤뷰 갱신
        Debug.Log("[SquadDetail] 자동 편성 완료");
    }

    public void OnClickReset()
    {
        int squadId = CurrentSquadIndex.Value + 1;
        var slots = _tempSquads[squadId].slot;
        for (int i = 0; i < 5; i++) slots[i] = -1;

        RefreshSlots();
        ScrollViewModel.RefreshSelection();
    }

    public void OnClickSave()
    {
        // 1. 임시 데이터를 실제 UserData에 반영 (Overwrite)
        var realSquads = Managers.Data.UserData.Squads;

        foreach (var kvp in _tempSquads)
        {
            int id = kvp.Key;
            UserSquadData tempData = kvp.Value;

            if (realSquads.TryGetValue(id, out var userSquad))
            {
                // 리스트 내용 복사
                userSquad.slot = new List<int>(tempData.slot);
            }
            else
            {
                // 없으면 새로 생성하여 추가
                realSquads.Add(id, tempData.Clone());
            }
        }

        // 2. 로컬 저장
        Managers.Data.SaveUserData();

        Debug.Log("[SquadDetail] 스쿼드 변경사항 저장 완료.");
        OnCloseRequested?.Invoke();
    }

    public void OnClickClose()
    {
        // 저장하지 않고 닫음 (임시 데이터 파기됨)
        OnCloseRequested?.Invoke();
    }

    protected override void OnDispose()
    {
        if (SlotViewModels != null)
        {
            foreach (var vm in SlotViewModels) vm.Release();
            SlotViewModels = null;
        }

        if (ScrollViewModel != null)
        {
            ScrollViewModel.OnSquadChanged -= OnScrollSelectionChanged;
            ScrollViewModel.Release();
            ScrollViewModel = null;
        }

        OnCloseRequested = null;
        OnSquadDataChanged = null;
    }
}
