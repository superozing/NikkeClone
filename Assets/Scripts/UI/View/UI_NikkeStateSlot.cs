using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UI;

/// <summary>
/// 개별 니케의 상태를 간단하게 표시하는 UI 슬롯입니다.
/// </summary>
public class UI_NikkeStateSlot : UI_View
{
    [Header("Components")]
    [SerializeField] private Image _cropImage;
    [SerializeField] private RectTransform _nikkeImageRT;


    [SerializeField] private Image _hpFill;
    [SerializeField] private Image _codeImage;
    [SerializeField] private TMP_Text _txtCurrentAmmo;
    [SerializeField] private TMP_Text _txtMaxAmmo;
    [SerializeField] private CanvasGroup _attackRootCG;    // 사격 UI 루트 (CanvasGroup)
    [SerializeField] private CanvasGroup _coverRootCG;     // 엄폐 UI 루트 (CanvasGroup)
    [SerializeField] private TMP_Text _txtCoverStatus;     // 엄폐/재장전 상태 텍스트
    [SerializeField] private Image _gradientImage;


    [Header("Nikke Image Position")]
    [SerializeField] private Vector2 _manualPos = Vector2.zero;  // 중앙 (전체 표시)
    [SerializeField] private Vector2 _autoPos = new Vector2(0, -80f); // 하단 (얼굴만)
    [SerializeField] private float _slideDuration = 0.25f;


    [Header("Death")]
    [SerializeField] private GameObject _deadOverlay;      // 사망 시 표시할 오버레이 이미지
    [SerializeField] private GameObject[] _aliveElements;  // 사망 시 비활성화할 요소들


    [Header("Visual Settings")]
    [SerializeField] private Color _coverColor = new Color(0.53f, 0.81f, 0.98f);
    [SerializeField] private Color _defaultColor = Color.black;
    [SerializeField] private float _fadeDuration = 0.2f;

    private NikkeStateViewModel _stateViewModel;
    private IUIAnimation _attackEntryAnim;
    private IUIAnimation _coverEntryAnim;
    private CanvasGroup _currentActiveRoot;

    // 연출 중 전환 시 RectTransform이 중간값을 유지하는 것을 방지하기 위해
    // 두 루트의 초기 anchoredPosition / localScale을 캐싱해둔다.
    private Vector2 _attackRootOriginalPos;
    private Vector2 _coverRootOriginalPos;
    private Vector3 _attackRootOriginalScale;
    private Vector3 _coverRootOriginalScale;

    public override void SetViewModel(ViewModelBase viewModel)
    {
        base.SetViewModel(viewModel);
        _stateViewModel = viewModel as NikkeStateViewModel;

        if (_stateViewModel == null)
        {
            if (viewModel != null)
                Debug.LogError($"[UI_NikkeStateSlot] Invalid ViewModel Type: {viewModel.GetType()}");
            return;
        }

        // Bind ReactiveProperties

        // 1. Crop Image
        Bind(_stateViewModel.ProfileImage, sprite => _cropImage.sprite = sprite);

        // 2. HP Ratio
        Bind(_stateViewModel.HpRatio, ratio => _hpFill.fillAmount = ratio);

        // 3. Attribute Code
        Bind(_stateViewModel.CodeIcon, sprite => _codeImage.sprite = sprite);

        // 4. Ammo (Split)
        Bind(_stateViewModel.CurrentAmmo, ammo => _txtCurrentAmmo.text = ammo.ToString());
        Bind(_stateViewModel.MaxAmmo, max => _txtMaxAmmo.text = max.ToString());

        // 5. State (Unified)
        Bind(_stateViewModel.CurrentState, state => UpdateSlotVisualState());

        // 6. Selection Focus
        Bind(_stateViewModel.IsSelected, isSelected => UpdateSlotVisualState());

        // 7. Global Cover Gradient Color
        Bind(_stateViewModel.IsGlobalCover, isGlobalCover =>
        {
            if (_gradientImage != null)
            {
                Color targetColor = isGlobalCover ? _coverColor : _defaultColor;

                _gradientImage.DOKill();
                _gradientImage.DOColor(targetColor, _fadeDuration).SetEase(Ease.Linear);
            }
        });

        // Initialize Animations
        InitEntryAnimations();
    }

    private void InitEntryAnimations()
    {
        float animDuration = 0.25f;
        float offsetY = 15f;
        Vector3 startScale = Vector3.one * 0.95f;

        if (_attackRootCG != null)
        {
            var attackRT = _attackRootCG.GetComponent<RectTransform>();
            // 등장 연출 기준점 캐싱: DOKill 후 리셋에 사용
            _attackRootOriginalPos = attackRT.anchoredPosition;
            _attackRootOriginalScale = attackRT.localScale;

            _attackEntryAnim = new UIAnimationComposite(
                new VerticalSlideFadeUIAnimation(_attackRootCG, animDuration, offsetY, Ease.OutCubic),
                new ScaleUIAnimation(attackRT, startScale, Vector3.one, animDuration, Ease.OutBack)
            );

            // Prefab 기본 상태와 무관하게 초기 비활성화
            _attackRootCG.alpha = 0f;
            _attackRootCG.gameObject.SetActive(false);
        }

        if (_coverRootCG != null)
        {
            var coverRT = _coverRootCG.GetComponent<RectTransform>();
            // 등장 연출 기준점 캐싱: DOKill 후 리셋에 사용
            _coverRootOriginalPos = coverRT.anchoredPosition;
            _coverRootOriginalScale = coverRT.localScale;

            _coverEntryAnim = new UIAnimationComposite(
                new VerticalSlideFadeUIAnimation(_coverRootCG, animDuration, offsetY, Ease.OutCubic),
                new ScaleUIAnimation(coverRT, startScale, Vector3.one, animDuration, Ease.OutBack)
            );

            // Prefab 기본 상태와 무관하게 초기 비활성화
            _coverRootCG.alpha = 0f;
            _coverRootCG.gameObject.SetActive(false);
        }
    }

    private void UpdateSlotVisualState()
    {
        if (_stateViewModel == null) return;

        eNikkeState state = _stateViewModel.CurrentState.Value;
        bool isSelected = _stateViewModel.IsSelected.Value;

        if (state == eNikkeState.Dead)
        {
            ApplyDeadState();
        }
        else if (isSelected)
        {
            ApplyManualState();
        }
        else
        {
            ApplyAutoState();
        }
    }

    private void ApplyManualState()
    {
        ApplyVisualGroupActive(true);
        TransitionToRoot(null); // Manual: 모든 루트 숨김
        SlideNikkeImage(_manualPos);
    }

    private void ApplyAutoState()
    {
        eNikkeState state = _stateViewModel.CurrentState.Value;
        bool isReloading = (state == eNikkeState.Reload);
        bool isCovering = (state == eNikkeState.Cover);

        ApplyVisualGroupActive(true);

        if (isReloading || isCovering)
        {
            if (_txtCoverStatus != null)
                _txtCoverStatus.text = isReloading ? "RELOADING" : "COVERED";

            TransitionToRoot(_coverRootCG, _coverEntryAnim);
        }
        else
        {
            TransitionToRoot(_attackRootCG, _attackEntryAnim);
        }

        SlideNikkeImage(_autoPos);
    }

    private void ApplyDeadState()
    {
        ApplyVisualGroupActive(false);
        TransitionToRoot(null);
    }

    private void TransitionToRoot(CanvasGroup targetCG, IUIAnimation entryAnim = null)
    {
        // 1. 타겟과 현재가 같으면 중단 (단, 텍스트 변경 등은 위에서 처리됨)
        if (_currentActiveRoot == targetCG) return;

        // 2. 기존 루트 즉시 비활성화 및 리셋
        CleanUpRoot(_currentActiveRoot);

        _currentActiveRoot = targetCG;

        // 3. 새 루트 활성화 및 등장 연출
        if (_currentActiveRoot != null)
        {
            _currentActiveRoot.gameObject.SetActive(true);
            if (entryAnim != null)
            {
                _ = entryAnim.ExecuteAsync();
            }
            else
            {
                _currentActiveRoot.alpha = 1f;
            }
        }
    }

    private void CleanUpRoot(CanvasGroup cg)
    {
        if (cg == null) return;

        cg.DOKill();

        // 트윈 중단 후 RectTransform이 중간값을 유지하는 것을 방지.
        var rt = cg.GetComponent<RectTransform>();
        rt.DOKill();

        if (cg == _attackRootCG)
        {
            rt.anchoredPosition = _attackRootOriginalPos;
            rt.localScale = _attackRootOriginalScale;
        }
        else if (cg == _coverRootCG)
        {
            rt.anchoredPosition = _coverRootOriginalPos;
            rt.localScale = _coverRootOriginalScale;
        }

        cg.alpha = 0f;
        cg.gameObject.SetActive(false);
    }

    /// <summary>
    /// 니케의 생존 상태에 따른 비주얼 그룹(오버레이, 생존 요소들)을 통합 관리합니다.
    /// </summary>
    private void ApplyVisualGroupActive(bool isAlive)
    {
        if (_deadOverlay != null) _deadOverlay.SetActive(!isAlive);
        if (_aliveElements == null) return;

        for (int i = 0; i < _aliveElements.Length; i++)
        {
            if (_aliveElements[i] != null)
                _aliveElements[i].SetActive(isAlive);
        }
    }

    private void SlideNikkeImage(Vector2 targetPos)
    {
        _nikkeImageRT.DOKill();
        _nikkeImageRT.DOAnchorPos(targetPos, _slideDuration).SetEase(Ease.OutQuad);
    }
}
