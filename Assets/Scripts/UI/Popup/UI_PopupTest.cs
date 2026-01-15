using TMPro;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_PopupTest : UI_Popup
{
    public override string ActionMapKey => "UI_PopupTest";

    [SerializeField] private Button _confirmButton;
    [SerializeField] private TMP_Text _titleText;

    private PopupTestViewModel _viewModel;

    public override void SetViewModel(ViewModelBase viewModel)
    {
        if (_viewModel != null)
            _viewModel.OnEscapeKeyDown -= OnClose;

        _viewModel = viewModel as PopupTestViewModel;

        base.SetViewModel(viewModel);

        if (_viewModel != null)
        {
            _viewModel.OnEscapeKeyDown += OnClose;
            Bind(_viewModel.Title, text => _titleText.text = text);
        }
    }

    protected override void Awake()
    {
        base.Awake();

        Managers.Input.BindAction("Close", OnEscapeAction, UnityEngine.InputSystem.InputActionPhase.Performed);
        _confirmButton.onClick.AddListener(OnConfirmAction);
    }

    private void OnEscapeAction(InputAction.CallbackContext context) => _viewModel?.OnEscape();
    private void OnConfirmAction() => _viewModel?.OnConfirm();

    void OnClose()
    {
        Debug.Log("UI_PopupTest 를 닫습니다.");
        Managers.UI.Close(this);
    }
}