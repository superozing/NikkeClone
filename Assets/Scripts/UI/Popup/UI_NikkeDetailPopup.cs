using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_NikkeDetailPopup : UI_Popup, IUIShowHideAnimation
{
    public override string ActionMapKey => "UI_NikkeDetailPopup";

    [SerializeField] private UI_NikkeDetailStatus _detailStatusView;
    [SerializeField] private UI_Money _moneyView;

    [SerializeField] private Image _standingImage;
    [SerializeField] private Button _backButton;

    [SerializeField] private List<Graphic> _colorTargets = new List<Graphic>();

    private NikkeDetailPopupViewModel _viewModel;

    // 연출 객체
    private readonly IUIAnimation _fadeIn = new FadeInUIAnimation(0.3f);
    private readonly IUIAnimation _fadeOut = new FadeOutUIAnimation(0.2f);

    protected override void Awake()
    {
        base.Awake();

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
            graphic.color = color;
    }

    private void OnCloseClick() => _viewModel?.OnClickClose();
    private void OnEscapeAction(InputAction.CallbackContext ctx) => OnCloseClick();

    private async void OnCloseRequested()
    {
        await PlayHideAnimationAsync();
        Managers.UI.Close(this);
    }

    // --- IUIShowHideAnimation Implementation ---

    public async Task PlayShowAnimationAsync(float delay = 0f)
    {
        if (_fadeIn != null && _canvasGroup != null)
            await _fadeIn.ExecuteAsync(_canvasGroup, delay);
    }

    public async Task PlayHideAnimationAsync(float delay = 0f)
    {
        if (_fadeOut != null && _canvasGroup != null)
            await _fadeOut.ExecuteAsync(_canvasGroup, delay);
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