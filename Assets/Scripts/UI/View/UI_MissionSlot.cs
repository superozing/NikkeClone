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

    public override void SetViewModel(IViewModel viewModel)
    {
        _viewModel = viewModel as MissionSlotViewModel;
        if (_viewModel == null && viewModel != null)
        {
            Debug.LogError($"[UI_MissionSlot] 잘못된 ViewModel 타입이 주입되었습니다.");
            return;
        }

        // 최초 1회만 설정되어야 하는 데이터는 이 때 초기화하면 되겠죠.
        _titleText.text = _viewModel.Title;
        _descText.text = _viewModel.Description;

        if (_rewardIcon != null)
            _rewardIcon.SetViewModel(_viewModel.RewardIconViewModel);

        base.SetViewModel(_viewModel);
    }

    protected override void OnStateChanged()
    {
        if (_viewModel == null)
            return;

        // 1. 미션 정보 설정
        _progressText.text = _viewModel.ProgressText;

        // 2. 진행도 설정
        if (_progressBar != null)
            _progressBar.value = _viewModel.Progress;
        
        // 3. 진행 완료 시 색상 변경
        if (_viewModel.Progress == 1f)
            _fillImage.color = new Color(.2f, .7f, .9f);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        (_viewModel as IDisposable)?.Dispose();
        _viewModel = null;
    }
}