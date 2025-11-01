using UI;
using UnityEngine;

public class UI_SquadTab : UI_TabBase
{
    public override eTabType TabType => eTabType.Squad;

    // private SquadTabViewModel _viewModel;

    public override void SetViewModel(IViewModel viewModel)
    {
        // _viewModel = viewModel as SquadTabViewModel;

        base.SetViewModel(viewModel);
    }

    protected override void OnStateChanged()
    {
        // if (_viewModel == null)
        // return;

        Debug.Log("UI_SquadTab.OnStateChanged() »£√‚µ ");
    }
}