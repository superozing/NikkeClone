using System;
using System.Threading.Tasks;

public class CampaignHUDViewModel : ViewModelBase
{
    // === Child ViewModels ===
    public MoneyViewModel MoneyViewModel { get; private set; }
    public MissionButtonViewModel MissionButtonViewModel { get; private set; }

    // === Events ===
    public event Action OnBackButtonClicked;
    public event Action OnSettingsButtonClicked;

    public CampaignHUDViewModel()
    {
        // 자식 뷰모델 생성
        MoneyViewModel = new MoneyViewModel();
        MissionButtonViewModel = new MissionButtonViewModel();

        MoneyViewModel.AddRef();
        MissionButtonViewModel.AddRef();
    }

    /// <summary>
    /// 뒤로가기 버튼 클릭 시 UI_LoadingPopup을 통해 MainScene으로 전환합니다.
    /// </summary>
    public async void HandleBackButtonClicked()
    {
        Func<Task> loadTask = async () =>
        {
            await Managers.Scene.LoadSceneAsync(eSceneType.MainScene);
        };

        var loadingVM = new LoadingPopupViewModel(loadTask);
        await Managers.UI.ShowDontDestroyAsync<UI_LoadingPopup>(loadingVM);

        OnBackButtonClicked?.Invoke();
    }

    public void HandleSettingsButtonClicked()
    {
        OnSettingsButtonClicked?.Invoke();
    }

    protected override void OnDispose()
    {
        base.OnDispose();

        // 자식 뷰모델 해제
        MoneyViewModel.Release();
        MissionButtonViewModel.Release();

        MoneyViewModel = null;
        MissionButtonViewModel = null;
        OnBackButtonClicked = null;
        OnSettingsButtonClicked = null;
    }
}
