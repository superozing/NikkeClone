using System;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_LobbyTab : UI_TabBase
{
    public override eTabType TabType => eTabType.Lobby;

    [Header("Buttons")]
    [SerializeField] private Button _shopButton;
    [SerializeField] private Button _outpostButton;
    [SerializeField] private Button _arkButton;
    [SerializeField] private Button _campaignButton;

    [SerializeField] private Button _friendButton;
    [SerializeField] private Button _unionButton;
    [SerializeField] private Button _commanderButton;

    [Header("Mission")]
    [SerializeField] private UI_MissionButton _missionButton;

    private LobbyTabViewModel _viewModel;

    protected override void Awake()
    {
        base.Awake();

        _campaignButton.onClick.AddListener(OnCampaignButtonClick);

        _shopButton.onClick.AddListener(OnUnusedButtonClick);
        _outpostButton.onClick.AddListener(OnUnusedButtonClick);
        _arkButton.onClick.AddListener(OnUnusedButtonClick);
        _friendButton.onClick.AddListener(OnUnusedButtonClick);
        _unionButton.onClick.AddListener(OnUnusedButtonClick);
        _commanderButton.onClick.AddListener(OnUnusedButtonClick);
    }

    public override void SetViewModel(ViewModelBase viewModel)
    {
        if (_viewModel != null)
        {
            _viewModel.OnRequestUnusedButton -= ShowUnusedLog;
        }

        _viewModel = viewModel as LobbyTabViewModel;

        if (_viewModel == null && viewModel != null)
        {
            Debug.LogError("[UI_LobbyTab] 잘못된 ViewModel 타입이 주입되었습니다.");
            return;
        }

        base.SetViewModel(viewModel); // 부모 호출 (기존 바인딩 해제 등)

        if (_viewModel != null)
        {
            // UI_MissionButton 뷰모델 주입
            if (_missionButton != null)
                _missionButton.SetViewModel(_viewModel.MissionButtonViewModel);

            // 뷰모델 이벤트 구독
            _viewModel.OnRequestUnusedButton += ShowUnusedLog;
        }
    }


    // --- ViewModel 호출을 위한 래퍼 ---

    private void OnUnusedButtonClick() => _viewModel?.OnUnusedButtonClicked();
    private void OnCampaignButtonClick() => _viewModel?.OnCampaignButtonClicked();

    // --- ViewModel 이벤트 핸들러 ---

    private void ShowUnusedLog()
    {
        Debug.Log("미구현 기능입니다.");
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        _campaignButton.onClick.RemoveListener(OnCampaignButtonClick);

        _shopButton.onClick.RemoveListener(OnUnusedButtonClick);
        _outpostButton.onClick.RemoveListener(OnUnusedButtonClick);
        _arkButton.onClick.RemoveListener(OnUnusedButtonClick);
        _friendButton.onClick.RemoveListener(OnUnusedButtonClick);
        _unionButton.onClick.RemoveListener(OnUnusedButtonClick);
        _commanderButton.onClick.RemoveListener(OnUnusedButtonClick);

        if (_viewModel != null)
        {
            _viewModel.OnRequestUnusedButton -= ShowUnusedLog;
        }

        // 뷰모델 해제 (Base에서 Release 호출되지만 명시적으로 null 처리)
        _viewModel = null;
    }
}
