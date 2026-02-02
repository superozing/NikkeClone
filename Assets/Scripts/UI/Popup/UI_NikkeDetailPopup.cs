using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UI;

public class UI_NikkeDetailPopup : UI_Popup, IUIShowHideable
{
    public override string ActionMapKey => "UI_NikkeDetailPopup";

    [SerializeField] private UI_NikkeDetailStatus _detailStatusView;
    [SerializeField] private UI_Money _moneyView;

    [SerializeField] private Image _standingImage;
    [SerializeField] private Button _backButton;

    [SerializeField] private List<Graphic> _colorTargets = new List<Graphic>();

    private NikkeDetailPopupViewModel _viewModel;

    // 연출 객체
    private IUIAnimation _showAnim;
    private IUIAnimation _hideAnim;

    protected override void Awake()
    {
        base.Awake();

        // Show: Alpha 0 -> 1
        _showAnim = new FadeUIAnimation(_canvasGroup, 0f, 1f, 0.3f);

        // Hide: Alpha 1 -> 0
        _hideAnim = new FadeUIAnimation(_canvasGroup, 1f, 0f, 0.3f);

        if (_backButton != null)
            _backButton.onClick.AddListener(OnCloseClick);

        // ESC 키 바인딩
        Managers.Input.BindAction("Close", OnEscapeAction, InputActionPhase.Performed);
    }
    protected async void OnEnable()
    {
        await PlayShowAnimationAsync();
    }

    public override void SetViewModel(ViewModelBase viewModel)
    {
        if (_viewModel != null)
        {
            _viewModel.OnCloseRequested -= OnCloseRequested;
        }

        _viewModel = viewModel as NikkeDetailPopupViewModel;

        base.SetViewModel(viewModel);

        if (_viewModel == null)
            return;

        _viewModel.OnCloseRequested += OnCloseRequested;

        // 1. 하위 뷰모델 바인딩 (Money)
        // MoneyViewModel은 한번 생성되고 유지되므로 바로 주입
        if (_moneyView != null)
            _moneyView.SetViewModel(_viewModel.MoneyViewModel);

        // 2. 동적 하위 뷰모델 바인딩 (Status)
        // StatusViewModel이 교체될 때마다 하위 View에 주입
        Bind(_viewModel.StatusViewModel, OnStatusViewModelChanged);

        // 3. 이미지 및 색상 바인딩
        Bind(_viewModel.NikkeStandingImage, UpdateStandingImage);
        Bind(_viewModel.ThemeColor, UpdateThemeColor);
    }

    private void OnStatusViewModelChanged(NikkeDetailStatusViewModel statusVM)
    {
        if (_detailStatusView != null)
            _detailStatusView.SetViewModel(statusVM);
    }

    private void UpdateStandingImage(Sprite sprite) => _standingImage.sprite = sprite;

    private void UpdateThemeColor(Color color)
    {
        foreach (var graphic in _colorTargets)
        {
            color.a = graphic.color.a;
            graphic.color = color;
        }
    }

    private void OnCloseClick() => _viewModel?.OnClickClose();
    private void OnEscapeAction(InputAction.CallbackContext ctx) => OnCloseClick();

    private async void OnCloseRequested()
    {
        await PlayHideAnimationAsync();
        Managers.UI.Close(this);
    }
    // --- IUIShowHideable Implementation ---

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

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Managers.Input.UnbindAction("Close", OnEscapeAction, InputActionPhase.Performed);

        if (_backButton != null)
            _backButton.onClick.RemoveListener(OnCloseClick);

        if (_viewModel != null)
            _viewModel.OnCloseRequested -= OnCloseRequested;

        if (_moneyView != null)
            _moneyView.SetViewModel(null);

        if (_detailStatusView != null)
            _detailStatusView.SetViewModel(null);

        _viewModel = null;
    }
}