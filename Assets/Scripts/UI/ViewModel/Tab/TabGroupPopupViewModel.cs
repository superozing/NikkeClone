using System;
using UI;

public class TabGroupPopupViewModel : IViewModel, IDisposable
{
    public event Action OnStateChanged;
    public eTabType CurrentTabType { get; private set; } = eTabType.Lobby;

    /// <summary>
    /// 탭 UI의 뷰모델 배열. eTabType enum 순서와 일치해야 합니다.
    /// </summary>
    public IViewModel[] TabViewModels { get; private set; }

    public TabGroupPopupViewModel()
    {
        // 자식 탭 UI의 뷰모델 생성
        TabViewModels = new IViewModel[(int)eTabType.End];
        TabViewModels[(int)eTabType.Lobby] = new LobbyTabViewModel();
        TabViewModels[(int)eTabType.Squad] = new SquadTabViewModel();
        TabViewModels[(int)eTabType.Nikke] = new NikkeTabViewModel();
        TabViewModels[(int)eTabType.Inventory] = new InventoryTabViewModel();
        TabViewModels[(int)eTabType.Recruit] = new RecruitTabViewModel();
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

    public void Dispose()
    {
        if (TabViewModels == null)
            return;

        // 자식 UI의 뷰모델을 정리합니다.
        foreach (IViewModel viewModel in TabViewModels)
            (viewModel as IDisposable)?.Dispose();
        
        TabViewModels = null;
    }
}