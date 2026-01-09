using System;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;

/// <summary>
/// 필터링, 정렬, 리스트 관리 등 공통 로직을 구현한 베이스 뷰모델입니다.
/// </summary>
public abstract class NikkeCardScrollViewModelBase : ViewModelBase, INikkeCardScrollViewModel
{
    public event Action OnListUpdated;
    public event Action<int> OnNikkeClickCallback;
    public event Action<bool> OnControlSortFilterView;

    // --- Filter & Sort Status ---
    public ReactiveProperty<bool> IsSearchActive { get; private set; } = new(false);

    // --- Sort State ---
    public ReactiveProperty<eNikkeSortType> SortType { get; private set; } = new(eNikkeSortType.CombatPower);
    public ReactiveProperty<bool> IsSortAscending { get; private set; } = new(false);

    // --- Filter State ---
    public ReactiveProperty<bool>[] ClassFilters { get; private set; } = new ReactiveProperty<bool>[(int)eNikkeClass.End];
    public ReactiveProperty<bool>[] CodeFilters { get; private set; } = new ReactiveProperty<bool>[(int)eNikkeCode.End];
    public ReactiveProperty<bool>[] WeaponFilters { get; private set; } = new ReactiveProperty<bool>[(int)eNikkeWeapon.End];
    public ReactiveProperty<bool>[] ManufacturerFilters { get; private set; } = new ReactiveProperty<bool>[(int)eNikkeManufacturer.End];
    public ReactiveProperty<bool>[] BurstFilters { get; private set; } = new ReactiveProperty<bool>[(int)eNikkeBurst.End];

    // --- Data ---
    protected readonly List<NikkeCardViewModel> _allNikkes = new();
    public List<NikkeCardViewModel> DisplayNikkes { get; protected set; } = new();

    public int TotalNikkeCount => _allNikkes.Count;

    protected bool _isBatchUpdating = false;

    public NikkeCardScrollViewModelBase()
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
        _allNikkes.Clear();

        var userData = Managers.Data.UserData.Nikkes;
        var gameData = Managers.Data.GetTable<NikkeGameData>();

        foreach (var userNikke in userData.Values)
        {
            if (gameData.TryGetValue(userNikke.id, out var data))
            {
                var vm = new NikkeCardViewModel(userNikke, data);
                vm.AddRef();
                vm.OnClick += OnCardClickInternal;
                _allNikkes.Add(vm);
            }
        }

        // 초기 리스트 구성
        RefreshList();
    }

    /// <summary>
    /// 카드 클릭 시 발생하는 내부 이벤트 핸들러입니다.
    /// 구체적인 동작은 자식 클래스에서 재정의할 수 있도록 가상 메서드 처리하거나
    /// 자식에서 OnNikkeClickCallback을 구독/호출하는 방식을 사용합니다.
    /// 여기서는 자식 클래스가 로직을 주입할 수 있도록 추상 메서드를 사용합니다.
    /// </summary>
    protected abstract void OnCardClick(int nikkeId);

    private void OnCardClickInternal(int nikkeId)
    {
        OnCardClick(nikkeId);
    }

    // --- Helper Methods ---

    protected void NotifyListUpdated() => OnListUpdated?.Invoke();
    protected void NotifyNikkeClick(int id) => OnNikkeClickCallback?.Invoke(id);
    protected void NotifyControlSortFilterView(bool isOpen) => OnControlSortFilterView?.Invoke(isOpen);

    private void FillFilterArray(ReactiveProperty<bool>[] filterArray)
    {
        for (int i = 0; i < filterArray.Length; i++)
        {
            filterArray[i] = new ReactiveProperty<bool>(false);
            filterArray[i].OnValueChanged += _ => RefreshList();
        }
    }

    // --- INikkeCardScrollViewModel Implementation ---

    public void OnClickSearch()
    {
        IsSearchActive.Value = !IsSearchActive.Value;
        RefreshList();
    }

    public void RequestOpenSortFilter() => OnControlSortFilterView?.Invoke(true);
    public void RequestCloseSortFilter() => OnControlSortFilterView?.Invoke(false);

    public void SetSortType(eNikkeSortType type)
    {
        if (SortType.Value != type)
            SortType.Value = type;
        RefreshList();
    }

    public void ToggleSortOrder()
    {
        IsSortAscending.Value = !IsSortAscending.Value;
        RefreshList();
    }

    public void OnClickBurst(int burstLevel)
    {
        int index = Mathf.Clamp(burstLevel, 1, 3);
        if (index < BurstFilters.Length)
            BurstFilters[index].Value = !BurstFilters[index].Value;
    }

    public void ToggleClassFilter(eNikkeClass type) => ToggleFilter(ClassFilters, type);
    public void ToggleCodeFilter(eNikkeCode type) => ToggleFilter(CodeFilters, type);
    public void ToggleWeaponFilter(eNikkeWeapon type) => ToggleFilter(WeaponFilters, type);
    public void ToggleManufacturerFilter(eNikkeManufacturer type) => ToggleFilter(ManufacturerFilters, type);

    protected void ToggleFilter<T>(ReactiveProperty<bool>[] filters, T type) where T : Enum
    {
        int index = Convert.ToInt32(type);
        if (index >= 0 && index < filters.Length)
            filters[index].Value = !filters[index].Value;
    }

    public void ResetFiltersAndPopup()
    {
        _isBatchUpdating = true;

        ResetFilterArray(ClassFilters);
        ResetFilterArray(CodeFilters);
        ResetFilterArray(WeaponFilters);
        ResetFilterArray(ManufacturerFilters);
        ResetFilterArray(BurstFilters);

        SortType.Value = eNikkeSortType.CombatPower;
        IsSortAscending.Value = false;

        if (IsSearchActive.Value) IsSearchActive.Value = false;

        OnControlSortFilterView?.Invoke(false);

        _isBatchUpdating = false;
        RefreshList();
    }

    private void ResetFilterArray(ReactiveProperty<bool>[] filters)
    {
        foreach (var filter in filters)
            if (filter.Value)
                filter.Value = false;
    }

    /// <summary>
    /// 필터 및 정렬 로직을 수행하고 DisplayNikkes를 갱신합니다.
    /// 자식 클래스에서 추가적인 정렬 로직(예: 선택된 니케 우선)이 필요할 경우 오버라이딩 할 수 있습니다.
    /// </summary>
    protected virtual void RefreshList()
    {
        if (_isBatchUpdating) return;

        IEnumerable<NikkeCardViewModel> query = _allNikkes;

        // 필터링
        query = ApplyFilterGroup(query, BurstFilters, vm => vm.BurstType);
        query = ApplyFilterGroup(query, ClassFilters, vm => vm.ClassType);
        query = ApplyFilterGroup(query, CodeFilters, vm => vm.CodeType);
        query = ApplyFilterGroup(query, WeaponFilters, vm => vm.WeaponType);
        query = ApplyFilterGroup(query, ManufacturerFilters, vm => vm.ManufacturerType);

        // 정렬
        if (IsSortAscending.Value)
        {
            query = SortType.Value switch
            {
                eNikkeSortType.Level => query.OrderBy(vm => vm.CurrentLevel),
                _ => query.OrderBy(vm => vm.CombatPower)
            };
        }
        else
        {
            query = SortType.Value switch
            {
                eNikkeSortType.Level => query.OrderByDescending(vm => vm.CurrentLevel),
                _ => query.OrderByDescending(vm => vm.CombatPower)
            };
        }

        // 이름 2차 정렬
        if (query is IOrderedEnumerable<NikkeCardViewModel> orderedQuery)
            query = orderedQuery.ThenBy(vm => vm.NikkeName);

        DisplayNikkes = query.ToList();

        OnListUpdated?.Invoke();
    }

    protected IEnumerable<NikkeCardViewModel> ApplyFilterGroup<T>(IEnumerable<NikkeCardViewModel> query, ReactiveProperty<bool>[] filters, Func<NikkeCardViewModel, T> selector) where T : Enum
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

    protected override void OnDispose()
    {
        foreach (var vm in _allNikkes)
        {
            vm.OnClick -= OnCardClickInternal;
            vm.Release();
        }
        _allNikkes.Clear();
        DisplayNikkes.Clear();

        OnListUpdated = null;
        OnNikkeClickCallback = null;
        OnControlSortFilterView = null;
    }
}