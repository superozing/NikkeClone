using DG.Tweening;
using System;
using System.Threading.Tasks;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_NikkeCard : UI_View, IUIShowHideAnimation
{
    [Header("Texts")]
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _nameText;

    [Header("Marquee Settings")]
    [SerializeField] private RectTransform _nameTextRect;
    private CanvasGroup _nameTextCanvasGroup;
    [SerializeField] private RectTransform _nameMaskRect;

    [Header("Images")]
    [SerializeField] private Image _faceImage;
    [SerializeField] private Image _classIcon;
    [SerializeField] private Image _codeIcon;
    [SerializeField] private Image _weaponIcon;
    [SerializeField] private Image _burstIcon;

    [Header("Interaction")]
    [SerializeField] private Button _cardButton;

    private NikkeCardViewModel _viewModel;

    private MarqueeUIAnimation _marqueeAnim;

    private readonly IUIAnimation _showAnim = new VerticalSlideFadeUIAnimation(0.3f, -50f, Ease.OutQuad);

    protected override void Awake()
    {
        base.Awake();
        _cardButton.onClick.AddListener(OnClicked);

        // 애니메이션 전략 초기화 (속도 30, 딜레이 1.5초)
        if (_nameMaskRect != null)
            _marqueeAnim = new MarqueeUIAnimation(_nameMaskRect, 30f, 1.5f);

        _nameTextCanvasGroup = _nameTextRect.gameObject.GetOrAddComponent<CanvasGroup>();
    }

    public override void SetViewModel(ViewModelBase viewModel)
    {
        // 1. 기존 애니메이션 정리
        _marqueeAnim?.Kill();
        if (_nameTextRect != null)
            _nameTextRect.anchoredPosition = Vector2.zero;

        base.SetViewModel(viewModel);

        _viewModel = viewModel as NikkeCardViewModel;
        if (_viewModel == null) return;

        // 2. 데이터 바인딩
        Bind(_viewModel.Level, lv => _levelText.text = $"Lv.{lv}");

        // 텍스트 변경 시 Marquee 애니메이션 갱신
        Bind(_viewModel.Name, UpdateNameText);

        Bind(_viewModel.FaceImage, SetSprite(_faceImage));
        Bind(_viewModel.ClassIcon, SetSprite(_classIcon));
        Bind(_viewModel.CodeIcon, SetSprite(_codeIcon));
        Bind(_viewModel.WeaponIcon, SetSprite(_weaponIcon));
        Bind(_viewModel.BurstIcon, SetSprite(_burstIcon));
    }

    // --- IUIShowHideAnimation Implementation ---

    public async Task PlayShowAnimationAsync(float delay = 0)
    {
        if (_showAnim != null && _canvasGroup != null)
            await _showAnim.ExecuteAsync(_canvasGroup);
    }

    public Task PlayHideAnimationAsync(float delay = 0)
    {
        // 퇴장 연출이 쓸모 없다. 사용할 일이 없다.
        // 이렇게 되면 .. 인터페이스를 show와 hide를 분리해야 하는 게 올바른 방향 아닐까요?
        // 일단 이렇게만 적어두고 나중에 수정하던가 해야겠어요.
        return Task.CompletedTask;
    }

    // -------------------------------------------

    private Action<Sprite> SetSprite(Image targetImage)
    {
        return (sprite) =>
        {
            if (targetImage == null) return;
            bool isValid = sprite != null;
            targetImage.gameObject.SetActive(isValid);
            if (isValid) targetImage.sprite = sprite;
        };
    }

    private void UpdateNameText(string name)
    {
        _nameText.text = name;

        // 레이아웃 갱신 (텍스트 길이에 맞춰 RectTransform 크기 업데이트)
        LayoutRebuilder.ForceRebuildLayoutImmediate(_nameTextRect);

        // 애니메이션 실행
        _marqueeAnim?.ExecuteAsync(_nameTextCanvasGroup);
    }

    private void OnClicked()
    {
        _viewModel?.OnCardClicked();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        _marqueeAnim?.Kill();
        _cardButton.onClick.RemoveListener(OnClicked);
        _viewModel = null;
    }
}