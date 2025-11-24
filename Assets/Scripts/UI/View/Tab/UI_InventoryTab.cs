using UI;
using UnityEngine;

public class UI_InventoryTab : UI_TabBase
{
    public override eTabType TabType => eTabType.Inventory;

    private InventoryTabViewModel _viewModel;

    public override void SetViewModel(ViewModelBase viewModel)
    {
        _viewModel = viewModel as InventoryTabViewModel;
        base.SetViewModel(viewModel);
    }
}