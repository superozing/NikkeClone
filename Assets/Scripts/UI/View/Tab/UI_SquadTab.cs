using UI;
using UnityEngine;

public class UI_SquadTab : UI_TabBase
{
    public override eTabType TabType => eTabType.Squad;

    private SquadTabViewModel _viewModel;

    public override void SetViewModel(ViewModelBase viewModel)
    {
        _viewModel = viewModel as SquadTabViewModel;
        base.SetViewModel(viewModel);
    }
}