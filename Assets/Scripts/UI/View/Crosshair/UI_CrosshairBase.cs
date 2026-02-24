using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UI;

/// <summary>
/// 조준선 UI의 추상 기반 클래스입니다. UI_View를 상속합니다.
/// 포인터 추적, ViewModel 바인딩 프레임워크, OnEnable/OnDisable 구독 패턴을 제공합니다.
/// 하위 클래스는 BindWeaponProperties()를 구현하여 필요한 ReactiveProperty만 선택적으로 구독합니다.
/// Implements Section 3: Crosshair View Layer (Phase 7.1 Refactor v2 Design)
/// </summary>
public abstract class UI_CrosshairBase : UI_View
{
    protected CrosshairViewModel _viewModel;
    protected RectTransform _rectTransform;

    [Header("Ammo Shared UI")]
    [SerializeField] protected TMPro.TMP_Text _ammoText;
    [SerializeField] protected UnityEngine.UI.Image _ammoFillImage;

    [Header("Color Feedback")]
    [SerializeField] protected UnityEngine.UI.Graphic[] _crosshairGraphics;

    private Camera _uiCamera;
    private RectTransform _parentRect;

    protected static readonly Color _advantageColor = new Color(0.4f, 0.8f, 1f); // 하늘색
    protected static readonly Color _defaultColor = Color.white;

    protected override void Awake()
    {
        base.Awake();
        _rectTransform = GetComponent<RectTransform>();

        // 최적화: 렌더 카메라를 매니저에서 직접 획득 (UI_Camera) 
        _uiCamera = Managers.UI.UICamera;
    }

    /// <summary>
    /// 공유 CrosshairViewModel을 바인딩합니다.
    /// UIManager.ShowAsync()에서 호출됩니다.
    /// Caller: UIManager.ShowAsync → UI_View.SetViewModel
    /// </summary>
    public override void SetViewModel(ViewModelBase viewModel)
    {
        _viewModel = viewModel as CrosshairViewModel;

        if (_viewModel == null && viewModel != null)
        {
            Debug.LogError($"[{GetType().Name}] 올바르지 않은 뷰모델 타입입니다: {viewModel.GetType()}");
            return;
        }

        base.SetViewModel(viewModel);

        // GO가 이미 활성화 상태이면 즉시 바인딩 (OnEnable이 먼저 호출된 경우 대비)
        if (_viewModel != null && gameObject.activeInHierarchy)
        {
            BindWeaponProperties();
            if (_viewModel.ActiveWeapon.Value != null)
            {
                _viewModel.ActiveWeapon.Value.IsInPreferredZone.OnValueChanged += OnPreferredZoneChanged;
            }
        }
    }

    /// <summary>
    /// 활성화 시 ViewModel의 ReactiveProperty를 구독합니다.
    /// CombatCrosshairSystem이 SetActive(true) 호출 시 트리거됩니다.
    /// </summary>
    protected virtual void OnEnable()
    {
        if (_viewModel != null)
        {
            BindWeaponProperties();
            if (_viewModel.ActiveWeapon.Value != null)
            {
                _viewModel.ActiveWeapon.Value.IsInPreferredZone.OnValueChanged += OnPreferredZoneChanged;
            }
        }
    }

    /// <summary>
    /// 비활성화 시 모든 구독을 해제합니다.
    /// CombatCrosshairSystem이 SetActive(false) 호출 시 트리거됩니다.
    /// </summary>
    protected virtual void OnDisable()
    {
        if (_viewModel?.ActiveWeapon.Value != null)
        {
            _viewModel.ActiveWeapon.Value.IsInPreferredZone.OnValueChanged -= OnPreferredZoneChanged;
        }
        UnbindAll();
    }



    /// <summary>
    /// 매 프레임 마우스(수동) 또는 타겟 Screen 좌표(자동)를 추적하여 RectTransform을 이동합니다.
    /// ScreenSpaceCamera 렌더 모드 대응 완료.
    /// </summary>
    protected virtual void Update()
    {
        if (_viewModel != null)
        {
            Vector2 targetScreenPos = Vector2.zero;

            if (_viewModel.IsAutoMode.Value)
            {
                // [변경] 즉시 할당이 아닌, 이전 TargetPosition을 기반으로 보간 
                Vector2 currentPos = _viewModel.TargetPosition.Value;
                Vector2 targetPos = _viewModel.AutoTargetScreenPosition.Value;

                // 타겟을 잃었거나 좌표가 초기화된 상태일 경우 화면 중앙 등 대기 위치로 보정
                if (targetPos == Vector2.zero)
                {
                    targetPos = new Vector2(Screen.width / 2f, Screen.height / 2f);
                }

                // 15f는 보간 속도, 필요에 따라 상수로 분리 가능
                targetScreenPos = Vector2.Lerp(currentPos, targetPos, Time.deltaTime * 15f);
            }
            else if (Pointer.current != null)
            {
                targetScreenPos = Pointer.current.position.ReadValue();
            }

            // [변경] 수동/자동 상관없이 항상 ViewModel에 현재 (보간된) 화면 픽셀 좌표(마우스 위치) 전달 
            _viewModel.TargetPosition.Value = targetScreenPos;

            // 오브젝트 풀링 환경 대응: Awake 시점에는 캔버스 하위가 아닐 수 있으므로 동적 할당
            if (_parentRect == null && _rectTransform.parent != null)
            {
                _parentRect = _rectTransform.parent as RectTransform;
            }

            // UI 렌더링 좌표계 변환 (Screen -> Local)
            if (_parentRect != null && _uiCamera != null)
            {
                // RectTransformUtility를 사용하여 스크린 좌표를 캔버스(부모) 내의 로컬 포지션으로 변환
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, targetScreenPos, _uiCamera, out Vector2 localPoint))
                {
                    // UIManager의 UI카메라가 "Perspective"이므로 Z값이 0이 아니면 카메라에 가까워져서(원근감)
                    // 엄청나게 커지고, 마우스 위치와 실제 렌더링 위치 사이의 오차(Parallax)가 발생합니다. (우상단 쏠림 현상의 주 원인)
                    // 반드시 Z 위치를 0으로 강제하여 캔버스 평면과 완전히 밀착시켜야 합니다.
                    _rectTransform.anchoredPosition3D = new Vector3(localPoint.x, localPoint.y, 0f);
                }
            }
            else
            {
                // Fallback
                _rectTransform.position = targetScreenPos;
            }
        }
    }

    /// <summary>
    /// 하위 클래스가 필요한 ViewModel ReactiveProperty만 선택적으로 구독합니다.
    /// OnEnable 및 SetViewModel에서 호출됩니다.
    /// </summary>
    protected abstract void BindWeaponProperties();

    /// <summary>
    /// 무기 전환 등에 의해 조준선이 활성화될 때 호출됩니다.
    /// Caller: CombatCrosshairSystem.SwitchCrosshair()
    /// </summary>
    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 무기 전환 등에 의해 조준선이 비활성화될 때 호출됩니다.
    /// Caller: CombatCrosshairSystem.SwitchCrosshair()
    /// </summary>
    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }

    // ViewModel 이벤트 바인딩용 가상 메서드 (파생 클래스 오버라이드용)
    protected virtual void OnFire() { }
    protected virtual void OnReloadStateChanged(bool isReloading) { }
    protected virtual void OnChargeRatioChanged(float ratio) { }

    protected virtual void UpdateAmmoUI(int current, int max)
    {
        float ratio = max > 0 ? (float)current / max : 0f;
        bool isLowAmmo = ratio < 0.4f;
        Color targetColor = isLowAmmo ? Color.red : Color.white;

        if (_ammoText != null)
        {
            _ammoText.text = $"{current:D3}";
            _ammoText.color = targetColor;
        }

        if (_ammoFillImage != null)
        {
            _ammoFillImage.fillAmount = ratio;
            _ammoFillImage.color = targetColor;
        }
    }

    protected virtual void OnPreferredZoneChanged(bool isPreferred)
    {
        Color targetColor = isPreferred ? _advantageColor : _defaultColor;

        if (_crosshairGraphics != null)
        {
            foreach (var graphic in _crosshairGraphics)
            {
                if (graphic != null)
                {
                    graphic.color = targetColor;
                }
            }
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _viewModel = null;
    }
}
