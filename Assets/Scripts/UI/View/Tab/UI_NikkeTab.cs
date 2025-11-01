using UI;
using UnityEngine;

public class UI_NikkeTab : UI_TabBase
{
    public override eTabType TabType => eTabType.Nikke;

    // private NikkeTabViewModel _viewModel;

    public override void SetViewModel(IViewModel viewModel)
    {
        // _viewModel = viewModel as NikkeTabViewModel;

        base.SetViewModel(viewModel);
    }

    protected override void OnStateChanged()
    {
        // if (_viewModel == null)
        // return;

        Debug.Log("UI_NikkeTab.OnStateChanged() »£√‚µ ");
    }
}