using System;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;

public class SquadDetailNikkeCardScrollViewModel : NikkeCardScrollViewModelBase
{
    // 편집 중인 임시 스쿼드 데이터 참조
    private UserSquadData _tempSquadData;

    public SquadDetailNikkeCardScrollViewModel() : base()
    {
    }

    /// <summary>
    /// 편집할 스쿼드 데이터를 설정합니다.
    /// </summary>
    /// <param name="squadData">복제된 임시 데이터여야 합니다.</param>
    public void SetSquadData(UserSquadData squadData)
    {
        _tempSquadData = squadData;
        RefreshSelection();
    }

    /// <summary>
    /// 현재 스쿼드 데이터에 맞춰 카드의 선택 상태를 갱신하고 리스트를 재정렬합니다.
    /// </summary>
    public void RefreshSelection()
    {
        if (_tempSquadData == null) return;

        // 1. 선택된 ID 집합 생성
        HashSet<int> selectedIds = new HashSet<int>();
        foreach (var id in _tempSquadData.slot)
        {
            if (id != -1) selectedIds.Add(id);
        }

        // 2. 모든 카드 뷰모델의 IsSelected 갱신
        foreach (var vm in _allNikkes)
        {
            vm.IsSelected.Value = selectedIds.Contains(vm.NikkeId);
        }

        // 3. 리스트 재정렬 (선택된 항목 위로)
        RefreshList();
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

        // 데이터 변경 후 선택 상태 및 정렬 갱신
        RefreshSelection();

        // 상위(PopupViewModel)에게 데이터 변경 알림 (슬롯 UI 갱신 등을 위해)
        // 여기서는 직접 이벤트를 발생시키기보다, 베이스의 공통 이벤트를 활용하거나
        // PopupViewModel이 이 VM의 상태를 감지하도록 해야 함.
        // 설계상 PopupViewModel이 SquadDataChanged 이벤트를 가지고 있으므로,
        // PopupViewModel에서 ScrollViewModel의 동작 후 갱신을 어떻게 처리할지 연결 고리가 필요함.
        // -> 간단하게 콜백을 하나 둡니다.
        OnSquadChanged?.Invoke();
    }

    public event Action OnSquadChanged;

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
