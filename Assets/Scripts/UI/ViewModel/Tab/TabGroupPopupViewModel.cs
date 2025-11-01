using System;
using UI;

public class TabGroupPopupViewModel : IViewModel
{
    public event Action OnStateChanged;
    public eTabType CurrentTabType { get; private set; } = eTabType.Lobby;

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
}