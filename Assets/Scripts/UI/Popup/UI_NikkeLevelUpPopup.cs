using System;
using System.Threading.Tasks;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_NikkeLevelUpPopup : UI_Popup, IUIShowHideAnimation
{
    public override string ActionMapKey => "UI_NikkeLevelUpPopup";

    [Header("Title Area")]
    [SerializeField] private TMP_Text _currentLevelText;
    [SerializeField] private TMP_Text _nextLevelText;

    [Header("Stats Area")]
    [SerializeField] private TMP_Text _statHpValueText;
    [SerializeField] private TMP_Text _statHpIncText;

    [SerializeField] private TMP_Text _statAtkValueText;
    [SerializeField] private TMP_Text _statAtkIncText;

    [SerializeField] private TMP_Text _statDefValueText;
    [SerializeField] private TMP_Text _statDefIncText;

    [Header("Materials Area")]
    [SerializeField] private Transform _materialLayoutRoot;
    [SerializeField] private Button _inventoryButton;

    [Header("Control Buttons")]
    [SerializeField] private Button _minButton;
    [SerializeField] private Button _minusButton;
    [SerializeField] private TMP_Text _targetLevelText;
    [SerializeField] private Button _plusButton;
    [SerializeField] private Button _maxButton;

    [Header("Action Buttons")]
    [SerializeField] private Button _levelUpButton;
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _blocker;

    private NikkeLevelUpPopupViewModel _viewModel;
    private readonly IUIAnimation _fadeIn = new FadeInUIAnimation(0.2f);
    private readonly IUIAnimation _fadeOut = new FadeOutUIAnimation(0.2f);

    protected override void Awake()
    {
        base.Awake();

        Managers.Input.BindAction("Close", OnEscapeAction, InputActionPhase.Performed);

        _minButton.onClick.AddListener(() => _viewModel?.OnClickMin());
        _minusButton.onClick.AddListener(() => _viewModel?.OnClickMinus());
        _plusButton.onClick.AddListener(() => _viewModel?.OnClickPlus());
        _maxButton.onClick.AddListener(() => _viewModel?.OnClickMax());

        _inventoryButton.onClick.AddListener(() => _viewModel?.OnClickInventory());
        _levelUpButton.onClick.AddListener(() => _viewModel?.OnClickLevelUp());

        _closeButton.onClick.AddListener(OnCloseClick);
        _blocker.onClick.AddListener(OnCloseClick);
    }

    protected async void OnEnable()
    {
        await PlayShowAnimationAsync();
    }

    public override void SetViewModel(ViewModelBase viewModel)
    {
        if (_viewModel != null)
            _viewModel.OnCloseRequested -= OnCloseRequested;

        _viewModel = viewModel as NikkeLevelUpPopupViewModel;
        base.SetViewModel(viewModel);

        if (_viewModel == null) return;

        _viewModel.OnCloseRequested += OnCloseRequested;

        // Binding
        Bind(_viewModel.CurrentLevelStr, text => SetText(_currentLevelText, text));
        Bind(_viewModel.NextLevelStr, text => SetText(_nextLevelText, text));
        Bind(_viewModel.TargetLevelStr, text => SetText(_targetLevelText, text));

        Bind(_viewModel.StatHpValue, text => SetText(_statHpValueText, text));
        Bind(_viewModel.StatHpInc, text => SetText(_statHpIncText, text));

        Bind(_viewModel.StatAtkValue, text => SetText(_statAtkValueText, text));
        Bind(_viewModel.StatAtkInc, text => SetText(_statAtkIncText, text));

        Bind(_viewModel.StatDefValue, text => SetText(_statDefValueText, text));
        Bind(_viewModel.StatDefInc, text => SetText(_statDefIncText, text));

        // Buttons
        Bind(_viewModel.IsMinusActive, active => _minusButton.interactable = active);
        Bind(_viewModel.IsPlusActive, active => _plusButton.interactable = active);
        Bind(_viewModel.IsLevelUpInteractable, active => _levelUpButton.interactable = active);

        UpdateMaterials();
    }

    private void UpdateMaterials()
    {
        if (_materialLayoutRoot == null || _viewModel == null) return;

        // TODO: UI_ItemIcon을 감싸는 새로운 UI 생성 예정.
        /*
        // 추후 구현 로직:
        
        // 1. 필요한 재료 목록 순회 (여기서는 Credit 하나만 예시)
        int required = _viewModel.RequiredCredit;
        bool isEnough = _viewModel.HasEnoughCredit;

        // 2. 아이콘 프리팹 생성 or 풀링
        // var iconUI = Instantiate(IconPrefab, _materialLayoutRoot);
        
        // 3. 데이터 설정
        // iconUI.SetCount(required);
        
        // 4. 재료 부족 시 텍스트 붉은색 처리 및 아이콘 비활성 느낌 처리
        if (!isEnough) 
        {
             // 텍스트 색상 변경
             // iconUI.SetTextColor(Color.red);
             
             // 필요시 반투명 처리 등
             // iconUI.SetAlpha(0.5f);
        }
        else
        {
             // iconUI.SetTextColor(Color.white);
             // iconUI.SetAlpha(1.0f);
        }
        */
    }

    private void SetText(TMP_Text target, string text)
    {
        if (target != null) target.text = text;
    }

    private void OnEscapeAction(InputAction.CallbackContext ctx) => OnCloseClick();
    private void OnCloseClick() => _viewModel?.OnClickClose();

    private async void OnCloseRequested()
    {
        await PlayHideAnimationAsync();
        Managers.UI.Close(this);
    }

    public async Task PlayShowAnimationAsync(float delay = 0)
    {
        if (_fadeIn != null && _canvasGroup != null)
            await _fadeIn.ExecuteAsync(_canvasGroup, delay);
    }

    public async Task PlayHideAnimationAsync(float delay = 0)
    {
        if (_fadeOut != null && _canvasGroup != null)
            await _fadeOut.ExecuteAsync(_canvasGroup, delay);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Managers.Input.UnbindAction("Close", OnEscapeAction, InputActionPhase.Performed);

        _minButton.onClick.RemoveAllListeners();
        _minusButton.onClick.RemoveAllListeners();
        _plusButton.onClick.RemoveAllListeners();
        _maxButton.onClick.RemoveAllListeners();
        _inventoryButton.onClick.RemoveAllListeners();
        _levelUpButton.onClick.RemoveAllListeners();
        _closeButton.onClick.RemoveAllListeners();
        _blocker.onClick.RemoveAllListeners();

        if (_viewModel != null)
            _viewModel.OnCloseRequested -= OnCloseRequested;

        _viewModel = null;
    }
}