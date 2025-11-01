using UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_TabButtonGroup : UI_View
{
    private TabGroupPopupViewModel _viewModel;

    [Header("Tab Button")]
    [SerializeField] private Button _lobbyButton;
    [SerializeField] private Button _squadButton;
    [SerializeField] private Button _nikkeButton;
    [SerializeField] private Button _inventoryButton;
    [SerializeField] private Button _recruitButton;

    private Button[] _buttonArray = new Button[(int)eTabType.End];

    protected override void Awake()
    {
        base.Awake();

        _buttonArray[(int)eTabType.Lobby] = _lobbyButton;
        _buttonArray[(int)eTabType.Squad] = _squadButton;
        _buttonArray[(int)eTabType.Nikke] = _nikkeButton;
        _buttonArray[(int)eTabType.Inventory] = _inventoryButton;
        _buttonArray[(int)eTabType.Recruit] = _recruitButton;
    }

    public override void SetViewModel(IViewModel viewModel)
    {
        if (_viewModel != null)
            UnbindButtonListeners();

        _viewModel = viewModel as TabGroupPopupViewModel;

        base.SetViewModel(viewModel);

        if (_viewModel == null && viewModel != null)
        {
            Debug.LogError($"[UI_TabButtonsGroup] 잘못된 ViewModel 타입이 주입되었습니다.");
            return;
        }

        if (_viewModel != null)
            BindButtonListeners();
    }

    protected override void OnStateChanged()
    {
        if (_viewModel == null)
            return;

        for (int i = 0; i < _buttonArray.Length; ++i)
        {
            // 선택된 탭은 버튼 비활성화
            // 시각효과도 넣으면 좋겠네요.
            _buttonArray[i].interactable = (i != (int)_viewModel.CurrentTabType);
        }
    }

    private void BindButtonListeners()
    {
        _buttonArray[(int)eTabType.Lobby].onClick.AddListener(() => _viewModel?.OnTabButtonClicked(eTabType.Lobby));
        _buttonArray[(int)eTabType.Squad].onClick.AddListener(() => _viewModel?.OnTabButtonClicked(eTabType.Squad));
        _buttonArray[(int)eTabType.Nikke].onClick.AddListener(() => _viewModel?.OnTabButtonClicked(eTabType.Nikke));
        _buttonArray[(int)eTabType.Inventory].onClick.AddListener(() => _viewModel?.OnTabButtonClicked(eTabType.Inventory));
        _buttonArray[(int)eTabType.Recruit].onClick.AddListener(() => _viewModel?.OnTabButtonClicked(eTabType.Recruit));
    }

    private void UnbindButtonListeners()
    {
        for (int i = 0; i < _buttonArray.Length; ++i)
            _buttonArray[i].onClick.RemoveAllListeners();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (_viewModel != null)
            UnbindButtonListeners();

        _viewModel = null;
        _buttonArray = null;
    }
}