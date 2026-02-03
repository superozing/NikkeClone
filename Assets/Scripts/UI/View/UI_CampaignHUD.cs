using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI;

public class UI_CampaignHUD : UI_View, IUIShowHideable
{
    // === Serialized Fields (View References) ===
    [SerializeField] private UI_Money _moneyView;           // 중앙 상단
    [SerializeField] private UI_MissionButton _missionButtonView; // 우상단

    // 임시로 추가한 UI들
    [SerializeField] private Button _settingsButton;        // 우상단 (설정 버튼)
    [SerializeField] private RectTransform _minimapPanel;   // 좌상단 (미니맵 패널)
    [SerializeField] private Button _backButton;            // 좌하단 (뒤로가기)
    [SerializeField] private RectTransform _chapterPanel;   // 우하단 (챕터 UI)

    [Header("UI Containers")]
    [SerializeField] private RectTransform _topCenterContainer;  // UI_Money 컨테이너
    [SerializeField] private RectTransform _topRightContainer;   // MissionButton+Settings 컨테이너
    [SerializeField] private RectTransform _topLeftContainer;    // Minimap 컨테이너
    [SerializeField] private RectTransform _bottomLeftContainer; // Back 컨테이너
    [SerializeField] private RectTransform _bottomRightContainer;// Chapter 컨테이너

    private CampaignHUDViewModel _viewModel;

    private Dictionary<RectTransform, IUIAnimation> _showAnimations;
    private Dictionary<RectTransform, IUIAnimation> _hideAnimations;

    protected override void Awake()
    {
        base.Awake();
        InitializeAnimations();

        // Event Listeners
        if (_backButton != null)
            _backButton.onClick.AddListener(OnBackButtonClicked);

        if (_settingsButton != null)
            _settingsButton.onClick.AddListener(OnSettingsButtonClicked);
    }

    private void OnSettingsButtonClicked()
    {
        Debug.Log("[UI_CampaignHUD] 세팅 버튼 클릭됨(구현해야 해요)");
        _viewModel?.HandleSettingsButtonClicked();
    }

    private void OnBackButtonClicked()
    {
        Debug.Log("[UI_CampaignHUD] 뒤로가기 버튼 클릭됨(구현해야 해요)");
        _viewModel?.HandleBackButtonClicked();
    }

    private void InitializeAnimations()
    {
        _showAnimations = new Dictionary<RectTransform, IUIAnimation>();
        _hideAnimations = new Dictionary<RectTransform, IUIAnimation>();

        // 각 컨테이너별 애니메이션 등록
        RegisterAnimation(_topCenterContainer, Vector2.up);
        RegisterAnimation(_topRightContainer, Vector2.right);
        RegisterAnimation(_topLeftContainer, Vector2.left);
        RegisterAnimation(_bottomLeftContainer, Vector2.left);
        RegisterAnimation(_bottomRightContainer, Vector2.right);
    }

    private void RegisterAnimation(RectTransform container, Vector2 direction)
    {
        if (container == null) return;

        var cg = container.gameObject.GetOrAddComponent<CanvasGroup>();

        const float offset = 100f;
        Vector2 offsetPos = direction * offset;

        // Show: Fade In + Slide In
        _showAnimations[container] = new UIAnimationComposite(
            new FadeUIAnimation(cg, 0f, 1f),
            new UIAnimationSlide(container, offsetPos, Vector2.zero)
        );

        // Hide: Fade Out + Slide Out
        _hideAnimations[container] = new UIAnimationComposite(
            new FadeUIAnimation(cg, 1f, 0f),
            new UIAnimationSlide(container, Vector2.zero, offsetPos)
        );
    }

    public override void SetViewModel(ViewModelBase viewModel)
    {
        _viewModel = viewModel as CampaignHUDViewModel;

        if (_viewModel == null && viewModel != null)
        {
            Debug.LogError($"[UI_CampaignHUD] 잘못된 뷰모델 타입입니다: {viewModel.GetType()}");
            return;
        }

        base.SetViewModel(viewModel);

        if (_viewModel != null)
        {
            // Bind Child ViewModels
            if (_moneyView != null && _viewModel.MoneyViewModel != null)
                _moneyView.SetViewModel(_viewModel.MoneyViewModel);

            if (_missionButtonView != null && _viewModel.MissionButtonViewModel != null)
                _missionButtonView.SetViewModel(_viewModel.MissionButtonViewModel);
        }
    }

    public async Task PlayShowAnimationAsync(float delay = 0f)
    {
        var tasks = new List<Task>();
        foreach (var anim in _showAnimations.Values)
        {
            tasks.Add(anim.ExecuteAsync(delay)); // delay 전파
        }
        await Task.WhenAll(tasks);
    }

    public async Task PlayHideAnimationAsync(float delay = 0f)
    {
        if (delay > 0) await Task.Delay(TimeSpan.FromSeconds(delay));

        var tasks = new List<Task>();
        foreach (var anim in _hideAnimations.Values)
        {
            tasks.Add(anim.ExecuteAsync());
        }
        await Task.WhenAll(tasks);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _viewModel = null;
    }
}
