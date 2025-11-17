using System;
using UI;

public class LobbyTabViewModel : IViewModel, IDisposable
{
    public event Action OnStateChanged;
    public event Action OnRequestUnusedButton; // 사용하지 않는 버튼
    public event Action OnRequestCampaignButton; // 캠페인 버튼

    public MissionButtonViewModel MissionButtonViewModel { get; private set; }

    public LobbyTabViewModel()
    {
        MissionButtonViewModel = new MissionButtonViewModel();
    }

    /// <summary>
    /// 사용하지 않는 버튼 클릭 이벤트
    /// </summary>
    public void OnUnusedButtonClicked()
    {
        OnRequestUnusedButton?.Invoke();
    }

    /// <summary>
    /// 캠페인 버튼 클릭 이벤트
    /// </summary>
    public void OnCampaignButtonClicked()
    {
        OnRequestCampaignButton?.Invoke();
    }

    public void Dispose()
    {
        (MissionButtonViewModel as IDisposable)?.Dispose();
        MissionButtonViewModel = null;

        OnRequestUnusedButton = null;
        OnRequestCampaignButton = null;
    }
}
