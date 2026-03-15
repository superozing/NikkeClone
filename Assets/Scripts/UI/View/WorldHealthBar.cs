using UnityEngine;
using UnityEngine.UI;
using UI;

/// <summary>
/// 엔터티 상단에 표시되는 월드 공간 체력바 뷰입니다.
/// </summary>
public class WorldHealthBar : UI_View
{
    [Header("UI Components")]
    [SerializeField] private Image _currentFill;
    [SerializeField] private Image _delayedFill;

    [Header("Settings")]
    [SerializeField] private float _lerpSpeed = 5f;

    private EntityHealthViewModel _healthViewModel;
    private Transform _targetAnchor;
    private Camera _mainCamera;
    private RectTransform _parentRect;
    private RectTransform _rectTransform;

    protected override void Awake()
    {
        base.Awake();
        _rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// 추적 대상과 필요한 참조를 설정합니다.
    /// </summary>
    public void SetTrackingTarget(Transform targetAnchor, Camera mainCamera, RectTransform parentRect)
    {
        _targetAnchor = targetAnchor;
        _mainCamera = mainCamera;
        _parentRect = parentRect;
    }

    public override void SetViewModel(ViewModelBase viewModel)
    {
        base.SetViewModel(viewModel);
        _healthViewModel = viewModel as EntityHealthViewModel;

        if (_healthViewModel != null)
        {
            // 프로젝트 표준 Bind 패턴 사용 (UI_View.Bind)
            Bind(_healthViewModel.HpRatio, ratio =>
            {
                if (_currentFill != null) _currentFill.fillAmount = ratio;
            });

            Bind(_healthViewModel.IsDead, isDead =>
            {
                if (isDead) gameObject.SetActive(false);
            });
        }
    }

    private void LateUpdate()
    {
        if (_targetAnchor == null || _mainCamera == null || _parentRect == null) return;

        // 1. 월드 좌표를 스크린 좌표로 변환
        Vector3 screenPos = _mainCamera.WorldToScreenPoint(_targetAnchor.position);

        // 2. 카메라 뒤에 있는 경우 숨김
        if (screenPos.z < 0)
        {
            if (_canvasGroup != null) _canvasGroup.alpha = 0f;
            return;
        }

        // 3. 스크린 좌표를 UI 로컬 좌표로 변환
        // [핵심] ScreenSpace-Camera 모드이므로 UI Camera를 반드시 전달하여 정확한 로컬 좌표를 계산합니다.
        Camera uiCamera = Managers.UI.UICamera;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, (Vector2)screenPos, uiCamera, out Vector2 localPoint))
        {
            // [핵심] Perspective UI Camera의 Z-Parallax 방지 - Z값을 0으로 강제하여 캔버스 평면과 밀착시킴
            _rectTransform.anchoredPosition3D = new Vector3(localPoint.x, localPoint.y, 0f);
            if (_canvasGroup != null) _canvasGroup.alpha = 1f;
        }

        // 4. 지연된 체력바(Delayed Bar) Lerp 연출
        if (_delayedFill != null && _healthViewModel != null)
        {
            _delayedFill.fillAmount = Mathf.Lerp(_delayedFill.fillAmount, _healthViewModel.HpRatio.Value, _lerpSpeed * Time.deltaTime);
        }
    }
}
