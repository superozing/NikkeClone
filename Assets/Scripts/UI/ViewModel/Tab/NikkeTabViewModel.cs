using System;
using UI;

public class NikkeTabViewModel : IViewModel, IDisposable
{
    public event Action OnStateChanged;

    public NikkeTabViewModel()
    {
        // 탭에 필요한 데이터 로드 및 ReactiveProperty 구독
    }

    public void Dispose()
    {
        // 구독한 ReactiveProperty 이벤트 해제
    }
}
