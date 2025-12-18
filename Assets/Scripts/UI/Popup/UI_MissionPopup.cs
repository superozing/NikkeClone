using UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;
using System.Threading.Tasks;
using DG.Tweening;

public class UI_MissionPopup : UI_Popup, IUIShowHideAnimation
{
    public override string ActionMapKey => "UI_MissionPopup";

    [Header("Components")]
    [SerializeField] private TMP_Text _missionResetTimeText; // "H시간 M분 남음"
    [SerializeField] private Transform _missionSlotRoot;
    [SerializeField] private GameObject _missionCompleteBlockerImage;
    [SerializeField] private TMP_Text _missionCompleteTimerText; // "HH:MM::SS"

    [Header("Buttons")]
    [SerializeField] private Button _blocker;
    [SerializeField] private Button _exitButton;

    private MissionPopupViewModel _viewModel;

    // 내부 연출 객체
    private IUIAnimation _fadeInAnimation = new FadeInUIAnimation(0.3f, Ease.OutQuad);
    private IUIAnimation _fadeOutAnimation = new FadeOutUIAnimation(0.2f, Ease.InQuad);

    protected override void Awake()
    {
        base.Awake();

        // 1. Input Action 바인딩 (ESC 키로 닫기)
        Managers.Input.BindAction("Close", OnEscapeAction, InputActionPhase.Performed);

        // 2. Button OnClick 리스너 바인딩
        _blocker.onClick.AddListener(OnCloseClick);
        _exitButton.onClick.AddListener(OnCloseClick);

        // 연출 전략 수립
        _fadeInAnimation  = new FadeInUIAnimation(0.3f, Ease.OutQuad);
        _fadeOutAnimation = new FadeOutUIAnimation(0.2f, Ease.InQuad);
    }

    protected async void OnEnable()
    {
        await PlayShowAnimationAsync();
    }

    public async Task PlayShowAnimationAsync(float delay = 0f)
    {
        // CanvasGroup은 부모(UI_View)의 protected 멤버 사용
        if (_fadeInAnimation != null && _canvasGroup != null)
            await _fadeInAnimation.ExecuteAsync(_canvasGroup, delay);
    }

    public async Task PlayHideAnimationAsync(float delay = 0f)
    {
        if (_fadeOutAnimation != null && _canvasGroup != null)
            await _fadeOutAnimation.ExecuteAsync(_canvasGroup, delay);
    }

    private void OnEscapeAction(InputAction.CallbackContext _) => OnCloseClick();
    private void OnCloseClick() => _viewModel?.OnClose();

    public override void SetViewModel(ViewModelBase viewModel)
    {
        if (_viewModel != null)
            _viewModel.OnCloseRequested -= OnCloseRequested;

        _viewModel = viewModel as MissionPopupViewModel;
        if (_viewModel == null && viewModel != null)
        {
            Debug.LogError($"[UI_MissionPopup] 잘못된 ViewModel 타입이 주입되었습니다.");
            return;
        }

        base.SetViewModel(viewModel);

        if (_viewModel != null)
        {
            _viewModel.OnCloseRequested += OnCloseRequested;

            Bind(_viewModel.MissionResetTimeText, text => _missionResetTimeText.text = text);
            Bind(_viewModel.MissionCompleteTimerText, text => _missionCompleteTimerText.text = text);
            Bind(_viewModel.IsAllMissionsComplete, isComplete => _missionCompleteBlockerImage.SetActive(isComplete));

            // UI_MissionSlot 생성
            GenerateMissionSlots();
        }
    }

    /// <summary>
    /// ViewModel이 가진 SlotViewModels 목록을 기반으로 UI_MissionSlot을 생성합니다.
    /// </summary>
    private async void GenerateMissionSlots()
    {
        if (_viewModel == null || _missionSlotRoot == null || _viewModel.SlotViewModels == null)
            return;

        // ViewModel 리스트를 순회하며 UIManager에 생성을 요청합니다.
        foreach (var slotViewModel in _viewModel.SlotViewModels)
            await Managers.UI.ShowAsync<UI_MissionSlot>(slotViewModel, _missionSlotRoot);
    }

    private async void OnCloseRequested()
    {
        // FadeOut 이후 UI를 닫아요.
        await PlayHideAnimationAsync();

        Managers.UI.Close(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Managers.Input.UnbindAction("Close", OnEscapeAction, InputActionPhase.Performed);

        _blocker.onClick.RemoveListener(OnCloseClick);
        _exitButton.onClick.RemoveListener(OnCloseClick);

        if (_viewModel != null)
            _viewModel.OnCloseRequested -= OnCloseRequested;

        (_viewModel as IDisposable)?.Dispose();
        _viewModel = null;
    }
}