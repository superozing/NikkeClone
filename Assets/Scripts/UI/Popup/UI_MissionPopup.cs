using UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;

public class UI_MissionPopup : UI_Popup
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

    private readonly FadeInUIAnimation _fadeIn = new(0.2f);
    private readonly FadeOutUIAnimation _fadeOut = new(0.2f);

    protected override void Awake()
    {
        base.Awake();

        // 1. Input Action 바인딩 (ESC 키로 닫기)
        Managers.Input.BindAction("Close", OnEscapeAction, InputActionPhase.Performed);

        // 2. Button OnClick 리스너 바인딩
        _blocker.onClick.AddListener(OnCloseClick);
        _exitButton.onClick.AddListener(OnCloseClick);
    }

    protected async void OnEnable()
    {
        if (_fadeIn != null)
            await _fadeIn.ExecuteAsync(_canvasGroup);
    }

    private void OnEscapeAction(InputAction.CallbackContext _) => OnCloseClick();
    private void OnCloseClick() => _viewModel?.OnClose();

    public override void SetViewModel(IViewModel viewModel)
    {
        if (_viewModel != null)
            _viewModel.OnCloseRequested -= OnCloseRequested;

        _viewModel = viewModel as MissionPopupViewModel;
        if (_viewModel == null && viewModel != null)
        {
            Debug.LogError($"[UI_MissionPopup] 잘못된 ViewModel 타입이 주입되었습니다.");
            return;
        }

        if (_viewModel != null)
            _viewModel.OnCloseRequested += OnCloseRequested;


        // UI_MissionSlot 생성
        GenerateMissionSlots();

        base.SetViewModel(_viewModel);
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

    protected override void OnStateChanged()
    {
        if (_viewModel == null) 
            return;

        _missionResetTimeText.text = _viewModel.MissionResetTimeText;
        _missionCompleteTimerText.text = _viewModel.MissionCompleteTimerText;
        _missionCompleteBlockerImage.SetActive(_viewModel.IsAllMissionsComplete);
    }

    private async void OnCloseRequested()
    {
        // FadeOut 이후 UI를 닫아요.
        await _fadeOut.ExecuteAsync(_canvasGroup);
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