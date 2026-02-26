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
    [SerializeField] private Color _coverColor = new Color(0.53f, 0.81f, 0.98f); // 하늘색
    [SerializeField] private Color _attackColor = new Color(1f, 0.3f, 0.3f);     // 빨간색
    [SerializeField] private float _fadeDuration = 0.2f;                         // 색상 전환 시간

    private NikkeStateViewModel _stateViewModel;
    private IUIAnimation _attackEntryAnim;
    private IUIAnimation _coverEntryAnim;
    private CanvasGroup _currentActiveRoot;

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
        Bind(_stateViewModel.ProfileImage, sprite =>
        {
            if (_cropImage != null) _cropImage.sprite = sprite;
        });

        // 2. HP Ratio
        Bind(_stateViewModel.HpRatio, ratio =>
        {
            if (_hpFill != null) _hpFill.fillAmount = ratio;
        });

        // 3. Attribute Code
        Bind(_stateViewModel.CodeIcon, sprite =>
        {
            if (_codeImage != null) _codeImage.sprite = sprite;
        });

        // 4. Ammo (Split)
        Bind(_stateViewModel.CurrentAmmo, ammo =>
        {
            if (_txtCurrentAmmo != null) _txtCurrentAmmo.text = ammo.ToString();
        });

        Bind(_stateViewModel.MaxAmmo, max =>
        {
            if (_txtMaxAmmo != null) _txtMaxAmmo.text = max.ToString();
        });

        // 5. State (Unified) & Gradient Color
        Bind(_stateViewModel.CurrentState, state =>
        {
            if (_gradientImage != null)
            {
                // 재장전(Reload)도 엄폐(Cover)와 동일한 하늘색으로 처리
                bool isCoverColor = (state == eNikkeState.Cover || state == eNikkeState.Reload);
                Color targetColor = isCoverColor ? _coverColor : _attackColor;

                _gradientImage.DOKill();
                _gradientImage.DOColor(targetColor, _fadeDuration).SetEase(Ease.Linear);
            }

            UpdateSlotVisualState();
        });

        // 6. Selection Focus
        Bind(_stateViewModel.IsSelected, isSelected =>
        {
            UpdateSlotVisualState();
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
            _attackEntryAnim = new UIAnimationComposite(
                new VerticalSlideFadeUIAnimation(_attackRootCG, animDuration, offsetY, Ease.OutCubic),
                new ScaleUIAnimation(_attackRootCG.GetComponent<RectTransform>(), startScale, Vector3.one, animDuration, Ease.OutBack)
            );
        }

        if (_coverRootCG != null)
        {
            _coverEntryAnim = new UIAnimationComposite(
                new VerticalSlideFadeUIAnimation(_coverRootCG, animDuration, offsetY, Ease.OutCubic),
                new ScaleUIAnimation(_coverRootCG.GetComponent<RectTransform>(), startScale, Vector3.one, animDuration, Ease.OutBack)
            );
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
        SetAliveElementsActive(true);
        TransitionToRoot(null); // Manual: 모든 루트 숨김
        if (_deadOverlay != null) _deadOverlay.SetActive(false);

        SlideNikkeImage(_manualPos);
    }

    private void ApplyAutoState()
    {
        eNikkeState state = _stateViewModel.CurrentState.Value;
        bool isReloading = (state == eNikkeState.Reload);
        bool isCovering = (state == eNikkeState.Cover);

        SetAliveElementsActive(true);

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

        if (_deadOverlay != null) _deadOverlay.SetActive(false);

        SlideNikkeImage(_autoPos);
    }

    private void ApplyDeadState()
    {
        SetAliveElementsActive(false);
        TransitionToRoot(null);
        if (_deadOverlay != null) _deadOverlay.SetActive(true);
    }

    private void TransitionToRoot(CanvasGroup targetCG, IUIAnimation entryAnim = null)
    {
        // 1. 타겟과 현재가 같으면 중단 (단, 텍스트 변경 등은 위에서 처리됨)
        if (_currentActiveRoot == targetCG) return;

        // 2. 기존 루트 즉시 비활성화 (진행 중인 트윈 Kill 후 퇴장)
        if (_currentActiveRoot != null)
        {
            _currentActiveRoot.DOKill();
            _currentActiveRoot.GetComponent<RectTransform>().DOKill();
            _currentActiveRoot.alpha = 0f;
            _currentActiveRoot.gameObject.SetActive(false);
        }

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

    private void SetAliveElementsActive(bool isActive)
    {
        if (_aliveElements == null) return;

        for (int i = 0; i < _aliveElements.Length; i++)
        {
            if (_aliveElements[i] != null)
                _aliveElements[i].SetActive(isActive);
        }
    }

    private void SlideNikkeImage(Vector2 targetPos)
    {
        if (_nikkeImageRT == null) return;

        _nikkeImageRT.DOKill();
        _nikkeImageRT.DOAnchorPos(targetPos, _slideDuration).SetEase(Ease.OutQuad);
    }
}
