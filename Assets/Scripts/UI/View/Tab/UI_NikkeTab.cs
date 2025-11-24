using UI;
using UnityEngine;

public class UI_NikkeTab : UI_TabBase
{
    public override eTabType TabType => eTabType.Nikke;

    private NikkeTabViewModel _viewModel;

    public override void SetViewModel(ViewModelBase viewModel)
    {
        _viewModel = viewModel as NikkeTabViewModel;
        base.SetViewModel(viewModel);
    }
}