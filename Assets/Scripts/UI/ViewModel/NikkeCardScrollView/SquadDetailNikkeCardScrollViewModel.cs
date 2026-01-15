using System;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;

public class SquadDetailNikkeCardScrollViewModel : NikkeCardScrollViewModelBase
{
    // 편집 중인 임시 스쿼드 데이터 참조
    private UserSquadData _tempSquadData;

    public event Action OnSquadChanged;

    public SquadDetailNikkeCardScrollViewModel() : base()
    {
    }

    /// <summary>
    /// 편집할 스쿼드 데이터를 설정합니다.
    /// 스쿼드가 변경되었으므로 리스트를 재정렬합니다.
    /// </summary>
    /// <param name="squadData">복제된 임시 데이터여야 합니다.</param>
    public void SetSquadData(UserSquadData squadData)
    {
        _tempSquadData = squadData;
        // 스쿼드 교체 시에는 정렬 수행
        UpdateSelectionState(sort: true);
    }

    /// <summary>
    /// 현재 스쿼드 데이터에 맞춰 카드의 선택 상태를 갱신합니다.
    /// </summary>
    /// <param name="sort">true일 경우 리스트 재정렬을 수행합니다.</param>
    public void UpdateSelectionState(bool sort)
    {
        if (_tempSquadData == null) return;

        // 1. 선택된 ID 집합 생성
        HashSet<int> selectedIds = new HashSet<int>();
        foreach (var id in _tempSquadData.slot)
        {
            if (id != -1) selectedIds.Add(id);
        }

        // 2. 모든 카드 뷰모델의 IsSelected 갱신
        // IsSelected는 ReactiveProperty이므로 값 변경 시 즉시 View에 반영됨
        foreach (var vm in _allNikkes)
        {
            vm.IsSelected.Value = selectedIds.Contains(vm.NikkeId);
        }

        // 3. 옵션에 따라 리스트 재정렬 (선택된 항목 위로)
        if (sort)
        {
            RefreshList();
        }
    }

    protected override void OnCardClick(int nikkeId)
    {
        if (_tempSquadData == null) return;

        var slots = _tempSquadData.slot;
        int existingIndex = slots.IndexOf(nikkeId);

        if (existingIndex != -1)
        {
            // 이미 스쿼드에 존재 -> 제거
            slots[existingIndex] = -1;
        }
        else
        {
            // 스쿼드에 없음 -> 빈 자리 찾아서 추가
            int emptyIndex = slots.IndexOf(-1);
            if (emptyIndex != -1)
            {
                slots[emptyIndex] = nikkeId;
            }
            else
            {
                Debug.Log("[SquadDetail] 빈 슬롯이 없습니다.");
                return; // 변경 사항 없음
            }
        }

        // 데이터 변경 후 선택 상태만 갱신하고 정렬은 하지 않음
        UpdateSelectionState(sort: false);

        // 상위(PopupViewModel)에게 데이터 변경 알림 (슬롯 UI 갱신 등을 위해)
        OnSquadChanged?.Invoke();
    }

    /// <summary>
    /// 정렬 로직 오버라이드: 선택된 니케를 최상단으로 올림
    /// </summary>
    protected override void RefreshList()
    {
        if (_isBatchUpdating) return;

        IEnumerable<NikkeCardViewModel> query = _allNikkes;

        // 필터링
        query = ApplyFilterGroup(query, BurstFilters, vm => vm.BurstType);
        query = ApplyFilterGroup(query, ClassFilters, vm => vm.ClassType);
        query = ApplyFilterGroup(query, CodeFilters, vm => vm.CodeType);
        query = ApplyFilterGroup(query, WeaponFilters, vm => vm.WeaponType);
        query = ApplyFilterGroup(query, ManufacturerFilters, vm => vm.ManufacturerType);

        // 정렬 (선택된 니케 우선)
        var orderedQuery = query.OrderByDescending(vm => vm.IsSelected.Value); // true(1)가 위로

        if (IsSortAscending.Value)
        {
            orderedQuery = SortType.Value switch
            {
                eNikkeSortType.Level => orderedQuery.ThenBy(vm => vm.CurrentLevel),
                _ => orderedQuery.ThenBy(vm => vm.CombatPower)
            };
        }
        else
        {
            orderedQuery = SortType.Value switch
            {
                eNikkeSortType.Level => orderedQuery.ThenByDescending(vm => vm.CurrentLevel),
                _ => orderedQuery.ThenByDescending(vm => vm.CombatPower)
            };
        }

        // 이름 2차 정렬
        orderedQuery = orderedQuery.ThenBy(vm => vm.NikkeName);

        DisplayNikkes = orderedQuery.ToList();

        NotifyListUpdated();
    }

    protected override void OnDispose()
    {
        OnSquadChanged = null;
        _tempSquadData = null;
        base.OnDispose();
    }
}
