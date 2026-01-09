using System;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;

public class SquadDetailNikkeCardScrollViewModel : ViewModelBase
{
    // 리스트 갱신 알림
    public event Action OnListUpdated;

    // 뷰에게 id값 전달 (클릭 시 편성/해제 요청)
    public event Action<int> OnNikkeClickCallback;

    // --- Filter & Sort Status ---
    public ReactiveProperty<bool> IsSearchActive { get; private set; } = new(false);

    // --- Sort State ---
    // 기본적으로 전투력 순, 필요 시 변경 가능하도록 유지
    public ReactiveProperty<eNikkeSortType> SortType { get; private set; } = new(eNikkeSortType.CombatPower);
    public ReactiveProperty<bool> IsSortAscending { get; private set; } = new(false);

    // --- Filter State (기본 스크롤뷰와 동일한 필터링 지원) ---
    public ReactiveProperty<bool>[] ClassFilters { get; private set; } = new ReactiveProperty<bool>[(int)eNikkeClass.End];
    public ReactiveProperty<bool>[] CodeFilters { get; private set; } = new ReactiveProperty<bool>[(int)eNikkeCode.End];
    public ReactiveProperty<bool>[] WeaponFilters { get; private set; } = new ReactiveProperty<bool>[(int)eNikkeWeapon.End];
    public ReactiveProperty<bool>[] ManufacturerFilters { get; private set; } = new ReactiveProperty<bool>[(int)eNikkeManufacturer.End];
    public ReactiveProperty<bool>[] BurstFilters { get; private set; } = new ReactiveProperty<bool>[(int)eNikkeBurst.End];

    // --- Data ---
    private readonly List<NikkeCardViewModel> _allNikkes = new();

    // View가 표시할 최종 리스트
    public List<NikkeCardViewModel> DisplayNikkes { get; private set; } = new();

    // 현재 선택된(편성된) 니케 ID 목록 (외부에서 주입)
    private HashSet<int> _selectedNikkeIds = new HashSet<int>();

    private bool _isBatchUpdating = false;

    public SquadDetailNikkeCardScrollViewModel()
    {
        FillFilterArray(ClassFilters);
        FillFilterArray(CodeFilters);
        FillFilterArray(WeaponFilters);
        FillFilterArray(ManufacturerFilters);
        FillFilterArray(BurstFilters);

        LoadAllNikkes();
    }

    private void LoadAllNikkes()
    {
        var userData = Managers.Data.UserData.Nikkes;
        var gameData = Managers.Data.GetTable<NikkeGameData>();

        _allNikkes.Clear();

        foreach (var userNikke in userData.Values)
        {
            if (gameData.TryGetValue(userNikke.id, out var data))
            {
                var vm = new NikkeCardViewModel(userNikke, data);
                vm.AddRef();
                vm.OnClick += OnCardClick;
                _allNikkes.Add(vm);
            }
        }
    }

    /// <summary>
    /// 현재 편성된 니케 목록을 갱신하고 리스트를 다시 정렬합니다.
    /// </summary>
    public void SetSelectedNikkes(List<int> selectedIds)
    {
        _selectedNikkeIds.Clear();
        if (selectedIds != null)
        {
            foreach (var id in selectedIds)
            {
                if (id != -1)
                    _selectedNikkeIds.Add(id);
            }
        }

        RefreshList();
    }

    private void RefreshList()
    {
        if (_isBatchUpdating) return;

        IEnumerable<NikkeCardViewModel> query = _allNikkes;

        // 1. 필터링 적용
        query = ApplyFilterGroup(query, BurstFilters, vm => vm.BurstType);
        query = ApplyFilterGroup(query, ClassFilters, vm => vm.ClassType);
        query = ApplyFilterGroup(query, CodeFilters, vm => vm.CodeType);
        query = ApplyFilterGroup(query, WeaponFilters, vm => vm.WeaponType);
        query = ApplyFilterGroup(query, ManufacturerFilters, vm => vm.ManufacturerType);

        // 2. 선택 상태(IsSelected) 갱신
        // ViewModel의 속성을 변경하여 View가 즉시 반영하도록 함
        foreach (var vm in _allNikkes)
        {
            bool isSelected = _selectedNikkeIds.Contains(vm.NikkeId);
            if (vm.IsSelected.Value != isSelected)
                vm.IsSelected.Value = isSelected;
        }

        // 3. 정렬 로직
        // 1순위: 현재 선택된 니케 (무조건 상단)
        // 2순위: 지정된 정렬 기준 (전투력 등)

        // OrderBy는 Stable Sort이므로, 2차 정렬부터 적용 후 1차 정렬을 적용하거나
        // OrderBy(...).ThenBy(...) 체인을 사용해야 함.
        // 여기서는 ThenBy 방식 사용.

        // 기본 정렬 (전투력/레벨)
        IOrderedEnumerable<NikkeCardViewModel> orderedQuery;

        if (IsSortAscending.Value)
        {
            orderedQuery = SortType.Value switch
            {
                eNikkeSortType.Level => query.OrderBy(vm => vm.CurrentLevel),
                _ => query.OrderBy(vm => vm.CombatPower)
            };
        }
        else
        {
            orderedQuery = SortType.Value switch
            {
                eNikkeSortType.Level => query.OrderByDescending(vm => vm.CurrentLevel),
                _ => query.OrderByDescending(vm => vm.CombatPower)
            };
        }

        // 선택된 니케 우선 정렬 (true가 1, false가 0이므로 Descending하면 true가 먼저 옴)
        orderedQuery = orderedQuery.OrderByDescending(vm => vm.IsSelected.Value ? 1 : 0)
                                   .ThenByDescending(vm => IsSortAscending.Value ?
                                        (SortType.Value == eNikkeSortType.Level ? vm.CurrentLevel : vm.CombatPower) * -1 : // 오름차순일 때 원래 정렬 유지용 trick 
                                        (SortType.Value == eNikkeSortType.Level ? vm.CurrentLevel : vm.CombatPower));

        // Linq OrderBy는 이미 정렬된 시퀀스를 다시 정렬하면 순서가 섞일 수 있으므로
        // 명시적으로: [선택여부 Desc] -> [정렬기준 Asc/Desc] -> [이름 Asc] 순으로 적용

        var finalQuery = query.OrderByDescending(vm => _selectedNikkeIds.Contains(vm.NikkeId)); // 1. 선택된 것 위로

        if (IsSortAscending.Value)
        {
            finalQuery = SortType.Value switch
            {
                eNikkeSortType.Level => finalQuery.ThenBy(vm => vm.CurrentLevel),
                _ => finalQuery.ThenBy(vm => vm.CombatPower)
            };
        }
        else
        {
            finalQuery = SortType.Value switch
            {
                eNikkeSortType.Level => finalQuery.ThenByDescending(vm => vm.CurrentLevel),
                _ => finalQuery.ThenByDescending(vm => vm.CombatPower)
            };
        }

        // 마지막 이름 정렬
        finalQuery = finalQuery.ThenBy(vm => vm.NikkeName);

        DisplayNikkes = finalQuery.ToList();

        OnListUpdated?.Invoke();
    }

    private IEnumerable<NikkeCardViewModel> ApplyFilterGroup<T>(IEnumerable<NikkeCardViewModel> query, ReactiveProperty<bool>[] filters, Func<NikkeCardViewModel, T> selector) where T : Enum
    {
        HashSet<int> activeIdx = new();
        for (int i = 0; i < filters.Length; ++i)
        {
            if (filters[i] != null && filters[i].Value)
                activeIdx.Add(i);
        }

        if (activeIdx.Count > 0)
            return query.Where(vm => activeIdx.Contains(Convert.ToInt32(selector(vm))));

        return query;
    }

    private void FillFilterArray(ReactiveProperty<bool>[] filterArray)
    {
        for (int i = 0; i < filterArray.Length; i++)
        {
            filterArray[i] = new ReactiveProperty<bool>(false);
            filterArray[i].OnValueChanged += _ => RefreshList();
        }
    }

    private void OnCardClick(int nikkeId)
    {
        OnNikkeClickCallback?.Invoke(nikkeId);
    }

    // --- Interaction Methods ---
    // (기존 스크롤 뷰모델과 동일한 필터 토글 메서드들)
    public void ToggleClassFilter(eNikkeClass type) => ToggleFilter(ClassFilters, type);
    public void ToggleCodeFilter(eNikkeCode type) => ToggleFilter(CodeFilters, type);
    public void ToggleWeaponFilter(eNikkeWeapon type) => ToggleFilter(WeaponFilters, type);
    public void ToggleManufacturerFilter(eNikkeManufacturer type) => ToggleFilter(ManufacturerFilters, type);

    private void ToggleFilter<T>(ReactiveProperty<bool>[] filters, T type) where T : Enum
    {
        int index = Convert.ToInt32(type);
        if (index >= 0 && index < filters.Length)
            filters[index].Value = !filters[index].Value;
    }

    public void OnClickSearch() { IsSearchActive.Value = !IsSearchActive.Value; RefreshList(); }
    public void SetSortType(eNikkeSortType type) { if (SortType.Value != type) { SortType.Value = type; RefreshList(); } }
    public void ToggleSortOrder() { IsSortAscending.Value = !IsSortAscending.Value; RefreshList(); }
    public void OnClickBurst(int burstLevel)
    {
        int index = Mathf.Clamp(burstLevel, 1, 3);
        if (index < BurstFilters.Length) BurstFilters[index].Value = !BurstFilters[index].Value;
    }

    public void RequestCloseSortFilter() { /* 팝업 닫기 이벤트 등 필요 시 구현 */ }

    protected override void OnDispose()
    {
        foreach (var vm in _allNikkes)
        {
            vm.OnClick -= OnCardClick;
            vm.Release();
        }
        _allNikkes.Clear();
        DisplayNikkes.Clear();

        OnListUpdated = null;
        OnNikkeClickCallback = null;
    }
}