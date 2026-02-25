using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SG 무기 전용 조준선입니다.
/// 샷건의 산탄 범위를 시각적으로 표현합니다.
/// Implements Section 4.3: UI_SGCrosshair (Phase 7.1 Refactor Design)
/// </summary>
public class UI_SGCrosshair : UI_CrosshairBase
{
    [Header("SG Crosshair")]
    [SerializeField] private Image _spreadCircleImage;

    private PunchScaleUIAnimation _recoilAnim;
    private int _prevAmmo = -1;

    protected override void Awake()
    {
        base.Awake();
        if (_spreadCircleImage != null)
        {
            // SG 반동 애니메이션 설정 (1.3배까지 커지고 0.15초에 복구)
            _recoilAnim = new PunchScaleUIAnimation(_spreadCircleImage.rectTransform, Vector3.one * 1.3f, 0.15f, 1, 0.5f);
        }
    }

    protected override void BindWeaponProperties()
    {
        Bind(_viewModel.CurrentAmmo, OnAmmoChanged);
        Bind(_viewModel.MaxAmmo, max => UpdateAmmoUI(_viewModel.CurrentAmmo.Value, max));
    }

    private void OnAmmoChanged(int currentAmmo)
    {
        if (_prevAmmo != -1 && currentAmmo < _prevAmmo)
        {
            OnFire();
        }
        _prevAmmo = currentAmmo;
        UpdateAmmoUI(currentAmmo, _viewModel.MaxAmmo.Value);
    }

    protected override void OnFire()
    {
        base.OnFire();
        // DOTween 애니메이션 클래스를 통해 재생
        _recoilAnim?.ExecuteAsync();
    }
}
