using System;
using UI;

public class TabGroupPopupViewModel : ViewModelBase
{
    public override event Action OnStateChanged;
    public eTabType CurrentTabType { get; private set; } = eTabType.Lobby;

    /// <summary>
    /// 탭 UI의 뷰모델 배열. eTabType enum 순서와 일치해야 합니다.
    /// </summary>
    public IViewModel[] TabViewModels { get; private set; }

    /// <summary>
    /// 상단 재화 UI의 뷰모델입니다.
    /// </summary>
    public MoneyViewModel MoneyViewModel { get; private set; }

    public TabGroupPopupViewModel()
    {
        // 1. 자식 탭 UI의 뷰모델 생성
        TabViewModels = new IViewModel[(int)eTabType.End];
        TabViewModels[(int)eTabType.Lobby] = new LobbyTabViewModel();
        TabViewModels[(int)eTabType.Squad] = new SquadTabViewModel();
        TabViewModels[(int)eTabType.Nikke] = new NikkeTabViewModel();
        TabViewModels[(int)eTabType.Inventory] = new InventoryTabViewModel();
        TabViewModels[(int)eTabType.Recruit] = new RecruitTabViewModel();

        foreach (var vm in TabViewModels)
            (vm as ViewModelBase)?.AddRef();

        // 2. 재화 UI 뷰모델 생성
        MoneyViewModel = new MoneyViewModel();
        MoneyViewModel.AddRef();
    }

    /// <summary>
    /// View(UI_TabGroupPopup)에서 탭 버튼 클릭 시 호출됩니다.
    /// </summary>
    /// <param name="tabType">새로 선택된 탭의 타입입니다.</param>
    public void OnTabButtonClicked(eTabType tabType)
    {
        // 같은 탭일 경우 예외처리
        if (CurrentTabType == tabType)
            return;

        CurrentTabType = tabType;

        // 뷰에 상태갱신
        OnStateChanged?.Invoke();
    }

    protected override void OnDispose()
    {
        // 자식 탭 뷰모델 정리
        if (TabViewModels != null)
        {
            foreach (var vm in TabViewModels)
                (vm as ViewModelBase)?.Release();
            TabViewModels = null;
        }

        // 재화 뷰모델 정리
        if (MoneyViewModel != null)
        {
            MoneyViewModel.Release();
            MoneyViewModel = null;
        }

        OnStateChanged = null;
    }
}