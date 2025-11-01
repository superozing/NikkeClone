using UI;
using UnityEngine;

public class UI_RecruitTab : UI_TabBase
{
    public override eTabType TabType => eTabType.Recruit;

    // private RecruitTabViewModel _viewModel;

    public override void SetViewModel(IViewModel viewModel)
    {
        // _viewModel = viewModel as RecruitTabViewModel;

        base.SetViewModel(viewModel);
    }

    protected override void OnStateChanged()
    {
        // if (_viewModel == null)
        // return;

        Debug.Log("UI_RecruitTab.OnStateChanged() »£√‚µ ");
    }
}
