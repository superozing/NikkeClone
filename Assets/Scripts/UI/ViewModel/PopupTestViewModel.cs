using System;
using UI;

public class PopupTestViewModel : IViewModel
{
    public event Action OnStateChanged;

    public string Title { get; private set; } = "테스트 팝업";
    private int _clickCount = 0;

    public void OnConfirm()
    {
        _clickCount++;
        Title = $"확인 버튼이 {_clickCount}번 클릭되었습니다.";
        OnStateChanged?.Invoke();
    }
}