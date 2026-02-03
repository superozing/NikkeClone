using UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;
using System.Threading.Tasks;

public class UI_ItemDetailPopup : UI_Popup, IUIShowHideable
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

    [Header("Background Image")]
    [SerializeField] private RectTransform _bgImageRectTransform;

    private ItemDetailPopupViewModel _viewModel;

    private IUIAnimation _showAnim;
    private IUIAnimation _hideAnim;

    protected override void Awake()
    {
        base.Awake();
        _showAnim = new FadeUIAnimation(_canvasGroup, 0f, 1f, 0.2f);
        _hideAnim = new FadeUIAnimation(_canvasGroup, 1f, 0f, 0.2f);

        // 1. Input Action 바인딩 (ESC 키로 닫기)
        Managers.Input.BindAction("Close", OnEscapeAction, InputActionPhase.Performed);

        // 2. Button OnClick 리스너 바인딩
        _okButton.onClick.AddListener(OnExitClick);
        _exitButton.onClick.AddListener(OnExitClick);
        _blocker.onClick.AddListener(OnExitClick);
    }

    protected async void OnEnable()
    {
        // 활성화 시 연출 시작
        await PlayShowAnimationAsync();
    }

    // --- IUIShowHideable 구현 ---

    public async Task PlayShowAnimationAsync(float delay = 0f)
    {
        if (_showAnim != null)
            await _showAnim.ExecuteAsync(delay);
    }

    public async Task PlayHideAnimationAsync(float delay = 0f)
    {
        if (_hideAnim != null)
            await _hideAnim.ExecuteAsync();
    }

    // --------------------------------

    public override void SetViewModel(ViewModelBase viewModel)
    {
        if (_viewModel != null)
            _viewModel.OnClose -= OnCloseRequested;

        _viewModel = viewModel as ItemDetailPopupViewModel;
        if (_viewModel == null && viewModel != null)
        {
            Debug.LogError($"[UI_ItemDetailPopup] 잘못된 ViewModel 타입이 주입되었습니다. Expected: {nameof(ItemDetailPopupViewModel)}, Actual: {viewModel.GetType()}");
            return;
        }

        base.SetViewModel(viewModel);

        if (_viewModel != null)
        {
            _viewModel.OnClose += OnCloseRequested;

            // ReactiveProperty 바인딩
            Bind(_viewModel.ItemName, text => _itemNameText.text = text);
            Bind(_viewModel.QuantityText, text => _quantityText.text = text);
            Bind(_viewModel.DescText, text =>
            {
                _descText.text = text;
                // 텍스트 변경 시 레이아웃 갱신
                if (_bgImageRectTransform != null)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(_bgImageRectTransform);
            });

            // 자식 ViewModel 바인딩
            if (_icon != null)
                _icon.SetViewModel(_viewModel.IconViewModel);
        }
    }

    private void OnExitClick() => _viewModel?.OnExit();
    private void OnEscapeAction(InputAction.CallbackContext context) => OnExitClick();

    /// <summary>
    /// ViewModel이 OnClose 이벤트를 호출했을 때(버튼 클릭 시) 실행됩니다.
    /// </summary>
    private async void OnCloseRequested()
    {
        await PlayHideAnimationAsync();

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