using System;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;

public class NikkeCardScrollViewModel : ViewModelBase
{
    // 리스트 갱신 알림
    public event Action OnListUpdated;

    // 뷰에게 id값 전달
    public event Action<int> OnNikkeClickCallback;
    // 뷰에게 정렬 필터 UI 활성/비활성 요청 전달
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
    private readonly List<NikkeCardViewModel> _allNikkes = new();

    // View가 표시할 최종 리스트
    public List<NikkeCardViewModel> DisplayNikkes { get; private set; } = new();

    // 전체 니케 수 (초기 생성용)
    public int TotalNikkeCount => _allNikkes.Count;

    // 업데이트 증인지 확인 위한 플래그
    private bool _isBatchUpdating = false;

    public NikkeCardScrollViewModel()
    {
        FillFilterArray(ClassFilters);
        FillFilterArray(CodeFilters);
        FillFilterArray(WeaponFilters);
        FillFilterArray(ManufacturerFilters);
        FillFilterArray(BurstFilters);

        // 모든 니케에 대한 뷰모델 생성
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

        // 초기 리스트 구성
        RefreshList();
    }

    /// <summary>
    /// 배열 내부의 ReactiveProperty를 초기화하고 이벤트를 연결하는 유틸 메서드입니다.
    /// </summary>
    private void FillFilterArray(ReactiveProperty<bool>[] filterArray)
    {
        for (int i = 0; i < filterArray.Length; i++)
        {
            filterArray[i] = new ReactiveProperty<bool>(false);
            filterArray[i].OnValueChanged += _ => RefreshList();
        }
    }

    /// <summary>
    /// 탭 이탈 시 호출: 모든 필터 초기화 및 팝업 닫기 요청
    /// </summary>
    public void ResetFiltersAndPopup()
    {
        _isBatchUpdating = true; // 리스트 갱신 일시 중지

        // 1. 모든 필터 배열 false로 초기화
        ResetFilterArray(ClassFilters);
        ResetFilterArray(CodeFilters);
        ResetFilterArray(WeaponFilters);
        ResetFilterArray(ManufacturerFilters);
        ResetFilterArray(BurstFilters);

        SortType.Value = eNikkeSortType.CombatPower;
        IsSortAscending.Value = false;

        // 2. 검색 상태 초기화
        if (IsSearchActive.Value) IsSearchActive.Value = false;

        // 3. 팝업 닫기 요청 (View에게 알림)
        OnControlSortFilterView?.Invoke(false);

        _isBatchUpdating = false; // 중지 해제

        // 4. 최종적으로 한 번만 리스트 갱신
        RefreshList();
    }

    private void ResetFilterArray(ReactiveProperty<bool>[] filters)
    {
        foreach (var filter in filters)
            if (filter.Value) 
                filter.Value = false;
    }

    /// <summary>
    /// 필터/정렬 상태를 기반으로 리스트를 갱신합니다.
    /// </summary>
    private void RefreshList()
    {
        // 현재 리스트 초기화 중일 경우 예외처리
        if (_isBatchUpdating) 
            return;

        IEnumerable<NikkeCardViewModel> query = _allNikkes;

        // 필터링
        query = ApplyFilterGroup(query, BurstFilters, vm => vm.BurstType);
        query = ApplyFilterGroup(query, ClassFilters, vm => vm.ClassType);
        query = ApplyFilterGroup(query, CodeFilters, vm => vm.CodeType);
        query = ApplyFilterGroup(query, WeaponFilters, vm => vm.WeaponType);
        query = ApplyFilterGroup(query, ManufacturerFilters, vm => vm.ManufacturerType);

        // 정렬 기준에 따라 정렬 적용
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

        // 사전 순 2차 정렬
        if (query is IOrderedEnumerable<NikkeCardViewModel> orderedQuery)
            query = orderedQuery.ThenBy(vm => vm.NikkeName);

        DisplayNikkes = query.ToList();

        OnListUpdated?.Invoke();
    }

    /// <summary>
    /// 특정 필터 그룹을 적용하는 헬퍼 메서드입니다.
    /// </summary>
    private IEnumerable<NikkeCardViewModel> ApplyFilterGroup<T>(IEnumerable<NikkeCardViewModel> query, ReactiveProperty<bool>[] filters, Func<NikkeCardViewModel, T> selector) where T : Enum
    {
        HashSet<int> activeIdx = new();
        for (int i = 0; i < filters.Length; ++i)
        {
            if (filters[i] != null && filters[i].Value)
                activeIdx.Add(i);
        }

        // 활성화된 필터가 하나라도 있으면, 그 중 하나라도 일치하는 항목만 통과 (OR 로직)
        if (activeIdx.Count > 0)
            return query.Where(vm => activeIdx.Contains(Convert.ToInt32(selector(vm))));

        // 활성화된 필터가 없으면 모두 통과
        return query;
    }

    // --- Interaction Methods (View -> ViewModel) ---

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
        // 인덱스 안전성 체크 후 직접 토글
        int index = Mathf.Clamp(burstLevel, 1, 3);
        if (index < BurstFilters.Length)
        {
            BurstFilters[index].Value = !BurstFilters[index].Value;
        }
    }

    private void OnCardClick(int nikkeId)
    {
        OnNikkeClickCallback?.Invoke(nikkeId);
    }

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
        OnControlSortFilterView = null;
    }
}