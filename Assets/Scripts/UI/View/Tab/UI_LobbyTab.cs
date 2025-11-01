using UI;
using UnityEngine;

public class UI_LobbyTab : UI_TabBase
{
    public override eTabType TabType => eTabType.Lobby;

    // private LobbyTabViewModel _viewModel;

    public override void SetViewModel(IViewModel viewModel)
    {
        // _viewModel = viewModel as LobbyTabViewModel;

        base.SetViewModel(viewModel);
    }

    protected override void OnStateChanged()
    {
        // if (_viewModel == null)
        // return;

        Debug.Log("UI_LobbyTab.OnStateChanged() »£√‚µ ");
    }
}
