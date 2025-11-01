using System;
using UI;

public class LobbyTabViewModel : IViewModel, IDisposable
{
    public event Action OnStateChanged;

    public LobbyTabViewModel()
    {
        // 탭에 필요한 데이터 로드 및 ReactiveProperty 구독
    }

    public void Dispose()
    {
        // 구독한 ReactiveProperty 이벤트 해제
    }
}
