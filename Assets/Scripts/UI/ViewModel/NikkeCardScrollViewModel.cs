using System;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;

public class NikkeCardScrollViewModel : ViewModelBase
{
    // 리스트 갱신 알림
    public event Action OnListUpdated;

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
    /// (현재는 로직이 들어갈 자리만 마련해 두었습니다)
    /// </summary>
    private void RefreshList()
    {
        // TODO: 실제 필터링/정렬 로직 구현 (다음 단계)

        // 현재는 전체 리스트를 그대로 표시
        DisplayNikkes = new List<NikkeCardViewModel>(_allNikkes);

        OnListUpdated?.Invoke();
    }

    // --- Interaction Methods (View -> ViewModel) ---

    public void OnClickSearch()
    {
        IsSearchActive.Value = !IsSearchActive.Value;
        RefreshList();
    }

    public void OnClickSort()
    {
        IsSortActive.Value = !IsSortActive.Value;
        RefreshList();
    }

    public void OnClickBurst(int level)
    {
        if (level == 1) IsBurst1Active.Value = !IsBurst1Active.Value;
        else if (level == 2) IsBurst2Active.Value = !IsBurst2Active.Value;
        else if (level == 3) IsBurst3Active.Value = !IsBurst3Active.Value;

        RefreshList();
    }

    private void OnCardClick(int nikkeId)
    {
        Debug.Log($"[NikkeCardScrollViewModel] 니케 클릭됨: ID {nikkeId}");
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
    }
}