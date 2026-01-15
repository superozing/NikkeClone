using System;
using UI;

public class PopupTestViewModel : ViewModelBase
{
    public event Action OnEscapeKeyDown;

    public ReactiveProperty<string> Title { get; private set; } = new("테스트 팝업");
    public int ClickCount = 0;

    public void OnEscape()
    {
        OnEscapeKeyDown?.Invoke();
    }

    public void OnConfirm()
    {
        ClickCount++;
        Title.Value = $"확인 버튼이 {ClickCount}번 클릭되었습니다.";

        if (ClickCount >= 10)
        {
            OnEscape();
            return;
        }
    }
}