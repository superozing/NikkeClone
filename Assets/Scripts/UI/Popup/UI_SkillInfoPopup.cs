using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UI;

public class UI_SkillInfoPopup : UI_Popup, IUIShowHideAnimation
{
    // GameInputActions에 해당 맵이 추가되어야 함
    public override string ActionMapKey => "UI_SkillInfoPopup";

    [Header("Skill Slots")]
    [SerializeField] private UI_SkillSlot[] _skillSlots = new UI_SkillSlot[3];

    [Header("Buttons")]
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _blocker;

    private SkillInfoPopupViewModel _viewModel;

    // 연출 객체
    private readonly IUIAnimation _fadeIn = new FadeInUIAnimation(0.2f);
    private readonly IUIAnimation _fadeOut = new FadeOutUIAnimation(0.2f);

    protected override void Awake()
    {
        base.Awake();

        // Input Action 바인딩 (ESC 키로 닫기)
        Managers.Input.BindAction("Close", OnEscapeAction, InputActionPhase.Performed);

        if (_closeButton != null)
            _closeButton.onClick.AddListener(OnCloseClick);
        if (_blocker != null)
            _blocker.onClick.AddListener(OnCloseClick);
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

        _viewModel = viewModel as SkillInfoPopupViewModel;

        base.SetViewModel(viewModel);

        if (_viewModel == null) return;

        _viewModel.OnCloseRequested += OnCloseRequested;

        // 슬롯 뷰모델 연결 (정적 할당 방식)
        BindSkillSlots();
    }

    private void BindSkillSlots()
    {
        if (_viewModel.SlotViewModels == null) return;

        // 데이터가 보장된 3개라고 가정하고 순회
        for (int i = 0; i < _skillSlots.Length; i++)
        {
            if (_skillSlots[i] == null)
            {
                Debug.LogError($"[UI_SkillInfoPopup] Skill Slot {i} is not assigned in Inspector.");
                continue;
            }

            // ViewModel 리스트 범위 내에 있으면 주입
            if (i < _viewModel.SlotViewModels.Count)
            {
                _skillSlots[i].gameObject.SetActive(true);
                _skillSlots[i].SetViewModel(_viewModel.SlotViewModels[i]);
            }
            else
            {
                // 데이터가 모자란 경우 비활성화 (예외 처리)
                _skillSlots[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnEscapeAction(InputAction.CallbackContext context) => OnCloseClick();
    private void OnCloseClick() => _viewModel?.OnClickClose();

    private async void OnCloseRequested()
    {
        await PlayHideAnimationAsync();
        Managers.UI.Close(this);
    }

    // --- IUIShowHideAnimation 구현 ---

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

        if (_closeButton != null)
            _closeButton.onClick.RemoveListener(OnCloseClick);
        if (_blocker != null)
            _blocker.onClick.RemoveListener(OnCloseClick);

        if (_viewModel != null)
            _viewModel.OnCloseRequested -= OnCloseRequested;

        // 자식 슬롯 ViewModel 해제
        foreach (var slot in _skillSlots)
        {
            if (slot != null) slot.SetViewModel(null);
        }

        _viewModel = null;
    }
}