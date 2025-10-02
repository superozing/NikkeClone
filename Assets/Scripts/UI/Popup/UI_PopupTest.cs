using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_PopupTest : UI_Popup
{
    [SerializeField] private Button _confirmButton;
    [SerializeField] private TMP_Text _titleText;

    private PopupTestViewModel _viewModel;

    public override void SetViewModel(IViewModel viewModel)
    {
        base.SetViewModel(viewModel);
        _viewModel = viewModel as PopupTestViewModel;
    }

    protected override void Awake()
    {
        base.Awake();

        _confirmButton.onClick.AddListener(() => _viewModel?.OnConfirm());
    }

    protected override void OnStateChanged()
    {
        if (_viewModel == null) 
            return;

        _titleText.text = _viewModel.Title;

        if (_viewModel._clickCount == 10)
            Managers.UI.Close(this);
    }
}