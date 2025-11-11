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

        base.SetViewModel(_viewModel);
    }

    protected override void OnStateChanged()
    {
        if (_viewModel == null)
            return;

        // 1. 미션 정보 설정
        // 제목과 설명 같은 경우에는 최초 한 번만 세팅하도록 하여 최적화를 할 수 있겠죠.
        _titleText.text = _viewModel.Title;
        _descText.text = _viewModel.Description;
        _progressText.text = _viewModel.ProgressText;

        // 2. 진행도 설정
        if (_progressBar != null)
            _progressBar.value = _viewModel.Progress;
        
        // 3. 진행 완료 시 색상 변경
        if (_viewModel.Progress == 1f)
            _fillImage.color = new Color(.2f, .7f, .9f);

        // 3. UI_Icon에 뷰모델 바인딩
        if (_rewardIcon != null)
            _rewardIcon.SetViewModel(_viewModel.RewardIconViewModel);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        (_viewModel as IDisposable)?.Dispose();
        _viewModel = null;
    }
}