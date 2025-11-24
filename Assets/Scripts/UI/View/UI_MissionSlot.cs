using System;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_MissionSlot : UI_View
{
    [Header("Components")]
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _descText;
    [SerializeField] private Slider _progressBar;
    [SerializeField] private Image _fillImage;
    [SerializeField] private TMP_Text _progressText;
    [SerializeField] private UI_Icon _rewardIcon;

    private MissionSlotViewModel _viewModel;

    public override void SetViewModel(ViewModelBase viewModel)
    {
        if (_viewModel != null)
            _viewModel.OnRequestRewardPopup -= ShowRewardPopup;

        _viewModel = viewModel as MissionSlotViewModel;
        if (_viewModel == null && viewModel != null)
        {
            Debug.LogError($"[UI_MissionSlot] 잘못된 ViewModel 타입이 주입되었습니다.");
            return;
        }

        base.SetViewModel(viewModel);

        if (_viewModel != null)
        {
            // 정적 프로퍼티는 이벤트 바인딩을 하지 않아요.
            _titleText.text = _viewModel.Title;
            _descText.text = _viewModel.Description;

            // 자식 뷰모델 연결
            if (_rewardIcon != null)
                _rewardIcon.SetViewModel(_viewModel.RewardIconViewModel);

            _viewModel.OnRequestRewardPopup += ShowRewardPopup;

            // ReactiveProperty 바인딩
            Bind(_viewModel.ProgressText, text => _progressText.text = text);
            Bind(_viewModel.Progress, value => { if (_progressBar != null) _progressBar.value = value; });
            Bind(_viewModel.MissionState, UpdateStateVisuals);
        }
    }

    private void UpdateStateVisuals(eMissionState state)
    {
        if (_fillImage != null)
        {
            if (state == eMissionState.Completed)
                _fillImage.color = new Color(.2f, .7f, .9f);
            else
                _fillImage.color = new Color(.2f, .2f, .2f);
        }
    }

    private async void ShowRewardPopup(int itemID, int count)
    {
        Debug.Log($"[UI_MissionSlot] 팝업 생성 요청: ItemID({itemID}), Count({count})");
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        (_viewModel as IDisposable)?.Dispose();
        _viewModel = null;
    }
}