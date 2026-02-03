using System;

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

    public void HandleBackButtonClicked()
    {
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
