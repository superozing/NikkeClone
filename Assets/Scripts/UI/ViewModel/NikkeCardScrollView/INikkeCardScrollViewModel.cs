using System;
using System.Collections.Generic;

public interface INikkeCardScrollViewModel
{
    // 리스트 갱신 알림
    event Action OnListUpdated;

    // 뷰에게 id값 전달 (클릭 시 동작 위임)
    event Action<int> OnNikkeClickCallback;

    // 뷰에게 정렬 필터 UI 활성/비활성 요청 전달
    event Action<bool> OnControlSortFilterView;

    // --- Filter & Sort Status ---
    ReactiveProperty<bool> IsSearchActive { get; }

    // --- Sort State ---
    ReactiveProperty<eNikkeSortType> SortType { get; }
    ReactiveProperty<bool> IsSortAscending { get; }

    // --- Filter State ---
    ReactiveProperty<bool>[] ClassFilters { get; }
    ReactiveProperty<bool>[] CodeFilters { get; }
    ReactiveProperty<bool>[] WeaponFilters { get; }
    ReactiveProperty<bool>[] ManufacturerFilters { get; }
    ReactiveProperty<bool>[] BurstFilters { get; }

    // View가 표시할 최종 리스트
    List<NikkeCardViewModel> DisplayNikkes { get; }

    // 전체 니케 수
    int TotalNikkeCount { get; }

    // --- Interaction Methods ---
    void OnClickSearch();
    void RequestOpenSortFilter();
    void RequestCloseSortFilter();
    void SetSortType(eNikkeSortType type);
    void ToggleSortOrder();
    void OnClickBurst(int burstLevel);

    void ToggleClassFilter(eNikkeClass type);
    void ToggleCodeFilter(eNikkeCode type);
    void ToggleWeaponFilter(eNikkeWeapon type);
    void ToggleManufacturerFilter(eNikkeManufacturer type);

    /// <summary>
    /// 탭 이탈 시 호출: 모든 필터 초기화 및 팝업 닫기 요청
    /// </summary>
    void ResetFiltersAndPopup();
}