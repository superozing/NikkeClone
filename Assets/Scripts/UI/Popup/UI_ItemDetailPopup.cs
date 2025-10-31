using UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;

public class UI_ItemDetailPopup : UI_Popup
{
    public override string ActionMapKey => "UI_ItemDetailPopup";

    [Header("Components")]
    [SerializeField] private TMP_Text _itemNameText;
    [SerializeField] private TMP_Text _quantityText;
    [SerializeField] private TMP_Text _descText;
    [SerializeField] private UI_Icon _icon;

    [Header("Buttons")]
    [SerializeField] private Button _okButton;
    [SerializeField] private Button _exitButton;
    [SerializeField] private Button _blocker;

    private ItemDetailPopupViewModel _viewModel;
    
    private readonly FadeInUIAnimation _fadeIn = new(0.2f);
    private readonly FadeOutUIAnimation _fadeOut = new(0.2f);
    private CanvasGroup _cg;

    protected override void Awake()
    {
        base.Awake();

        // 1. Input Action 바인딩 (ESC 키로 닫기)
        Managers.Input.BindAction("Close", OnEscapeAction, InputActionPhase.Performed);

        // 2. Button OnClick 리스너 바인딩
        _okButton.onClick.AddListener(OnExitClick);
        _exitButton.onClick.AddListener(OnExitClick);
        _blocker.onClick.AddListener(OnExitClick);

        // 3. CanvasGroup 캐싱 (연출 호출 위해서)
        _cg = gameObject.GetOrAddComponent<CanvasGroup>();
    }

    protected async void OnEnable()
    {
        // 활성화 시 연출 시작
        if (_fadeIn != null)
            await _fadeIn.ExecuteAsync(_cg);
    }

    public override void SetViewModel(IViewModel viewModel)
    {
        if (_viewModel != null)
            _viewModel.OnClose -= OnCloseRequested;

        // ViewModel 타입 캐스팅
        _viewModel = viewModel as ItemDetailPopupViewModel;
        if (_viewModel == null && viewModel != null)
        {
            Debug.LogError($"[UI_ItemDetailPopup] 잘못된 ViewModel 타입이 주입되었습니다. Expected: {nameof(ItemDetailPopupViewModel)}, Actual: {viewModel.GetType()}");
            return;
        }

        base.SetViewModel(_viewModel);

        if (_viewModel != null)
            _viewModel.OnClose += OnCloseRequested;
    }

    protected override void OnStateChanged()
    {
        if (_viewModel == null) return;

        // 1. 텍스트 정보 바인딩
        _itemNameText.text = _viewModel.ItemName;
        _quantityText.text = _viewModel.QuantityText;
        _descText.text = _viewModel.DescText;

        // 2. 자식 View(UI_Icon)에 자식 ViewModel(IconViewModel)을 바인딩
        if (_icon != null)
            _icon.SetViewModel(_viewModel.IconViewModel);
    }

    private void OnExitClick() => _viewModel?.OnExit();
    private void OnEscapeAction(InputAction.CallbackContext context) => OnExitClick();

    /// <summary>
    /// ViewModel이 OnClose 이벤트를 호출했을 때(버튼 클릭 시) 실행됩니다.
    /// </summary>
    private async void OnCloseRequested()
    {
        if (_fadeOut != null)
            await _fadeOut.ExecuteAsync(_cg);

        // 연출 이후 자신을 닫아요.
        Managers.UI.Close(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // Input Action 바인딩 해제
        Managers.Input.UnbindAction("Close", OnEscapeAction, InputActionPhase.Performed);

        _okButton.onClick.RemoveListener(OnExitClick);
        _exitButton.onClick.RemoveListener(OnExitClick);
        _blocker.onClick.RemoveListener(OnExitClick);

        if (_viewModel != null)
            _viewModel.OnClose -= OnCloseRequested;

        (_viewModel as IDisposable)?.Dispose();
        _viewModel = null;
    }
}