using System;
using System.Threading.Tasks;
using UI;

public class LobbyTabViewModel : ViewModelBase
{
    public event Action OnRequestUnusedButton; // 사용하지 않는 버튼

    public MissionButtonViewModel MissionButtonViewModel { get; private set; }

    public LobbyTabViewModel()
    {
        MissionButtonViewModel = new MissionButtonViewModel();
        MissionButtonViewModel.AddRef();
    }

    /// <summary>
    /// 사용하지 않는 버튼 클릭 이벤트
    /// </summary>
    public void OnUnusedButtonClicked()
    {
        OnRequestUnusedButton?.Invoke();
    }

    /// <summary>
    /// 캠페인 버튼 클릭 시 UI_LoadingPopup을 통해 CampaignScene으로 전환합니다.
    /// Caller: UI_LobbyTab.OnCampaignButtonClick() (Button.onClick -> 래퍼)
    /// </summary>
    public async void OnCampaignButtonClicked()
    {
        Func<Task> loadTask = async () =>
        {
            await Managers.Scene.LoadSceneAsync(eSceneType.CampaignScene);
        };

        var loadingVM = new LoadingPopupViewModel(loadTask);
        await Managers.UI.ShowDontDestroyAsync<UI_LoadingPopup>(loadingVM);
    }

    protected override void OnDispose()
    {
        if (MissionButtonViewModel != null)
        {
            MissionButtonViewModel.Release();
            MissionButtonViewModel = null;
        }

        OnRequestUnusedButton = null;
    }
}
