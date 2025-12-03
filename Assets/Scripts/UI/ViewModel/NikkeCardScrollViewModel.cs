using System;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;

public class NikkeCardScrollViewModel : ViewModelBase
{
    // 리스트 갱신 알림
    public event Action OnListUpdated;

    // 뷰에게 클릭 시 id값 전달
    public event Action<int> OnNikkeClickCallback;

    // --- Filter & Sort Status ---
    public ReactiveProperty<bool> IsSearchActive { get; private set; } = new(false);
    public ReactiveProperty<bool> IsSortActive { get; private set; } = new(false); // 정렬 상세 버튼 활성화 여부

    public ReactiveProperty<bool> IsBurst1Active { get; private set; } = new(false);
    public ReactiveProperty<bool> IsBurst2Active { get; private set; } = new(false);
    public ReactiveProperty<bool> IsBurst3Active { get; private set; } = new(false);

    // --- Data ---
    private readonly List<NikkeCardViewModel> _allNikkes = new();

    // View가 표시할 최종 리스트
    public List<NikkeCardViewModel> DisplayNikkes { get; private set; } = new();

    // 전체 니케 수 (초기 생성용)
    public int TotalNikkeCount => _allNikkes.Count;

    public NikkeCardScrollViewModel()
    {
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
    /// 필터/정렬 상태를 기반으로 리스트를 갱신합니다.
    /// </summary>
    private void RefreshList()
    {
        // 필터링: Burst 1, 2, 3 (OR 조건)
        // 아무 필터도 켜져있지 않으면 전체 표시
        bool isAnyBurstFilterOn = IsBurst1Active.Value || IsBurst2Active.Value || IsBurst3Active.Value;

        IEnumerable<NikkeCardViewModel> query = _allNikkes;

        if (isAnyBurstFilterOn)
        {
            query = query.Where(vm =>
                (IsBurst1Active.Value && vm.BurstLevel == 1) ||
                (IsBurst2Active.Value && vm.BurstLevel == 2) ||
                (IsBurst3Active.Value && vm.BurstLevel == 3)
            );
        }

        // 정렬: 기본 전투력 내림차순
        query = query.OrderByDescending(vm => vm.CombatPower).ThenBy(vm => vm.NikkeName);

        DisplayNikkes = query.ToList();

        OnListUpdated?.Invoke();
    }

    // --- Interaction Methods (View -> ViewModel) ---

    public void OnClickSearch()
    {
        IsSearchActive.Value = !IsSearchActive.Value;
        // 구현해야 해요.
        RefreshList();
    }

    public void OnClickSort()
    {
        IsSortActive.Value = !IsSortActive.Value;
        // 구현해야 해요.
        RefreshList();
    }

    public void OnClickBurst(int burstLevel)
    {
        if (burstLevel == 1) IsBurst1Active.Value = !IsBurst1Active.Value;
        else if (burstLevel == 2) IsBurst2Active.Value = !IsBurst2Active.Value;
        else if (burstLevel == 3) IsBurst3Active.Value = !IsBurst3Active.Value;

        RefreshList();
    }

    private void OnCardClick(int nikkeId)
    {
        OnNikkeClickCallback.Invoke(nikkeId);
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
    }
}