using System;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_MissionButton : UI_View
{
    [SerializeField] private Button _missionButton;
    [SerializeField] private TMP_Text _missionDescText;
    [SerializeField] private GameObject _redDot;

    private MissionButtonViewModel _viewModel;

    /// <summary>
    /// 이 View와 상호작용할 ViewModel을 설정(주입)하고 데이터 바인딩을 시작합니다.
    /// </summary>
    /// <param name="viewModel">주입할 ViewModel입니다. (반드시 MissionButtonViewModel이어야 함)</param>
    public override void SetViewModel(ViewModelBase viewModel)
    {
        if (_viewModel != null)
            _viewModel.OnRequestMissionPopup -= ShowMissionPopup;

        _viewModel = viewModel as MissionButtonViewModel;

        base.SetViewModel(viewModel);

        if (_viewModel == null && viewModel != null)
        {
            Debug.LogError($"[UI_MissionButton] 잘못된 ViewModel 타입이 주입되었습니다.");
            return;
        }

        if (_viewModel != null)
        {
            // ReactiveProperty 바인딩
            Bind(_viewModel.MissionDesc, text => _missionDescText.text = text);
            Bind(_viewModel.IsRedDotActive, active => { if (_redDot) _redDot.SetActive(active); });

            _viewModel.OnRequestMissionPopup += ShowMissionPopup;
        }

        _missionButton.onClick.RemoveAllListeners();
        _missionButton.onClick.AddListener(() => _viewModel?.OnMissionButtonClicked());
    }

    /// <summary>
    /// MissionPopup을 열어요.
    /// </summary>
    private async void ShowMissionPopup()
    {
        await Managers.UI.ShowAsync<UI_MissionPopup>(new MissionPopupViewModel());
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (_viewModel != null)
            _viewModel.OnRequestMissionPopup -= ShowMissionPopup;

        _missionButton.onClick.RemoveAllListeners();

        (_viewModel as IDisposable)?.Dispose();
        _viewModel = null;
    }
}
