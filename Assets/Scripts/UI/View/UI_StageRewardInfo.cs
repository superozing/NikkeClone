using UI;
using UnityEngine;

/// <summary>
/// 스테이지 보상 정보 View입니다.
/// UI_StageInfoPopup의 하위 UI로, 보상 아이콘을 표시합니다.
/// </summary>
public class UI_StageRewardInfo : UI_View
{
    [Header("Reward Icons")]
    [SerializeField] private UI_Icon[] _rewardIcons;

    private StageRewardInfoViewModel _viewModel;

    /// <summary>
    /// ViewModel을 설정하고 데이터 바인딩을 수행합니다.
    /// </summary>
    public override void SetViewModel(ViewModelBase viewModel)
    {
        _viewModel = viewModel as StageRewardInfoViewModel;

        base.SetViewModel(viewModel);

        if (_viewModel == null) return;

        // 1. RewardCount 구독하여 아이콘 활성화/비활성화
        Bind(_viewModel.RewardCount, UpdateRewardIcons);

        // 2. 각 보상 아이콘 ViewModel 연결
        int iconCount = Mathf.Min(_rewardIcons.Length, _viewModel.RewardIconViewModels.Count);
        for (int i = 0; i < iconCount; ++i)
        {
            _rewardIcons[i].SetViewModel(_viewModel.RewardIconViewModels[i]);
        }
    }

    /// <summary>
    /// 보상 개수에 따라 아이콘을 활성화/비활성화합니다.
    /// </summary>
    /// <param name="count">활성화할 아이콘 개수</param>
    private void UpdateRewardIcons(int count)
    {
        for (int i = 0; i < _rewardIcons.Length; ++i)
        {
            _rewardIcons[i].gameObject.SetActive(i < count);
        }
    }
}
