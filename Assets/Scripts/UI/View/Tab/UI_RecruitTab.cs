using UI;
using UnityEngine;

public class UI_RecruitTab : UI_TabBase
{
    public override eTabType TabType => eTabType.Recruit;

    private RecruitTabViewModel _viewModel;

    public override void SetViewModel(ViewModelBase viewModel)
    {
        _viewModel = viewModel as RecruitTabViewModel;
        base.SetViewModel(viewModel);
    }
}