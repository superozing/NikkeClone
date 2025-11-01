using UI;
using UnityEngine;

public class UI_InventoryTab : UI_TabBase
{
	public override eTabType TabType => eTabType.Inventory;

	// private InventoryTabViewModel _viewModel;

	public override void SetViewModel(IViewModel viewModel)
	{
		// _viewModel = viewModel as InventoryTabViewModel;

		base.SetViewModel(viewModel);
	}

	protected override void OnStateChanged()
	{
		// if (_viewModel == null)
		// return;

		Debug.Log("UI_InventoryTab.OnStateChanged() »£√‚µ ");
	}
}
