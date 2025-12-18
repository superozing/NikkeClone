using UI;
using UnityEngine;

public class UI_NikkeTab : UI_TabBase
{
    public override eTabType TabType => eTabType.Nikke;

    [SerializeField] private UI_NikkeCardScrollView _scrollView;

    private NikkeTabViewModel _viewModel;

    public override void SetViewModel(ViewModelBase viewModel)
    {
        _viewModel = viewModel as NikkeTabViewModel;

        base.SetViewModel(viewModel);

        if (_viewModel != null && _scrollView != null)
            _scrollView.SetViewModel(_viewModel.ScrollViewModel);
    }

    public override void OnTabSelected()
    {
        base.OnTabSelected();

        // 탭 활성화 시 연출 호출
        if (_scrollView != null)
            _scrollView.PlayAnimationSequence();
    }
}